using PTHPlayer.Controllers.Listeners;
using PTHPlayer.DataStorage.Models;
using PTHPlayer.DataStorage.Service;
using PTHPlayer.Event.Enums;
using PTHPlayer.Event.Listeners;
using PTHPlayer.HTSP.Parsers;
using System;
using System.Collections.Generic;

namespace PTHPlayer.HTSP.Listeners
{
    public class HTSPListener : HTSConnectionListener
    {
        private DataService DataStorageClient;

        private IEventListener EvenetNotificationListener;

        private IPlayerListener SubcriptionListener;
        private IHTSPListener HTSPControllerListener;

        public HTSPListener(DataService dataStorageClient, IPlayerListener subscriptionListener, IHTSPListener hTSPListener, IEventListener evenetNotificationListener)
        {
            DataStorageClient = dataStorageClient;
            SubcriptionListener = subscriptionListener;
            HTSPControllerListener = hTSPListener;
            EvenetNotificationListener = evenetNotificationListener;
        }

        public void onError(Exception ex)
        {
            EvenetNotificationListener.SendNotification(nameof(HTSPListener), ex.Message, eventType: Event.Enums.EventType.Error);
        }

        public void onMessage(HTSMessage response)
        {

            //throw new NotImplementedException();
            switch (response.Method)
            {
                case "subscriptionStart":
                    {
                        SubcriptionListener.OnSubscriptionStart(response);
                        break;
                    }
                case "subscriptionStop":
                    {
                        SubcriptionListener.OnSubscriptionStop(response);
                        break;
                    }
                case "subscriptionSkip":
                    {
                        SubcriptionListener.OnSubscriptionSkip(response);
                        break;
                    }
                case "muxpkt":
                    {
                        SubcriptionListener.OnMuxPkt(response);
                        break;
                    }
                case "channelAdd":
                    {
                        var channel = new ChannelModel();
                        channel.Id = response.getInt("channelId");

                        channel.Label = response.getString("channelName");
                        channel.Number = response.getInt("channelNumber");

                        if (response.containsField("eventId"))
                        {
                            channel.EventId = response.getInt("eventId");
                        }

                        if (response.containsField("nextEventId"))
                        {
                            channel.EventId = response.getInt("nextEventId");
                        }

                        if (response.containsField("channelIcon"))
                        {
                            channel.Icon = response.getString("channelIcon");
                        }

                        DataStorageClient.ChannelAdd(channel);

                        HTSPControllerListener.ChannelUpdate(channel.Id);
                        break;
                    }
                case "channelUpdate":
                    {
                        var fileds = new Dictionary<string, object>();
                        var channId = response.getInt("channelId");

                        if (response.containsField("eventId"))
                        {
                            fileds.Add("EventId", response.getInt("eventId"));
                        }
                        if (response.containsField("nextEventId"))
                        {
                            fileds.Add("NextEventId", response.getInt("nextEventId"));
                        }
                        if (response.containsField("channelNumber"))
                        {
                            fileds.Add("Number", response.getInt("channelNumber"));
                        }

                        DataStorageClient.ChannelUpdate(channId, fileds);
                        HTSPControllerListener.ChannelUpdate(channId);

                        break;
                    }
                case "channelDelete":
                    {
                        var channId = response.getInt("channelId");
                        DataStorageClient.ChannelRemove(channId);
                        break;
                    }
                case "initialSyncCompleted":
                    {
                        EvenetNotificationListener.SendNotification(nameof(HTSPListener), "Init Sync Completed", EventId.MetaData, EventType.Success);
                        break;
                    }
                case "eventAdd":
                    {
                        var epg = new EPGModel();
                        epg.EventId = response.getInt("eventId");
                        epg.ChannelId = response.getInt("channelId");
                        epg.Start = UnixTimeStampToDateTime(response.getInt("start"));
                        epg.End = UnixTimeStampToDateTime(response.getInt("stop"));

                        if (response.containsField("title"))
                        {
                            epg.Title = response.getString("title");
                        }

                        if (response.containsField("summary"))
                        {
                            epg.Summary = response.getString("summary");
                        }

                        if (response.containsField("description"))
                        {
                            epg.Description = response.getString("description");
                            if (epg.Summary == string.Empty)
                            {
                                epg.Summary = epg.Description.Replace(Environment.NewLine, " ");
                            }
                        }

                        DataStorageClient.EPGAdd(epg);
                        break;
                    }
                case "eventUpdate":
                    {

                        break;
                    }
                case "eventDelete":
                    {
                        var eventId = response.getInt("eventId");
                        DataStorageClient.EPGRemove(eventId);
                        break;
                    }
                case "subscriptionStatus":
                    {
                        break;
                    }
                case "subscriptionGrace":
                    {
                        break;
                    }
                case "signalStatus":
                    {
                        var parsedSignal = new SignalStatusParser(response);
                        DataStorageClient.SingnalStatus = parsedSignal.Response();
                        break;
                    }
            }
        }
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }
    }
}
