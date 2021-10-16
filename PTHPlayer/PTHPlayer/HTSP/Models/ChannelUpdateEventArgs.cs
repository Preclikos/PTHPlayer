using PTHPlayer.HTSP.Enums;
using System;

namespace PTHPlayer.HTSP.Models
{
    public class ChannelUpdateEventArgs : EventArgs
    {
        public ChannelEvent Event { get; set; }
        public int ChannelId { get; set; }
        HTSMessage Message { get; set; }
    }
}
