using PTHPlayer.DataStorage.Models;
using System;
using System.Collections.Generic;

namespace PTHPlayer.HTSP.Parsers
{
    public static class EventParser
    {
        public static EPGModel Add(HTSMessage eventMsg)
        {
            var epg = new EPGModel
            {
                EventId = eventMsg.getInt("eventId"),
                ChannelId = eventMsg.getInt("channelId"),
                Start = UnixTimeStampToDateTime(eventMsg.getInt("start")),
                End = UnixTimeStampToDateTime(eventMsg.getInt("stop"))
            };

            if (eventMsg.containsField("title"))
            {
                epg.Title = eventMsg.getString("title");
            }

            if (eventMsg.containsField("summary"))
            {
                epg.Summary = eventMsg.getString("summary");
            }

            if (eventMsg.containsField("description"))
            {
                epg.Description = eventMsg.getString("description");
                if (String.IsNullOrEmpty(epg.Summary))
                {
                    epg.Summary = epg.Description.Replace(Environment.NewLine, " ");
                }
            }

            return epg;
        }

        public static (int Id, Dictionary<string, object> Fields) Update(HTSMessage eventMsg)
        {
            var fields = new Dictionary<string, object>();
            var eventId = eventMsg.getInt("eventId");

            if (eventMsg.containsField("channelId"))
            {
                fields.Add("ChannelId", eventMsg.getInt("channelId"));
            }

            if (eventMsg.containsField("start"))
            {
                fields.Add("Start", UnixTimeStampToDateTime(eventMsg.getInt("start")));
            }

            if (eventMsg.containsField("channelId"))
            {
                fields.Add("End", UnixTimeStampToDateTime(eventMsg.getInt("stop")));
            }

            if (eventMsg.containsField("title"))
            {
                fields.Add("Title", eventMsg.getString("title"));
            }

            if (eventMsg.containsField("summary"))
            {
                fields.Add("Summary", eventMsg.getString("summary"));
            }

            if (eventMsg.containsField("description"))
            {
                var description = eventMsg.getString("description");
                fields.Add("Description", description);

                if (fields.ContainsKey("Summary") && String.IsNullOrEmpty((string)fields.GetValueOrDefault("Summary")))
                {
                    fields.Remove("Summary");
                    fields.Add("Summary", description.Replace(Environment.NewLine, " "));
                }
            }

            return (eventId, fields);
        }
        private static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }
    }
}
