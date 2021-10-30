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
        readonly HTSPService HTSPClient;
        readonly DataService DataStorage;
        readonly IEventListener EventNotificationListener;

        HTSPListener HTSListener { get; set; }

        public event EventHandler<ChannelUpdateEventArgs> ChannelUpdateEvent;

        public HTSPController(DataService dataStorage, HTSPService hTSPClient, IEventListener eventNotificationListener)
        {
            HTSPClient = hTSPClient;
            DataStorage = dataStorage;
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

        public bool Connect(bool overMonitor, string server = "", int port = 0, string userName = "", string password = "")
        {
            if (HTSListener == null)
            {
                throw new ArgumentNullException(nameof(HTSListener));
            }

            if (String.IsNullOrEmpty(server) || port == 0 || String.IsNullOrEmpty(userName) || String.IsNullOrEmpty(password))
            {
                if (DataStorage.IsCredentialsExist())
                {
                    var credentials = DataStorage.GetCredentials();
                    server = credentials.Server;
                    port = credentials.Port;
                    userName = credentials.UserName;
                    password = credentials.Password;
                }
                else
                {
                    throw new Exception("No stored credentials error!");
                }
            }

            if (overMonitor)
            {
                HTSPClient.Open(server, port, userName, password, HTSListener);
                return true;
            }
            else
            {
                if (HTSPClient.Open(server, port, HTSListener))
                {

                    if (HTSPClient.Login(userName, password))
                    {
                        return true;
                    }
                    else
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

        public void Close(bool syncClose = false)
        {
            HTSPClient.Close(syncClose);
        }

        protected void DelegateChannelUpdate(object sender, ChannelUpdateEventArgs e)
        {
            ChannelUpdateEvent?.Invoke(this, e);
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
