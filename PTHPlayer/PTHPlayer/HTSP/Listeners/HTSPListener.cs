using PTHLogger;
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
        readonly DataService DataStorageClient;

        readonly IEventListener EvenetNotificationListener;

        readonly IPlayerListener SubcriptionListener;
        readonly IHTSPListener HTSPControllerListener;

        private readonly ILogger Logger = LoggerManager.GetInstance().GetLogger("PTHPlayer.HTSP.Listeners");
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
            try
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
                            var channel = ChannelParser.Add(response);
                            DataStorageClient.ChannelAdd(channel);

                            Logger.Info("ChannelAdd with update evet channel with Id: " + channel.Id);
                            HTSPControllerListener.ChannelUpdate(channel.Id);

                            break;
                        }
                    case "channelUpdate":
                        {
                            var channel = ChannelParser.Update(response);
                            DataStorageClient.ChannelUpdate(channel.Id, channel.Fields);

                            Logger.Info("ChannelUpdate with update evet channel with Id: " + channel.Id);
                            HTSPControllerListener.ChannelUpdate(channel.Id);

                            break;
                        }
                    case "channelDelete":
                        {
                            var channId = response.getInt("channelId");
                            Logger.Info("ChannelDetele channel with Id: " + channId);
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
                            var epg = EventParser.Add(response);
                            DataStorageClient.EPGAdd(epg);

                            break;
                        }
                    case "eventUpdate":
                        {
                            var epg = EventParser.Update(response);
                            DataStorageClient.EPGUpdate(epg.Id, epg.Fields);

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
                            if (response.containsField("status"))
                            {
                                EvenetNotificationListener.SendNotification("Status", response.getString("status"), EventId.Subscription, EventType.Loading);
                            }
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
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
        }
    }
}
