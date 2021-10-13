using PTHPlayer.DataStorage.Models;
using PTHPlayer.Subtitles.Models;
using System.Collections.Generic;
using System.Linq;

namespace PTHPlayer.DataStorage.Service
{
    public class DataModel
    {
        public int SelectedChannelId = -1;

        public List<ChannelModel> Channels = new List<ChannelModel>();

        public List<EPGModel> EPGs = new List<EPGModel>();

        public SignalStatusModel SingnalStatus = new SignalStatusModel();

        public List<ChannelModel> GetChannels()
        {
            return Channels.ToList();
        }

        public List<EPGModel> GetEPGs()
        {
            return EPGs.ToList();
        }
    }
}
