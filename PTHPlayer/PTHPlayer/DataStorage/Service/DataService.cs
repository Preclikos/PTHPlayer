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

        private readonly object channelLock = new object();
        private List<ChannelModel> Channels = new List<ChannelModel>();

        private readonly object epgLock = new object();
        private List<EPGModel> EPGs = new List<EPGModel>();

        public string ServerWebRoot;
        public SignalStatusModel SingnalStatus = new SignalStatusModel();

        IDataStorage NativeDataService;
        public DataService()
        {
            NativeDataService = DependencyService.Get<IDataStorage>();
        }

        public void CleanChannelsAndEPGs()
        {
            lock (channelLock)
            {
                lock (epgLock)
                {
                    Channels.Clear();
                    EPGs.Clear();
                }
            }
        }

        public void ChannelAdd(ChannelModel channel)
        {
            lock (channelLock)
            {
                Channels.RemoveAll(r => r.Id == channel.Id);
                Channels.Add(channel);
            }
        }

        public void ChannelUpdate(int id, Dictionary<string, object> fieldsToUpdate)
        {
            var properties = typeof(ChannelModel).GetProperties();
            lock (channelLock)
            {
                var channelToUpdate = Channels.SingleOrDefault(s => s.Id == id);
                if (channelToUpdate != null)
                {
                    foreach (var field in fieldsToUpdate)
                    {
                        var property = properties.SingleOrDefault(s => s.Name == field.Key);
                        if (property != null)
                        {
                            if (property.PropertyType == typeof(int))
                            {
                                property.SetValue(channelToUpdate, (int)field.Value);
                            }
                        }
                    }
                }
            }
        }

        public void ChannelRemove(int channelId)
        {
            lock (channelLock)
            {
                lock (epgLock)
                {
                    Channels.RemoveAll(r => r.Id == channelId);
                    EPGs.RemoveAll(r => r.ChannelId == channelId);
                }
            }
        }

        public void EPGAdd(EPGModel epg)
        {
            lock (epgLock)
            {
                EPGs.RemoveAll(r => r.EventId == epg.EventId);
                EPGs.Add(epg);
            }
        }

        public void EPGRemove(int eventId)
        {
            lock (epgLock)
            {
                EPGs.RemoveAll(r => r.EventId == eventId); ;
            }
        }

        public List<ChannelModel> GetChannels()
        {
            lock (channelLock)
            {
                return Channels.ToList();
            }
        }

        public List<EPGModel> GetEPGs()
        {
            lock (epgLock)
            {
                return EPGs.ToList();
            }
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
