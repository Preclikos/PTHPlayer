﻿using PTHPlayer.HTSP.HTSP_Responses;
using PTHPlayer.HTSP.Listeners;
using System;
using System.Threading;

namespace PTHPlayer.HTSP
{
    public class HTSPService
    {
        public static HTSConnectionAsync HTPClient;
        public HTSPService()
        {
        }

        public void Open(string address, int port, HTSPListener HTPListener)
        {
            HTPClient = new HTSConnectionAsync(HTPListener, "Tizen C#", "Beta");
            HTPClient.Open(address, port);
        }

        public bool Login(string userName, string password)
        {
            return HTPClient.Authenticate(userName, password);
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
    }
}
