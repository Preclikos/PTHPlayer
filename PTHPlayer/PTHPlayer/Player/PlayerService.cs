using PTHPlayer.HTSP;
using PTHPlayer.Player.Enums;
using PTHPlayer.Player.Interfaces;
using PTHPlayer.Player.Models;
using PTHPlayer.Subtitles.Decoder;
using PTHPlayer.Subtitles.Models;
using PTHPlayer.VideoPlayer.Models;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace PTHPlayer.VideoPlayer
{
    public class PlayerService
    {
        public event EventHandler<PlayerStateChangeEventArgs> PlayerStateChange;
        public event EventHandler<PlayerErrorEventArgs> PlayerError;

        private List<PacketModel> bufferedPackets = new List<PacketModel>();

        VideoConfigModel videoConfig = new VideoConfigModel();
        AudioConfigModel audioConfig = new AudioConfigModel();
        SubtitleConfigModel subtitleConfig = new SubtitleConfigModel();

        List<AudioConfigModel> audioConfigs = new List<AudioConfigModel>();
        List<SubtitleConfigModel> subtitleConfigs = new List<SubtitleConfigModel>();

        private long WaitToBuffer = 0;

        public Queue<SubtitleElement> SubtitleElemnts = new Queue<SubtitleElement>();

        IPlayerService NativePlayerService;
        public PlayerService()
        {
            NativePlayerService = DependencyService.Get<IPlayerService>();
            PlayerError += PlayerService_PlayerError;
        }

        public double GetPlayerTime()
        {
            var span = NativePlayerService.GetPlayerTime();
            return span.TotalMilliseconds;
        }

        void ResetPlayer()
        {

            Stop();

            NativePlayerService.Init(PlayerError);

        }

        public void ChangeAudioTrack(int index)
        {
            var selectedAudio = audioConfigs.SingleOrDefault(s => s.Index == index);
            if (selectedAudio != null)
            {

                ResetPlayer();

                audioConfig = selectedAudio;

                CleanSubtitles();
            }
        }

        public void EnableSubtitleTrack(bool enabled, int index = -1)
        {
            if (enabled)
            {
                if (index != -1)
                {
                    ChangeSubtitleTrack(index);
                    return;
                }

                var selectedSubtitle = subtitleConfigs.FirstOrDefault();
                if (selectedSubtitle != null)
                {
                    CleanSubtitles();

                    subtitleConfig = selectedSubtitle;
                }
                return;
            }

            subtitleConfig = new SubtitleConfigModel();
        }

        public void ChangeSubtitleTrack(int index)
        {
            var selectedSubtitle = subtitleConfigs.SingleOrDefault(s => s.Index == index);
            if (selectedSubtitle != null)
            {
                CleanSubtitles();

                subtitleConfig = selectedSubtitle;
            }
        }

        public ICollection<AudioConfigModel> GetAudioConfigs()
        {
            return audioConfigs.ToList();
        }

        public ICollection<SubtitleConfigModel> GetSubtitleConfigs()
        {
            return subtitleConfigs.ToList();
        }

        public AudioConfigModel GetSelectedAudioConfig()
        {
            return audioConfig;
        }

        public SubtitleConfigModel GetSelectedSubtitleConfig()
        {
            return subtitleConfig;
        }

        public bool SubtitleEnabled()
        {
            return subtitleConfig.Index != 0;
        }

        void CleanSubtitles()
        {
            SubtitleElemnts.Clear();
        }

        public void Subscription(HTSMessage subscriptionStart)
        {
            CleanSubtitles();

            var subscriptionStartId = subscriptionStart.getInt("subscriptionId");

            var streams = subscriptionStart.getList("streams");

            foreach (HTSMessage stream in streams)
            {
                var streamType = stream.getString("type");
                var streamIndex = stream.getInt("index");

                if (Enum.IsDefined(typeof(VideoCodec), streamType))
                {
                    videoConfig.Codec = (int)Enum.Parse(typeof(VideoCodec), streamType);

                    videoConfig.Width = stream.getInt("width");
                    videoConfig.Heght = stream.getInt("height");

                    if (subscriptionStart.containsField("meta"))
                    {
                        videoConfig.CodecData = subscriptionStart.getByteArray("meta");
                    }

                    videoConfig.Index = streamIndex;
                    continue;
                }

                if (Enum.IsDefined(typeof(AudioCodec), streamType))
                {

                    var audio = new AudioConfigModel();

                    audio.Codec = (int)Enum.Parse(typeof(AudioCodec), streamType);

                    audio.Language = "unk";
                    if (stream.containsField("language"))
                    {
                        audio.Language = stream.getString("language");
                    }

                    audio.Channels = stream.getInt("channels");
                    audio.BitRate = stream.getInt("rate");

                    audio.Index = streamIndex;

                    audioConfigs.Add(audio);

                    continue;
                }

                if (streamType == "DVBSUB")
                {
                    var subtitle = new SubtitleConfigModel();

                    subtitle.Language = "unk";
                    if (stream.containsField("language"))
                    {
                        subtitle.Language = stream.getString("language");
                    }

                    subtitle.Index = streamIndex;

                    subtitleConfigs.Add(subtitle);

                    continue;
                }
            }

            audioConfig = audioConfigs.First();

            NativePlayerService.Init(PlayerError);
        }

        private PacketModel ParsePacket(HTSMessage muxPkt)
        {
            return new PacketModel()
            {
                StreamId = muxPkt.getInt("stream"),
                PTS = muxPkt.getLong("pts") * 1000,
                Duration = muxPkt.getLong("duration") * 1000,
                Data = muxPkt.getByteArray("payload")
            };
        }

        public void PushToBuffer(HTSMessage muxPkt)
        {
            var packet = ParsePacket(muxPkt);
            if (videoConfig.Index != packet.StreamId && audioConfig.Index != packet.StreamId)// && subtitleConfig.Index != packet.StreamId)
            {
                return;
            }
            bufferedPackets.Add(packet);
        }

        public void TryCalculateFps(HTSMessage muxPkt, TaskCompletionSource<bool> calculationCompletetion)
        {
            var streamIndex = muxPkt.getInt("stream");
            var duration = muxPkt.getLong("duration");
            if (streamIndex == videoConfig.Index && duration != 0)
            {
                videoConfig.Num = 1000000 / (int)duration;
                videoConfig.Den = 1;

                calculationCompletetion.SetResult(true);

            }
        }

        public void TryCalculateSampleRate(HTSMessage muxPkt, TaskCompletionSource<bool> calculationCompletetion)
        {
            var streamIndex = muxPkt.getInt("stream");
            var duration = muxPkt.getLong("duration");
            if (streamIndex == audioConfig.Index && duration != 0)
            {
                audioConfig.SampleRate = (int)((double)1000000 / duration * 1000);
                calculationCompletetion.SetResult(true);

            }
        }

        void SetConfig()
        {
            NativePlayerService.SetConfig(videoConfig, audioConfig);
        }

        public void Packet(HTSMessage muxPkt)
        {

            var streamIndex = muxPkt.getInt("stream");

            if (videoConfig.Index != streamIndex && audioConfig.Index != streamIndex && subtitleConfig.Index != streamIndex)
            {
                return;
            }

            var packet = ParsePacket(muxPkt);

            if (bufferedPackets.Count != 0)
            {

                foreach (var bufferedPacket in bufferedPackets)
                {
                    if (bufferedPacket.StreamId == videoConfig.Index)
                    {
                        NativePlayerService.VideoPacket(bufferedPacket);
                    }
                    if (bufferedPacket.StreamId == audioConfig.Index)
                    {
                        NativePlayerService.AudioPacket(bufferedPacket);
                    }

                }

                bufferedPackets.Clear();
            }

            if (streamIndex == videoConfig.Index)
            {
                NativePlayerService.VideoPacket(packet);
            }

            if (streamIndex == audioConfig.Index)
            {
                NativePlayerService.AudioPacket(packet);
            }

            if (streamIndex == subtitleConfig.Index)
            {
                SubtitlePacket(packet);
            }

            var playerState = NativePlayerService.GetPlayerState();
            if (playerState == PlayerState.Ready && playerState != PlayerState.Paused)
            {
                NativePlayerService.Play();
                if (NativePlayerService.GetPlayerState() == PlayerState.Playing)
                {
                    DelegatePlayerStateChange(new PlayerStateChangeEventArgs() { State = PlayerState.Playing });
                }
            }
            if (playerState == PlayerState.Paused && WaitToBuffer < DateTime.Now.Ticks)
            {
                NativePlayerService.Resume();
                if (NativePlayerService.GetPlayerState() == PlayerState.Playing)
                {
                    DelegatePlayerStateChange(new PlayerStateChangeEventArgs() { State = PlayerState.Playing });
                }
            }
        }

        void SubtitlePacket(PacketModel packet)
        {

            var dvbSub = new DvbSubPes(0, packet.Data);
            var subImage = dvbSub.GetImageFull();

            MemoryStream memStream = new MemoryStream();
            SKManagedWStream wstream = new SKManagedWStream(memStream);

            var subtitleElement = new SubtitleElement();
            subtitleElement.Pts = packet.PTS;
            subtitleElement.TimeOut = dvbSub.PageTimeOut();

            if (dvbSub.ContainsData())
            {
                //Encode can block packet receiving maybe better to move after queue
                if (SKPixmap.Encode(wstream, subImage, SKEncodedImageFormat.Png, 70))
                {
                    memStream.Position = 0;
                    subtitleElement.Empty = false;
                    subtitleElement.Data = memStream.ToArray();
                }
                else
                {
                    subtitleElement.Empty = true;
                }
            }
            else
            {
                subtitleElement.Empty = true;
            }

            SubtitleElemnts.Enqueue(subtitleElement);

        }

        public Task PreparePlayer()
        {
            SetConfig();
            return NativePlayerService.PreparePlayer();
        }

        public Task PacketReady()
        {
            return NativePlayerService.PacketReady();
        }

        public void SubscriptionStop()
        {

            DelegatePlayerStateChange(new PlayerStateChangeEventArgs() { State = PlayerState.Stop });

            Stop();

            CleanUp();
        }

        public void CleanUp()
        {
            videoConfig = new VideoConfigModel();
            audioConfig = new AudioConfigModel();
            subtitleConfig = new SubtitleConfigModel();

            bufferedPackets.Clear();
            audioConfigs.Clear();
            subtitleConfigs.Clear();

        }

        public void Stop()
        {
            NativePlayerService.Close();
        }

        private void PlayerService_PlayerError(object sender, PlayerErrorEventArgs e)
        {
            if (e.Type == PlayerErrorType.BufferChange)
            {
                if (NativePlayerService.GetPlayerState() == PlayerState.Playing)
                {
                    NativePlayerService.Pause();
                    DelegatePlayerStateChange(new PlayerStateChangeEventArgs() { State = PlayerState.Paused });
                }
                WaitToBuffer = (DateTime.Now + TimeSpan.FromSeconds(1)).Ticks;
            }
        }

        protected void DelegatePlayerStateChange(PlayerStateChangeEventArgs e)
        {
            EventHandler<PlayerStateChangeEventArgs> handler = PlayerStateChange;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}
