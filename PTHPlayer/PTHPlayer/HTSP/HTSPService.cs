using PTHPlayer.HTSP.Listeners;
using PTHPlayer.HTSP.Responses;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PTHPlayer.HTSP
{
    public class HTSPService
    {
        const string ClientName = "Tizen C#";
        const string ClientVersion = "Beta";

        public static HTSConnectionAsync HTPClient;
        public event EventHandler<HTSPConnectionStateChangeArgs> ConnectionStateChange;
        public event EventHandler<HTSPErrorArgs> ErrorHandler;

        public void Open(string address, int port, string userName, string password, HTSPListener HTPListener)
        {
            HTPClient = new HTSConnectionAsync(HTPListener, ClientName, ClientVersion);
            HTPClient.ErrorHandler += this.ErrorHandler;
            HTPClient.ConnectionStateChange += this.ConnectionStateChange;

            HTPClient.Start(address, port, userName, password);
        }

        public bool Open(string address, int port, HTSPListener HTPListener)
        {
            HTPClient = new HTSConnectionAsync(HTPListener, ClientName, ClientVersion);
            return HTPClient.Open(address, port);
        }

        public bool Login(string userName, string password)
        {
            return HTPClient.Authenticate(userName, password, CancellationToken.None);
        }

        public int Subscribe(int channelId)
        {

            Random rnd = new Random();
            int subscriptionId = rnd.Next(1, 99999);

            HTSMessage getTicketMessage = new HTSMessage
            {
                Method = "subscribe"
            };
            getTicketMessage.putField("channelId", channelId);
            getTicketMessage.putField("normts", 1);
            getTicketMessage.putField("subscriptionId", subscriptionId);

            LoopBackResponseHandler lbrh = new LoopBackResponseHandler();
            HTPClient.SendMessage(getTicketMessage, lbrh);

            lbrh.getResponse(CancellationToken.None);

            return subscriptionId;
        }

        public void UnSubscribe(int subscriptionId)
        {
            HTSMessage getTicketMessage = new HTSMessage
            {
                Method = "unsubscribe"
            };
            getTicketMessage.putField("subscriptionId", subscriptionId);

            LoopBackResponseHandler lbrh = new LoopBackResponseHandler();
            HTPClient.SendMessage(getTicketMessage, lbrh);
            //lbrh.getResponse();
        }

        public void EnableAsyncMetadata()
        {
            HTSMessage getTicketMessage = new HTSMessage
            {
                Method = "enableAsyncMetadata"
            };
            getTicketMessage.putField("epg", 1);
            DateTime foo = DateTime.UtcNow + TimeSpan.FromHours(4);
            long unixTime = ((DateTimeOffset)foo).ToUnixTimeSeconds();
            getTicketMessage.putField("epgMaxTime", unixTime);

            LoopBackResponseHandler lbrh = new LoopBackResponseHandler();
            HTPClient.SendMessage(getTicketMessage, lbrh);

        }

        public void GetEvent(int eventId)
        {
            HTSMessage getTicketMessage = new HTSMessage
            {
                Method = "getEvent"
            };
            getTicketMessage.putField("eventId", eventId);

            LoopBackResponseHandler lbrh = new LoopBackResponseHandler();
            HTPClient.SendMessage(getTicketMessage, lbrh);
            lbrh.getResponse(CancellationToken.None);
        }

        public void Close(bool syncClose = false)
        {
            if (syncClose)
            {
                var closingTask = Task.Run(() => HTPClient.Stop(true));
                closingTask.Wait();
            }
            else
            {
                Task.Run(() => HTPClient.Stop(true));
            }
        }
    }
}
