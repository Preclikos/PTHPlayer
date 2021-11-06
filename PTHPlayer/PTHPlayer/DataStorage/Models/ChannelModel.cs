namespace PTHPlayer.DataStorage.Models
{
    public class ChannelModel
    {
        public string Label { get; set; }
        public int Id { get; set; }
        public int Number { get; set; }
        public int EventId { get; set; }
        public int NextEventId { get; set; }
        public string Icon { get; set; }

        public bool HasHttpIcon()
        {
            if(Icon.StartsWith("http", System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }
    }
}
