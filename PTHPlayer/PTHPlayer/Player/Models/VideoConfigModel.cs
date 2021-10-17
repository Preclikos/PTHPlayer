namespace PTHPlayer.Player.Models
{
    public class VideoConfigModel
    {
        public int Index { get; set; }
        public int Codec { get; set; }
        public int Width { get; set; }
        public int Heght { get; set; }
        public byte[] CodecData { get; set; }
        public int Den { get; set; }
        public int Num { get; set; }
    }
}
