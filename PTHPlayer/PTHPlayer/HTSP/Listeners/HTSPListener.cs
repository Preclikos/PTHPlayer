using PTHLogger;
using PTHPlayer.Controllers.Listeners;
using PTHPlayer.DataStorage.Service;
using PTHPlayer.Event.Enums;
using PTHPlayer.Event.Listeners;
using PTHPlayer.HTSP.Parsers;
using System;

namespace PTHPlayer.HTSP.Listeners
{
    public class HTSPListener : IHTSConnectionListener
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

        public void OnError(Exception ex)
        {
            EvenetNotificationListener.SendNotification(nameof(HTSPListener), ex.Message, eventType: Event.Enums.EventType.Error);
        }

        public void OnMessage(HTSMessage response)
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

                            Logger.Info("ChannelAdd with update event channel with Id: " + channel.Id);
                            HTSPControllerListener.ChannelUpdate(channel.Id);
                            break;
                        }
                    case "channelUpdate":
                        {
                            var (Id, Fields) = ChannelParser.Update(response);
                            DataStorageClient.ChannelUpdate(Id, Fields);

                            Logger.Info("ChannelUpdate with update event channel with Id: " + Id);
                            HTSPControllerListener.ChannelUpdate(Id);
                            break;
                        }
                    case "channelDelete":
                        {
                            var channId = response.getInt("channelId");
                            DataStorageClient.ChannelRemove(channId);

                            Logger.Info("ChannelDetele channel with Id: " + channId);
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

                            Logger.Info("EventAdd with Id: " + epg.EventId);
                            break;
                        }
                    case "eventUpdate":
                        {
                            var (Id, Fields) = EventParser.Update(response);
                            DataStorageClient.EPGUpdate(Id, Fields);

                            Logger.Info("EventUpdate with Id: " + Id);
                            break;
                        }
                    case "eventDelete":
                        {
                            var eventId = response.getInt("eventId");
                            DataStorageClient.EPGRemove(eventId);

                            Logger.Info("EventDelete with Id: " + eventId);
                            break;
                        }
                    case "subscriptionStatus":
                        {
                            if (response.containsField("status"))
                            {
                                EvenetNotificationListener.SendNotification("Status", response.getString("status"), EventId.Subscription, EventType.Loading);
                            }
                            Logger.Info("Subscription Status");
                            break;
                        }
                    case "subscriptionGrace":
                        {
                            Logger.Info("Subscription Grace");
                            break;
                        }
                    case "signalStatus":
                        {
                            var parsedSignal = new SignalStatusParser(response);
                            DataStorageClient.SingnalStatus = parsedSignal.Response();
                            Logger.Info("Subscription Status");
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
