using PTHLogger;
using PTHPlayer.Controllers.Enums;
using PTHPlayer.Controllers.Listeners;
using PTHPlayer.DataStorage.Service;
using PTHPlayer.Event.Listeners;
using PTHPlayer.HTSP;
using PTHPlayer.Player.Enums;
using PTHPlayer.Player.Models;
using PTHPlayer.Subtitles.Player;
using PTHPlayer.VideoPlayer;
using PTHPlayer.VideoPlayer.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace PTHPlayer.Controllers
{
    public class PlayerController : IPlayerListener
    {
        public event EventHandler SubscriptionStartEvent;
        public event EventHandler SubscriptionCompletedEvent;
        public event EventHandler<PlayerStateChangeEventArgs> PlayerStateChange;

        readonly DataService DataStorage;
        readonly HTSPService HTSPClient;
        readonly IEventListener EventNotificationListener;

        PlayerService PlayerService { get; }
        SubtitlePlayer SubtitlePlayer { get; }

        int SubscriptionId = -1;

        Task SubscriptionTask;
        Task UnsubscriptionTask;

        TaskCompletionSource<HTSMessage> SubscriptionStart;
        TaskCompletionSource<HTSMessage> SubscriptionSkip;
        TaskCompletionSource<HTSMessage> SubscriptionStop;

        TaskCompletionSource<bool> CalculationFps;
        TaskCompletionSource<bool> CalculationSampleRate;

        CancellationTokenSource SubscriptionTaskCancellationToken = new CancellationTokenSource();
        CancellationTokenSource PlayerSubscriptionCancellationToken = new CancellationTokenSource();

        SubscriptionStatus Status = SubscriptionStatus.New;

        private readonly ILogger Logger = LoggerManager.GetInstance().GetLogger("PTHPlayer.Controllers");

        public PlayerController(DataService dataStorage, HTSPService hTSPClient, IEventListener eventNotificationListener)
        {
            DataStorage = dataStorage;
            HTSPClient = hTSPClient;
            EventNotificationListener = eventNotificationListener;

            PlayerService = new PlayerService();
            SubtitlePlayer = new SubtitlePlayer(PlayerService);

            PlayerService.PlayerStateChange += DelegatePlayerStateChange;
            PlayerService.PlayerError += DelegatePlayerError;

            HTSPClient.ConnectionStateChange += HTSPClient_ConnectionStateChange;
        }

        private void HTSPClient_ConnectionStateChange(object sender, HTSPConnectionStateChangeArgs e)
        {
            switch (e.ConnectionChangeState)
            {
                case ConnectionState.Disconnected:
                    {
                        Stop();
                        break;
                    }
                case ConnectionState.Authenticated:
                    {
                        if (DataStorage.SelectedChannelId != -1)
                        {
                            Subscription(DataStorage.SelectedChannelId);
                        }
                        break;
                    }
            }
        }

        public void SetSubtitleDisplay(Image image)
        {
            SubtitlePlayer.SetDisplay(image);
        }

        public void Stop()
        {
            try
            {
                SubtitlePlayer.Stop();
                PlayerService.SubscriptionStop();
                UnSubscribe();
                Status = SubscriptionStatus.New;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
        }

        public void Subscription(int channelId)
        {
            Logger.Info("Subscription Call");
            SubscriptionStartEvent?.Invoke(this, new EventArgs());

            SubscriptionTaskCancellationToken.Cancel();

            SubscriptionTaskCancellationToken = new CancellationTokenSource();
            Task.Run(() =>
            {
                try
                {
                    if (SubscriptionTask != null && !SubscriptionTask.IsCompleted)
                    {
                        Logger.Info("Subscription wait");
                        SubscriptionTask.Wait(SubscriptionTaskCancellationToken.Token);
                        Logger.Info("Subscription wait Resume");
                    }

                    if (UnsubscriptionTask != null && !UnsubscriptionTask.IsCompleted)
                    {
                        Logger.Info("Unsubscription wait");
                        UnsubscriptionTask.Wait(SubscriptionTaskCancellationToken.Token);
                        Logger.Info("Unsubscription wait Resume");
                    } else if(SubscriptionId != -1)
                    {
                        Logger.Info("Unsubscription start and wait");
                        UnSubscribe();
                        UnsubscriptionTask.Wait(SubscriptionTaskCancellationToken.Token);
                        Logger.Info("Unsubscription wait Resume");
                    }

                    if (Status == SubscriptionStatus.New)
                    {
                        PlayerSubscriptionCancellationToken.Cancel();
                        Logger.Info("Player Subscription token Cancelled");
                    }
                    PlayerSubscriptionCancellationToken = new CancellationTokenSource();

                    Logger.Info("Player Subscription Started");
                    SubscriptionTask = Task.Run(async () => await SubscriptionProcess(channelId));

                }
                catch (OperationCanceledException)
                {
                    Logger.Info("Subscription Task token Cancelled");
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.Message);
                }
            });

            Logger.Info("Subscription Call Done");
        }

        async Task SubscriptionProcess(int channelId)
        {
            SubscriptionStart = new TaskCompletionSource<HTSMessage>();
            SubscriptionSkip = new TaskCompletionSource<HTSMessage>();

            try
            {
                Status = SubscriptionStatus.New;
                var subscriptionId = HTSPClient.Subscribe(channelId);
                SubscriptionId = subscriptionId;

                Task.WaitAny(new[] { SubscriptionStart.Task, SubscriptionSkip.Task }, 120000, PlayerSubscriptionCancellationToken.Token);

                //Flow one SubscriptionStart Success
                if (SubscriptionStart.Task.IsCompleted)
                {
                    Status = SubscriptionStatus.Submitted;

                    CalculationFps = new TaskCompletionSource<bool>();
                    CalculationSampleRate = new TaskCompletionSource<bool>();

                    PlayerService.Subscription(SubscriptionStart.Task.Result);

                    var results = Task.WaitAll(new[] { CalculationFps.Task, CalculationSampleRate.Task }, 5000, PlayerSubscriptionCancellationToken.Token);
                    if (results)
                    {

                        Status = SubscriptionStatus.WaitForPlay;

                        var playerPrepare = PlayerService.PreparePlayer();

                        await PlayerService.PacketReady();
                        Status = SubscriptionStatus.Play;

                        //Wait block packet for a while this do scratch on start -- maybe
                        playerPrepare.Wait();
                        SubtitlePlayer.Start();
                    }
                    else
                    {

                    }
                }

                //Flow one SubscriptionSkip Nothing to do reset state
                else if (SubscriptionSkip.Task.IsCompleted)
                {
                    Status = SubscriptionStatus.New;
                    DelegatePlayerStateChange(this, new PlayerStateChangeEventArgs { State = PlayerState.Stop });
                }
            }
            catch (OperationCanceledException)
            {
                Logger.Info("Player Subscription Cancel Exception");
                DelegatePlayerStateChange(this, new PlayerStateChangeEventArgs { State = PlayerState.Stop });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                DelegatePlayerStateChange(this, new PlayerStateChangeEventArgs { State = PlayerState.Stop });
            }
            Logger.Info("Subscription Completed");
            SubscriptionCompletedEvent?.Invoke(this, new EventArgs());
        }

        public void UnSubscribe(bool forceStop = false, bool withPlayer = true)
        {
            try
            {
                Logger.Info("Unsubscribe Call with Id: " + SubscriptionId);
                if (withPlayer)
                {
                    PlayerSubscriptionCancellationToken.Cancel();
                }
                if (UnsubscriptionTask == null || UnsubscriptionTask.IsCompleted)
                {
                    UnsubscriptionTask = Task.Run(() => UnSubscribeProcess(forceStop));
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
        }

        void UnSubscribeProcess(bool forceStop = false)
        {
            try
            {
                if (SubscriptionId == -1)
                {
                    return;
                }

                SubscriptionStop = new TaskCompletionSource<HTSMessage>();

                switch (Status)
                {
                    //New is before send maybe send and not Submitted need other check
                    case SubscriptionStatus.New:
                        {
                            PlayerSubscriptionCancellationToken.Cancel();
                            HTSPClient.UnSubscribe(SubscriptionId);
                            return;
                        }
                    //Received Sub from server they need to be released
                    case SubscriptionStatus.Submitted:
                        {
                            PlayerSubscriptionCancellationToken.Cancel();
                            HTSPClient.UnSubscribe(SubscriptionId);
                            break;
                        }
                        //This state need to be player prepared cannot be stoped
                    case SubscriptionStatus.WaitForPlay:
                        {
                            Thread.Sleep(1000);
                            UnSubscribeProcess(forceStop);
                            return;
                        }
                        //Play need completed shut down
                    case SubscriptionStatus.Play:
                        {
                            SubtitlePlayer.Stop();
                            PlayerService.SubscriptionStop();
                            HTSPClient.UnSubscribe(SubscriptionId);
                            break;
                        }
                }

                //If not in first state need to wait for unsub response
                if (Status != SubscriptionStatus.New || !forceStop)
                {
                    if(!SubscriptionStop.Task.Wait(10000))
                    {
                        Logger.Error("UnSubscription TimeOut maybe some problem");
                    }
                }

                SubscriptionId = -1;
            }
            catch (Exception ex)
            {
                //If all fail need to reset all to usable state
                Logger.Error(ex.Message);
            }
            Logger.Info("Unsubscription Completed");
        }

        public ICollection<AudioConfigModel> GetAudioConfigs()
        {
            return PlayerService.GetAudioConfigs();
        }

        public AudioConfigModel GetSelectedAudioConfig()
        {
            return PlayerService.GetSelectedAudioConfig();
        }

        public ICollection<SubtitleConfigModel> GetSubtitleConfigs()
        {
            return PlayerService.GetSubtitleConfigs();
        }
        public SubtitleConfigModel GetSelectedSubtitleConfig()
        {
            return PlayerService.GetSelectedSubtitleConfig();
        }

        public void ChangeAudioTrack(int indexId)
        {
            Task.Run(async () =>
            {
                Status = SubscriptionStatus.Pause;

                PlayerService.ChangeAudioTrack(indexId);

                CalculationFps = new TaskCompletionSource<bool>();
                CalculationSampleRate = new TaskCompletionSource<bool>();

                var results = Task.WaitAll(new[] { CalculationFps.Task, CalculationSampleRate.Task }, 5000);
                if (results)
                {
                    var playerPrepare = PlayerService.PreparePlayer();

                    await PlayerService.PacketReady();
                    Status = SubscriptionStatus.Play;

                    playerPrepare.Wait();
                }
            });
        }

        public void OnSubscriptionStart(HTSMessage message)
        {
            if (SubscriptionStart != null && !SubscriptionStart.Task.IsCompleted)
                SubscriptionStart.SetResult(message);
        }

        public void OnSubscriptionStop(HTSMessage message)
        {
            if (SubscriptionStop != null && !SubscriptionStop.Task.IsCompleted)
                SubscriptionStop.SetResult(message);
        }

        public void OnSubscriptionSkip(HTSMessage message)
        {
            if (SubscriptionSkip != null && !SubscriptionSkip.Task.IsCompleted)
                SubscriptionSkip.SetResult(message);
        }

        public void OnMuxPkt(HTSMessage message)
        {
            try
            {
                var subscriptionId = message.getInt("subscriptionId");
                if (subscriptionId != SubscriptionId)
                {
                    return;
                }

                switch (Status)
                {
                    case SubscriptionStatus.Pause:
                    case SubscriptionStatus.WaitForPlay:
                    case SubscriptionStatus.Submitted:
                        {
                            PlayerService.PushToBuffer(message);
                            if (!CalculationFps.Task.IsCompleted)
                                PlayerService.TryCalculateFps(message, CalculationFps);
                            if (!CalculationSampleRate.Task.IsCompleted)
                                PlayerService.TryCalculateSampleRate(message, CalculationSampleRate);
                            break;
                        }
                    case SubscriptionStatus.Play:
                        {
                            PlayerService.Packet(message);
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
        }

        public bool GetSubtitleEnabled()
        {
            return PlayerService.SubtitleEnabled();
        }

        public void EnableSubtitleTrack(bool isEnabled, int index = -1)
        {
            PlayerService.EnableSubtitleTrack(isEnabled, index);
        }

        protected void DelegatePlayerStateChange(object sender, PlayerStateChangeEventArgs e)
        {
            PlayerStateChange?.Invoke(this, e);
        }

        protected void DelegatePlayerError(object sender, PlayerErrorEventArgs e)
        {

            if (e.Type != PlayerErrorType.BufferChange)
            {
                switch (e.Type)
                {
                    case PlayerErrorType.EndOfStream:
                        {
                            EventNotificationListener.SendNotification(nameof(PlayerController), e.Type.ToString());
                            break;
                        }
                    case PlayerErrorType.DisposeError:
                        {
                            EventNotificationListener.SendNotification(nameof(PlayerController), e.Type.ToString());
                            break;
                        }
                    case PlayerErrorType.PlayerError:
                        {
                            EventNotificationListener.SendNotification(e.Type.ToString(), e.PlayerError.ToString());
                            break;
                        }
                }

            }

        }
    }
}