using PTHPlayer.Controllers.Listeners;
using PTHPlayer.DataStorage.Models;
using PTHPlayer.HTSP.Parsers;
using System;
using System.Linq;

namespace PTHPlayer.HTSP.Listeners
{
    public class HTSPListener : HTSConnectionListener
    {

        private IPlayerListener SubcriptionListener;
        private IHTSPListener HTSPControllerListener;
        public HTSPListener(IPlayerListener subscriptionListener, IHTSPListener hTSPListener)
        {
            SubcriptionListener = subscriptionListener;
            HTSPControllerListener = hTSPListener;
        }

        public void onError(Exception ex)
        {
            //throw new NotImplementedException();
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
                        var channId = response.getInt("channelId");
                        var eventId = response.getInt("eventId");
                        var nextEventId = response.getInt("nextEventId");
                        var channName = response.getString("channelName");
                        var channNumber = response.getInt("channelNumber");
                        App.DataStorageService.Channels.Add(new ChannelModel() { Label = channName, Id = channId, Number = channNumber, EventId = eventId, NextEventId = nextEventId });
                        HTSPControllerListener.ChannelUpdate(channId);
                        break;
                    }
                case "channelUpdate":
                    {
                        var channId = response.getInt("channelId");
                        var channel = App.DataStorageService.Channels.SingleOrDefault(s => s.Id == channId);
                        if (channel != null)
                        {
                            if (response.containsField("eventId"))
                            {
                                channel.EventId = response.getInt("eventId");
                            }
                            if (response.containsField("nextEventId"))
                            {
                                channel.NextEventId = response.getInt("nextEventId");
                            }
                            if (response.containsField("channelNumber"))
                            {
                                channel.Number = response.getInt("channelNumber");
                            }
                        }
                        HTSPControllerListener.ChannelUpdate(channId);
                        break;
                    }
                case "channelDelete":
                    {
                        var channId = response.getInt("channelId");
                        App.DataStorageService.Channels.RemoveAll(r => r.Id == channId);
                        App.DataStorageService.EPGs.RemoveAll(r => r.ChannelId == channId);
                        break;
                    }
                case "initialSyncCompleted":
                    {

                        break;
                    }
                case "eventAdd":
                    {
                        var start = response.getInt("start");
                        var stop = response.getInt("stop");
                        var eventId = response.getInt("eventId");

                        var title = string.Empty;

                        if (response.containsField("title"))
                        {
                            title = response.getString("title");
                        }

                        int channelId = 0;
                        if (response.containsField("channelId"))
                        {
                            channelId = response.getInt("channelId");
                        }

                        var summary = string.Empty;
                        if (response.containsField("summary"))
                        {
                            summary = response.getString("summary");
                        }

                        var description = string.Empty;
                        if (response.containsField("description"))
                        {
                            description = response.getString("description");
                            if (summary == string.Empty)
                            {
                                summary = description.Replace(Environment.NewLine, " ");
                            }
                        }

                        var epg = new EPGModel() { EventId = eventId, ChannelId = channelId, Title = title, Summary = summary, Description = description, Start = UnixTimeStampToDateTime(start), End = UnixTimeStampToDateTime(stop) };

                        App.DataStorageService.EPGs.Add(epg);

                        break;
                    }
                case "eventUpdate":
                    {

                        break;
                    }
                case "eventDelete":
                    {

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
                        App.DataStorageService.SingnalStatus = parsedSignal.Response();
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
