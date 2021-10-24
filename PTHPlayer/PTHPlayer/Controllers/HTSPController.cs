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

            HTSPClient.ConnectedEvent += AfterConnected;
            HTSPClient.DisconnectedEvent += Disconnected;
            HTSPClient.ReconnectingEvent += Reconnecting;
        }

        public void SetListener(HTSPListener hTSPListener)
        {
            HTSListener = hTSPListener;
        }

        public void Connect(bool withMonitor = true)
        {
            if (HTSListener == null)
            {
                throw new ArgumentNullException(nameof(HTSListener));
            }

            if (DataStorageClient.IsCredentialsExist())
            {

                var credentials = DataStorageClient.GetCredentials();

                if (HTSPClient.NeedRestart())
                {
                    HTSPClient.OpenAndLogin(credentials.Server, credentials.Port, credentials.UserName, credentials.Password, HTSListener, withMonitor);
                }
            }
        }

        public void AfterConnected(object sender, EventArgs eventArgs)
        {
            EventNotificationListener.SendNotification(nameof(HTSPController), "Connected", EventId.Connection, EventType.Success);
            HTSPClient.EnableAsyncMetadata();
        }

        public void Disconnected(object sender, EventArgs eventArgs)
        {
            EventNotificationListener.SendNotification(nameof(HTSPController), "Disconnected", EventId.Generic, EventType.Error);
        }

        public void Reconnecting(object sender, EventArgs eventArgs)
        {
            EventNotificationListener.SendNotification(nameof(HTSPController), "Reconnecting", EventId.Connection, EventType.Error);
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
