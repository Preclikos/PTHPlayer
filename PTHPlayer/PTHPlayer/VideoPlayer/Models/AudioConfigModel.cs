namespace PTHPlayer.VideoPlayer.Models
{
    public class AudioConfigModel
    {
        public int Index { get; set; }
        public int Codec { get; set; }
        public byte[] CodecData { get; set; }
        public int Channels { get; set; }
        public int BitRate { get; set; }
        public int SampleRate { get; set; }
        public string Language { get; set; }
    }
}
