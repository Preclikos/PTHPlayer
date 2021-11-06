using PTHPlayer.HTSP.Listeners;
using PTHPlayer.HTSP.Models;
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
            DateTime maxEPGDateTime = DateTime.Now + TimeSpan.FromHours(4);
            long maxEPGUnixTime = ((DateTimeOffset)maxEPGDateTime).ToUnixTimeSeconds();
            getTicketMessage.putField("epgMaxTime", maxEPGUnixTime);

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

        public OpenFileResponse FileOpen(string path)
        {
            HTSMessage openFileMessage = new HTSMessage
            {
                Method = "fileOpen"
            };
            openFileMessage.putField("file", path);

            LoopBackResponseHandler lbrh = new LoopBackResponseHandler();
            HTPClient.SendMessage(openFileMessage, lbrh);
            var response = lbrh.getResponse(CancellationToken.None);

            var openResponse = new OpenFileResponse
            {
                Id = response.getInt("id"),
                Size = response.getLong("size"),
            };

            if(response.containsField("mtime"))
            {
                var lastModifyLong = response.getLong("mtime");
                openResponse.LastModified = UnixTimeStampToDateTime(lastModifyLong);
            }

            return openResponse;
        }

        public byte[] FileRead(int id, long size)
        {
            HTSMessage readFileMessage = new HTSMessage
            {
                Method = "fileRead"
            };
            readFileMessage.putField("id", id);
            readFileMessage.putField("size", size);

            LoopBackResponseHandler lbrh = new LoopBackResponseHandler();
            HTPClient.SendMessage(readFileMessage, lbrh);
            var response = lbrh.getResponse(CancellationToken.None);

            return response.getByteArray("data");
        }

        public void FileClose(int id)
        {
            HTSMessage closeFileMessage = new HTSMessage
            {
                Method = "fileClose"
            };
            closeFileMessage.putField("id", id);

            LoopBackResponseHandler lbrh = new LoopBackResponseHandler();
            HTPClient.SendMessage(closeFileMessage, lbrh);
            //lbrh.getResponse(CancellationToken.None);
        }

        private static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }
    }
}
