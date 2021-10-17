using PTHPlayer.Controllers.Listeners;
using PTHPlayer.DataStorage.Service;
using PTHPlayer.Event.Enums;
using PTHPlayer.Event.Listeners;
using PTHPlayer.HTSP;
using PTHPlayer.HTSP.Listeners;
using PTHPlayer.HTSP.Models;
using System;

namespace PTHPlayer.Controllers
{
    public class HTSPController : IHTSPListener
    {
        private HTSPService HTSPClient;
        private DataService DataStorageClient;
        private IEventListener EventNotificationListener;

        private HTSPListener HTSListener { get; set; }

        public event EventHandler<ChannelUpdateEventArgs> ChannelUpdateEvent;

        public HTSPController(HTSPService hTSPClient, DataService dataStorageClient, IEventListener eventNotificationListener)
        {
            HTSPClient = hTSPClient;
            DataStorageClient = dataStorageClient;
            EventNotificationListener = eventNotificationListener;
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
                    EventNotificationListener.SendNotification(nameof(HTSPController), "Start Async Load", EventId.MetaData, EventType.Loading);
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
