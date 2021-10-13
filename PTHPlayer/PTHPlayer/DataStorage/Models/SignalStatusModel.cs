namespace PTHPlayer.DataStorage.Models
{
    public class SignalStatusModel
    {
        public string FEStatus { get; set; }
        public int SNR { get; set; }
        public int Signal { get; set; }
        public int BER { get; set; }
        public int UNC { get; set; }
    }
}