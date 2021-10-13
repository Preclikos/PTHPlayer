namespace PTHPlayer.Subtitles.Models
{
    public class SubtitleElement
    {
        public long Pts { get; set; }
        public bool Empty { get; set; }
        public int TimeOut { get; set; }
        public byte[] Data { get; set; }
    }
}
