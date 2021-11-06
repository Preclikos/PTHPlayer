using Xamarin.Forms;

namespace PTHPlayer.Forms.ViewModels
{
    public class ChannelViewModel
    {
        public string Label { get; set; }
        public int Id { get; set; }
        public int Number { get; set; }
        public string Title { get; set; }
        public double Progress { get; set; }
        public string Image { get; set; }
        public string ImageUrl { get; set; }
    }
}
