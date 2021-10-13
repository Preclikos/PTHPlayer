using System;

namespace PTHPlayer.VideoPlayer.Models
{
    public class PlayerErrorEventArgs : EventArgs
    {
        public string ErrorMsg { get; set; }
    }
}
