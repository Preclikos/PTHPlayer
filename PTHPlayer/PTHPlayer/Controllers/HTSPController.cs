using PTHPlayer.Controllers.Listeners;
using PTHPlayer.DataStorage.Service;
using PTHPlayer.HTSP;
using PTHPlayer.HTSP.Listeners;
using PTHPlayer.HTSP.Models;
using System;

namespace PTHPlayer.Controllers
{
    public class HTSPController : IHTSPListener
    {
        private HTSPService HTSPClient { get; }
        private DataService DataStorageClient { get; }
        private HTSPListener HTSListener { get; set; }

        public event EventHandler<ChannelUpdateEventArgs> ChannelUpdateEvent;

        public HTSPController(HTSPService hTSPClient, DataService dataStorageClient)
        {
            HTSPClient = hTSPClient;
            DataStorageClient = dataStorageClient; 
        }

        public void SetListener(HTSPListener hTSPListener)
        {
            HTSListener = hTSPListener;
        }

        public void Connect()
        {
            if(HTSListener == null)
            {
                throw new ArgumentNullException(nameof(HTSListener));
            }

            if (DataStorageClient.IsCredentialsExist())
            {

                var credentials = DataStorageClient.GetCredentials();

                if (HTSPClient.NeedRestart())
                {

                    HTSPClient.Open(credentials.Server, credentials.Port, HTSListener);

                    HTSPClient.Login(credentials.UserName, credentials.Password);

                    HTSPClient.EnableAsyncMetadata();
                }
            }
        }

        public void Close()
        {
            HTSPClient.Close();
        }

        protected void DelegateChannelUpdate(object sender, ChannelUpdateEventArgs e)
        {
            EventHandler<ChannelUpdateEventArgs> handler = ChannelUpdateEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public void ChannelUpdate(int channelId)
        {
            var eventArgs = new ChannelUpdateEventArgs
            {
                ChannelId = channelId
            };

            DelegateChannelUpdate(this, eventArgs);
        }
    }
}
