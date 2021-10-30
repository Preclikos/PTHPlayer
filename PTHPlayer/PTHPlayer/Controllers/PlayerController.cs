using PTHPlayer.Controllers.Enums;
using PTHPlayer.Controllers.Listeners;
using PTHPlayer.DataStorage.Service;
using PTHPlayer.Event.Enums;
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
        public event EventHandler<PlayerStateChangeEventArgs> PlayerStateChange;

        DataService DataStorage;

        HTSPService HTSPClient;
        IEventListener EventNotificationListener;

        PlayerService PlayerService { get; }
        SubtitlePlayer SubtitlePlayer { get; }

        int SubscriptionId = -1;

        TaskCompletionSource<HTSMessage> SubscriptionStart;
        TaskCompletionSource<HTSMessage> SubscriptionSkip;
        TaskCompletionSource<HTSMessage> SubscriptionStop;

        TaskCompletionSource<bool> CalculationFps;
        TaskCompletionSource<bool> CalculationSampleRate;

        CancellationTokenSource CancellationTokenSrc = new CancellationTokenSource();

        SubscriptionStatus Status = SubscriptionStatus.New;

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
            switch(e.ConnectionChangeState)
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
            SubtitlePlayer.Stop();
            PlayerService.SubscriptionStop();
            Status = SubscriptionStatus.New;

        }

        public void Subscription(int channelId)
        {

            if (Status == SubscriptionStatus.New)
            {
                CancellationTokenSrc.Cancel();
                CancellationTokenSrc = new CancellationTokenSource();
            }

            Task.Run(() => _ = SubscriptionProcess(channelId));
        }

        async Task SubscriptionProcess(int channelId)
        {
            SubscriptionStart = new TaskCompletionSource<HTSMessage>();
            SubscriptionSkip = new TaskCompletionSource<HTSMessage>();

            UnSubscribe(SubscriptionId == -1);

            var subscriptionId = HTSPClient.Subscribe(channelId);
            Status = SubscriptionStatus.New;
            SubscriptionId = subscriptionId;

            Task.WaitAny(new[] { SubscriptionStart.Task, SubscriptionSkip.Task }, 120000, CancellationTokenSrc.Token);

            //Flow one SubscriptionStart Success
            if (SubscriptionStart.Task.IsCompleted)
            {
                CalculationFps = new TaskCompletionSource<bool>();
                CalculationSampleRate = new TaskCompletionSource<bool>();

                Status = SubscriptionStatus.Submitted;
                PlayerService.Subscription(SubscriptionStart.Task.Result);

                var results = Task.WaitAll(new[] { CalculationFps.Task, CalculationSampleRate.Task }, 5000, CancellationTokenSrc.Token);
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
            }

            //Flow one SubscriptionSkip Nothing to do reset state
            else if (SubscriptionSkip.Task.IsCompleted)
            {
                Status = SubscriptionStatus.New;
            }
        }

        public void UnSubscribe(bool forceStop = false)
        {
            PlayerService.SubscriptionStop();
            SubtitlePlayer.Stop();

            SubscriptionStop = new TaskCompletionSource<HTSMessage>();
            HTSPClient.UnSubscribe(SubscriptionId);
            SubscriptionId = -1;
            if (Status != SubscriptionStatus.New || forceStop)
            {
                if (!forceStop)
                {
                    SubscriptionStop.Task.Wait(10000);
                }
            }

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
            EventHandler<PlayerStateChangeEventArgs> handler = PlayerStateChange;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected void DelegatePlayerError(object sender, PlayerErrorEventArgs e)
        {

            if (e.Type != PlayerErrorType.BufferChange)
            {
                switch(e.Type)
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