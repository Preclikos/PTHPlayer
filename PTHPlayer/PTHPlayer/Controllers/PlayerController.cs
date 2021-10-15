﻿using PTHPlayer.Controllers.Enums;
using PTHPlayer.Controllers.Listeners;
using PTHPlayer.HTSP;
using PTHPlayer.Models;
using PTHPlayer.Subtitles.Player;
using PTHPlayer.VideoPlayer.Models;
using PTHPlayer.VideoPlayer.Player;
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

        HTSPService HTSPClient { get; }
        PlayerService PlayerService { get; }
        SubtitlePlayer SubtitlePlayer { get; }

        private int SubscriptionId = -1;

        TaskCompletionSource<HTSMessage> SubscriptionStart;
        TaskCompletionSource<HTSMessage> SubscriptionSkip;
        TaskCompletionSource<HTSMessage> SubscriptionStop;

        TaskCompletionSource<bool> CalculationFps;
        TaskCompletionSource<bool> CalculationSampleRate;

        CancellationTokenSource CancellationTokenSrc = new CancellationTokenSource();

        SubscriptionStatus Status = SubscriptionStatus.New;

        public PlayerController(HTSPService hTSPClient)
        {
            HTSPClient = hTSPClient;

            PlayerService = new PlayerService();
            SubtitlePlayer = new SubtitlePlayer(PlayerService);

            PlayerService.PlayerStateChange += DelegatePlayerStateChange;
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

             Task.Run(() => _= SubscriptionProcess(channelId));
        }

        async Task SubscriptionProcess(int channelId)
        {
            SubscriptionStart = new TaskCompletionSource<HTSMessage>();
            SubscriptionSkip = new TaskCompletionSource<HTSMessage>();

            UnSubscribe();

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
                else
                {
                    UnSubscribe();
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
            SubscriptionStop = new TaskCompletionSource<HTSMessage>();
            HTSPClient.UnSubscribe(SubscriptionId);
            if (Status != SubscriptionStatus.New || forceStop)
            {

                if (!forceStop)
                {
                    SubscriptionStop.Task.Wait(2000);
                }
                SubtitlePlayer.Stop();
                PlayerService.SubscriptionStop();

            }

        }

        public ICollection<AudioConfigModel> GetAudioConfigs()
        {
            return PlayerService.GetAudioConfigs();
        }

        public ICollection<SubtitleConfigModel> GetSubtitleConfigs()
        {
            return PlayerService.GetSubtitleConfigs();
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
            switch (Status)
            {
                case SubscriptionStatus.Pause:
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
    }
}