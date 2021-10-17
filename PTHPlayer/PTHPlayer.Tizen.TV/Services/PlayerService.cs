using PTHPlayer.Player.Enums;
using PTHPlayer.Player.Interfaces;
using PTHPlayer.Player.Models;
using PTHPlayer.Tizen.TV.Services;
using PTHPlayer.VideoPlayer.Models;
using System;
using System.Threading.Tasks;
using Tizen.TV.Multimedia;

[assembly: Xamarin.Forms.Dependency(typeof(PlayerService))]
namespace PTHPlayer.Tizen.TV.Services
{
    public class PlayerService : IPlayerService
    {

        ESPlayer player;
        TaskCompletionSource<bool> VideoPrepareReady = new TaskCompletionSource<bool>();
        TaskCompletionSource<bool> AudioPrepareReady = new TaskCompletionSource<bool>();
        EventHandler<PlayerErrorEventArgs> ErrorEvent;

        public void Init(EventHandler<PlayerErrorEventArgs> errorEvent)
        {

            ErrorEvent = errorEvent;
            if (player != null)
            {
                return;
            }
            player = new ESPlayer();

            player.Open();

            player.SetDisplay(Program.ParentMainWindow);

            player.BufferStatusChanged += OnBufferStatusChanged;
            player.ErrorOccurred += OnError;
            player.EOSEmitted += OnEos;
        }

        protected virtual void DelegatePlayerError(PlayerErrorEventArgs e)
        {
            EventHandler<PlayerErrorEventArgs> handler = ErrorEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        void onPlayerPrepareReady(StreamType streamType)
        {
            switch (streamType)
            {
                case StreamType.Video:
                    {
                        VideoPrepareReady.SetResult(true);
                        break;
                    }
                case StreamType.Audio:
                    {
                        AudioPrepareReady.SetResult(true);
                        break;
                    }
            }
        }

        private void OnEos(object sender, EOSEventArgs eosArgs)
        {
            var errorEvent = new PlayerErrorEventArgs
            {
                Type = PlayerErrorType.EndOfStream,
                ErrorMessage = "EosError"
            };
            DelegatePlayerError(errorEvent);
        }

        private void OnError(object sender, ErrorEventArgs errorArgs)
        {
            var errorEvent = new PlayerErrorEventArgs
            {
                Type = PlayerErrorType.PlayerError,
                ErrorMessage = errorArgs.ErrorType.ToString()
            };
            DelegatePlayerError(errorEvent);
        }

        private void OnBufferStatusChanged(object sender, BufferStatusEventArgs bufferArgs)
        {
            var errorEvent = new PlayerErrorEventArgs
            {
                Type = PlayerErrorType.BufferChange,
                ErrorMessage = bufferArgs.BufferStatus.ToString()
            };
            DelegatePlayerError(errorEvent);
        }

        public TimeSpan GetPlayerTime()
        {
            if (player != null && player.GetState() == ESPlayerState.Playing)
            {
                TimeSpan outTime = TimeSpan.Zero;
                player.GetPlayingTime(out outTime);
                return outTime;
            }
            return TimeSpan.Zero;
        }

        public void SetConfig(VideoConfigModel videoConfig, AudioConfigModel audioConfig)
        {

            var stremVideoInfo = new VideoStreamInfo();

            stremVideoInfo.mimeType = (VideoMimeType)videoConfig.Codec;
            if (videoConfig.CodecData != null)
            {
                stremVideoInfo.codecData = videoConfig.CodecData;
            }
            stremVideoInfo.height = videoConfig.Heght;
            stremVideoInfo.width = videoConfig.Width;
            stremVideoInfo.num = videoConfig.Num;
            stremVideoInfo.den = videoConfig.Den;

            player.SetStream(stremVideoInfo);


            var stremAudioInfo = new AudioStreamInfo();

            stremAudioInfo.mimeType = (AudioMimeType)audioConfig.Codec;
            if (audioConfig.CodecData != null)
            {
                stremAudioInfo.codecData = audioConfig.CodecData;
            }
            stremAudioInfo.channels = audioConfig.Channels;
            stremAudioInfo.bitrate = audioConfig.BitRate;
            stremAudioInfo.sampleRate = audioConfig.SampleRate;

            player.SetStream(stremAudioInfo);
        }

        public async Task PacketReady()
        {
            await Task.WhenAll(new[] { VideoPrepareReady.Task, AudioPrepareReady.Task });
        }

        public Task PreparePlayer()
        {
            return player.PrepareAsync(onPlayerPrepareReady);
        }

        private ESPacket ParsePacket(PacketModel packet)
        {
            var esPacket = new ESPacket();
            esPacket.pts = (ulong)packet.PTS;
            esPacket.duration = (ulong)packet.Duration;
            esPacket.buffer = packet.Data;

            return esPacket;
        }

        public int VideoPacket(PacketModel response)
        {
            var packet = ParsePacket(response);
            packet.type = StreamType.Video;

            if (player != null)
            {
                return (int)player.SubmitPacket(packet);
            }
            return -1;
        }

        public int AudioPacket(PacketModel response)
        {
            var packet = ParsePacket(response);

            packet.type = StreamType.Audio;

            if (player != null)
            {
                return (int)player.SubmitPacket(packet);
            }
            return -1;
        }

        public int GetPlayerState()
        {
            if (player != null)
            {
                return (int)player.GetState();
            }
            return -1;
        }

        public void Play()
        {
            player.Start();
        }

        public void Pause()
        {
            player.Pause();
        }

        public void Resume()
        {
            player.Resume();
        }

        public void Close()
        {
            VideoPrepareReady = new TaskCompletionSource<bool>();
            AudioPrepareReady = new TaskCompletionSource<bool>();

            if (player != null)
            {
                player.BufferStatusChanged -= OnBufferStatusChanged;
                player.ErrorOccurred -= OnError;
                player.EOSEmitted -= OnEos;

                //player.Stop();
                //player.Close();
                try
                {
                    player?.Dispose();
                }
                catch
                {
                    var errorEvent = new PlayerErrorEventArgs
                    {
                        Type = PlayerErrorType.PlayerError,
                        ErrorMessage = "DisposeError"
                    };
                    DelegatePlayerError(errorEvent);

                    //Say cant stop player but player already stopped
                }
                player = null;
            }

        }
    }
}
