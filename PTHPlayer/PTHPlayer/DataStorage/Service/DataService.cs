using PTHPlayer.DataStorage.Models;
using PTHPlayer.Models;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace PTHPlayer.DataStorage.Service
{
    public class DataService
    {
        public int SelectedChannelId = -1;

        public List<ChannelModel> Channels = new List<ChannelModel>();

        public List<EPGModel> EPGs = new List<EPGModel>();

        public SignalStatusModel SingnalStatus = new SignalStatusModel();

        IDataStorage NativeDataService;
        public DataService()
        {
            NativeDataService = DependencyService.Get<IDataStorage>();
        }

        public void CleanChannelsAndEPGs()
        {
            Channels.Clear();
            EPGs.Clear();
        }

        public List<ChannelModel> GetChannels()
        {
            return Channels.ToList();
        }

        public List<EPGModel> GetEPGs()
        {
            return EPGs.ToList();
        }

        public CredentialsModel GetCredentials()
        {
            return NativeDataService.GetCredentials();
        }

        public void SetCredentials(CredentialsModel credentials)
        {
            NativeDataService.SaveCredentials(credentials);
        }

        public void ClearCredentials()
        {
            NativeDataService.ClearCredentials();
        }

        public bool IsCredentialsExist()
        {
            var credentials = NativeDataService.GetCredentials();
            if (string.IsNullOrEmpty(credentials.Server) || 
                string.IsNullOrEmpty(credentials.UserName) || 
                string.IsNullOrEmpty(credentials.Password) || 
                credentials.Port == 0)
            {
                return false;
            }
            return true;
        }
    }
}
