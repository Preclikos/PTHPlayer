using PTHPlayer.Controllers.Listeners;
using PTHPlayer.DataStorage.Service;
using PTHPlayer.Event.Enums;
using PTHPlayer.Event.Listeners;
using PTHPlayer.HTSP;
using PTHPlayer.HTSP.Listeners;
using PTHPlayer.HTSP.Models;
using System;
using System.Threading.Tasks;

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

            HTSPClient.ErrorHandler += HTSPClient_ErrorHandler;
            HTSPClient.ConnectionStateChange += HTSPClient_ConnectionStateChange;
        }

        private void HTSPClient_ConnectionStateChange(object sender, HTSPConnectionStateChangeArgs e)
        {
            switch (e.ConnectionChangeState)
            {
                case ConnectionState.Disconnected:
                    {
                        EventNotificationListener.SendNotification(nameof(HTSPController), e.Message, EventId.Generic, EventType.Error);
                        break;
                    }
                case ConnectionState.ConnectingFail:
                    {
                        EventNotificationListener.SendNotification(nameof(HTSPController), e.ConnectionChangeState.ToString(), EventId.Connection, EventType.Error);
                        break;
                    }
                case ConnectionState.Connecting:
                    {
                        EventNotificationListener.SendNotification(nameof(HTSPController), e.ConnectionChangeState.ToString(), EventId.Connection, EventType.Loading);
                        break;
                    }
                case ConnectionState.Connected:
                    {
                        EventNotificationListener.SendNotification(nameof(HTSPController), e.ConnectionChangeState.ToString(), EventId.Connection, EventType.Success);
                        break;
                    }
                case ConnectionState.AuthenticatingFail:
                    {
                        EventNotificationListener.SendNotification(nameof(HTSPController), e.ConnectionChangeState.ToString(), EventId.Authentication, EventType.Error);
                        break;
                    }
                case ConnectionState.Authenticating:
                    {
                        EventNotificationListener.SendNotification(nameof(HTSPController), e.ConnectionChangeState.ToString(), EventId.Authentication, EventType.Loading);
                        break;
                    }
                case ConnectionState.Authenticated:
                    {
                        EventNotificationListener.SendNotification(nameof(HTSPController), e.ConnectionChangeState.ToString(), EventId.Authentication, EventType.Success);
                        HTSPClient.EnableAsyncMetadata();
                        break;
                    }
            }
        }

        private void HTSPClient_ErrorHandler(object sender, HTSPErrorArgs e)
        {
            EventNotificationListener.SendNotification(nameof(HTSPController), e.Message, EventId.Generic, EventType.Error);
        }

        public void SetListener(HTSPListener hTSPListener)
        {
            HTSListener = hTSPListener;
        }

        public void Connect(bool overMonitor)
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
                    if (overMonitor)
                    {
                        HTSPClient.Open(credentials.Server, credentials.Port, credentials.UserName, credentials.Password, HTSListener);
                    }
                    else
                    {
                        if(HTSPClient.Open(credentials.Server, credentials.Port, HTSListener))
                        {

                            if (!HTSPClient.Login(credentials.UserName, credentials.Password))
                            {
                                throw new Exception("Invalid Login Credentials");
                            }
                        }
                        else
                        {
                            throw new Exception("Cannot connect server");
                            
                        }
                    }

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
