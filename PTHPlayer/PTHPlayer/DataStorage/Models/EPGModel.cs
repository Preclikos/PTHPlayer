using System;

namespace PTHPlayer.DataStorage.Models
{
    public class EPGModel
    {
        public int EventId { get; set; }
        public int ChannelId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }   
        public string Description { get; set; }
    }
}
