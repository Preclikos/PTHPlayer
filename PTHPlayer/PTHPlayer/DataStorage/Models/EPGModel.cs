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


        public double GetProgress()
        {
            if(Start == null || End == null)
            {
                return 0;
            }

            var start = Start.Ticks;
            var end = End.Ticks;
            var current = DateTime.Now.Ticks;

            var range = end - start;
            var currentOnRange = end - current;

            var onePercentOnRange = range / 100;
            var currentPercent = currentOnRange / onePercentOnRange;

            if(onePercentOnRange < 0)
            {
                return 0;
            }

            var progressPercent = 1 - currentPercent / (double)100;

            return progressPercent;
        }
    }
}
