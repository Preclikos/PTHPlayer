using PTHPlayer.VideoPlayer.Enums;
using System;

namespace PTHPlayer.VideoPlayer.Models
{
    public class PlayerErrorEventArgs : EventArgs
    {
        public PlayerErrorSource Source { get; set; }
        public string ErrorMessage { get; set; }
    }
}
