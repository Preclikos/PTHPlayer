using PTHPlayer.HTSP.HTSP_Responses;
using PTHPlayer.HTSP.Listeners;
using System;
using System.Threading;

namespace PTHPlayer.HTSP
{
    public class HTSPService
    {
        public EventHandler<EventArgs> ConnectedEvent;
        public EventHandler<EventArgs> DisconnectedEvent;
        public EventHandler<EventArgs> ReconnectingEvent;
        public EventHandler<EventArgs> WrongLogin;

        Thread Monitor;
        bool MonitorWhanted;

        public HTSConnectionAsync HTPClient;
        public HTSPService()
        {
            Monitor = new Thread(new ParameterizedThreadStart(MonitorThread));
        }

        public void OpenAndLogin(string address, int port, string userName, string password, HTSPListener HTPListener, bool withMonitor = false)
        {
            if (Monitor.ThreadState == ThreadState.Running)
            {
                return;
            }

            if (withMonitor)
            {
                MonitorWhanted = true;
                Monitor.Start(new MonitorConnectionStart { HTPListener = HTPListener, address = address, port = port, userName = userName, password = password });
            }
            else
            {
                Open(address, port, HTPListener);
                if (!Login(userName, password))
                {
                    throw new Exception("Invalid credentials");
                }
            }
        }

        void MonitorThread(object connectionParameters)
        {
            var connectionParams = (MonitorConnectionStart)connectionParameters;
            while (MonitorWhanted)
            {
                try
                {

                    if (HTPClient == null || !HTPClient.Connected())
                    {
                        OnReconnecting(new EventArgs());
                        if (Open(connectionParams.address, connectionParams.port, connectionParams.HTPListener))
                        {
                            
                            if (Login(connectionParams.userName, connectionParams.password))
                            {
                                OnConnected(new EventArgs());
                            }
                            else
                            {
                                OnWrongLogin(new EventArgs());
                                //MonitorWhanted = false;
                                //return;
                            }
                        }
                    }
                }
                catch
                {
                    OnDisconnected(new EventArgs());
                }
                Thread.Sleep(1000);
            }
        }

        public bool Login(string userName, string password)
        {
            return HTPClient.Authenticate(userName, password);
        }

        bool Open(string address, int port, HTSPListener HTPListener)
        {
            HTPClient = new HTSConnectionAsync(HTPListener, "Tizen C#", "Beta");
            HTPClient.ConnectiongHandler += ConnResponse;
            return HTPClient.Open(address, port);
        }

        public void ConnResponse(object sender, string password)
        {
            OnConnected(new EventArgs());
        }

        protected virtual void OnConnected(EventArgs e)
        {
            EventHandler<EventArgs> handler = ConnectedEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnReconnecting(EventArgs e)
        {
            EventHandler<EventArgs> handler = ReconnectingEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnDisconnected(EventArgs e)
        {
            EventHandler<EventArgs> handler = DisconnectedEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnWrongLogin(EventArgs e)
        {
            EventHandler<EventArgs> handler = WrongLogin;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public int Subscribe(int channelId)
        {

            Random rnd = new Random();
            int subscriptionId = rnd.Next(1, 99999);

            //App.PlayerService.SetSubscriptionId(subscriptionId);

            HTSMessage getTicketMessage = new HTSMessage();
            getTicketMessage.Method = "subscribe";
            getTicketMessage.putField("channelId", channelId);
            getTicketMessage.putField("normts", 1);
            getTicketMessage.putField("subscriptionId", subscriptionId);

            LoopBackResponseHandler lbrh = new LoopBackResponseHandler();
            HTPClient.sendMessage(getTicketMessage, lbrh);

            lbrh.getResponse();

            return subscriptionId;
        }

        public void UnSubscribe(int subscriptionId)
        {
            HTSMessage getTicketMessage = new HTSMessage();
            getTicketMessage.Method = "unsubscribe";
            getTicketMessage.putField("subscriptionId", subscriptionId);

            LoopBackResponseHandler lbrh = new LoopBackResponseHandler();
            HTPClient.sendMessage(getTicketMessage, lbrh);
            //lbrh.getResponse();
        }

        public void EnableAsyncMetadata()
        {
            HTSMessage getTicketMessage = new HTSMessage();
            getTicketMessage.Method = "enableAsyncMetadata";
            getTicketMessage.putField("epg", 1);
            DateTime foo = DateTime.UtcNow + TimeSpan.FromHours(4);
            long unixTime = ((DateTimeOffset)foo).ToUnixTimeSeconds();
            getTicketMessage.putField("epgMaxTime", unixTime);

            LoopBackResponseHandler lbrh = new LoopBackResponseHandler();
            HTPClient.sendMessage(getTicketMessage, lbrh);

        }

        public void GetEvent(int eventId)
        {
            HTSMessage getTicketMessage = new HTSMessage();
            getTicketMessage.Method = "getEvent";
            getTicketMessage.putField("eventId", eventId);

            LoopBackResponseHandler lbrh = new LoopBackResponseHandler();
            HTPClient.sendMessage(getTicketMessage, lbrh);
            lbrh.getResponse();
        }

        public void Close()
        {
            MonitorWhanted = false;
            HTPClient.Stop();
        }

        public bool NeedRestart()
        {
            if (HTPClient == null)
            {
                return true;
            }

            return HTPClient.NeedsRestart();
        }

        class MonitorConnectionStart
        {
            public string address;
            public int port;
            public string userName;
            public string password;
            public HTSPListener HTPListener;
        }
    }
}
