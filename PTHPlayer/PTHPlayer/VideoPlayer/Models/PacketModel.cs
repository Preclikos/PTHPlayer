namespace PTHPlayer.VideoPlayer.Models
{
    public class PacketModel
    {
        public int StreamId { get; set; }
        public long Duration { get; set; }
        public long PTS { get; set; }
        public byte[] Data { get; set; }
    }
}
