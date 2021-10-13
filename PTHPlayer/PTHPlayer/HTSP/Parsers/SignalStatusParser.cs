using PTHPlayer.DataStorage.Models;

namespace PTHPlayer.HTSP.Parsers
{
    public class SignalStatusParser
    {
        public string FEStatus { get; set; }
        public int SNR { get; set; }
        public int FESignal { get; set; }
        public int BER { get; set; }
        public int UNC { get; set; }
        public SignalStatusParser(HTSMessage signalMsg)
        {
            FEStatus = signalMsg.getString("feStatus");

            if (signalMsg.containsField("feSNR"))
            {
                SNR = signalMsg.getInt("feSNR");
            }

            if (signalMsg.containsField("feSignal"))
            {
                FESignal = signalMsg.getInt("feSignal");
            }

            if (signalMsg.containsField("feBER"))
            {
                BER = signalMsg.getInt("feBER");
            }

            if (signalMsg.containsField("feUNC"))
            {
                UNC = signalMsg.getInt("feUNC");
            }
        }

        public SignalStatusModel Response()
        {
            return new SignalStatusModel
            {
                FEStatus = this.FEStatus,
                SNR = this.SNR,
                Signal = FESignal,
                BER = this.BER,
                UNC = this.UNC
            };
        }
    }
}
