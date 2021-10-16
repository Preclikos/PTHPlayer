using System.Collections.Generic;

namespace PTHPlayer.HTSP.Parsers
{
    public class ChannelParser
    {
        public int ChannelId { get; set; }
        public string ChannelName { get; set; }
        public string ChannelIcon { get; set; }
        public int EventId { get; set; }
        public int NextEventId { get; set; }
        public int Number { get; set; }
        public ICollection<int> Tags { get; set; }
        public ChannelParser(HTSMessage channelMsg)
        {
            ChannelId = channelMsg.getInt("channelId");
            if (channelMsg.containsField("channelName"))
            {
                ChannelName = channelMsg.getString("channelName");
            }
            if (channelMsg.containsField("eventId"))
            {
                EventId = channelMsg.getInt("eventId");
            }
            if (channelMsg.containsField("nextEventId"))
            {
                NextEventId = channelMsg.getInt("nextEventId");
            }
            if (channelMsg.containsField("channelNumber"))
            {
                Number = channelMsg.getInt("channelNumber");
            }
        }
    }
}
