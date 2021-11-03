using PTHPlayer.DataStorage.Models;
using System;
using System.Collections.Generic;

namespace PTHPlayer.HTSP.Parsers
{
    public static class ChannelParser
    {
        public static ChannelModel Add(HTSMessage channelMsg)
        {
            var channel = new ChannelModel
            {
                Id = channelMsg.getInt("channelId"),

                Label = channelMsg.getString("channelName"),
                Number = channelMsg.getInt("channelNumber")
            };

            if (channelMsg.containsField("eventId"))
            {
                channel.EventId = channelMsg.getInt("eventId");
            }

            if (channelMsg.containsField("nextEventId"))
            {
                channel.EventId = channelMsg.getInt("nextEventId");
            }

            if (channelMsg.containsField("channelIcon"))
            {
                channel.Icon = channelMsg.getString("channelIcon");
            }
            return channel;
        }

        public static (int Id, Dictionary<string, object> Fields) Update(HTSMessage channelMsg)
        {
            var fields = new Dictionary<string, object>();
            var channelId = channelMsg.getInt("channelId");

            if (channelMsg.containsField("eventId"))
            {
                fields.Add("EventId", channelMsg.getInt("eventId"));
            }
            if (channelMsg.containsField("nextEventId"))
            {
                fields.Add("NextEventId", channelMsg.getInt("nextEventId"));
            }
            if (channelMsg.containsField("channelNumber"))
            {
                fields.Add("Number", channelMsg.getInt("channelNumber"));
            }
            if (channelMsg.containsField("channelIcon"))
            {
                fields.Add("Icon", channelMsg.getString("channelIcon"));
            }

            return (channelId, fields);
        }
    }
}
