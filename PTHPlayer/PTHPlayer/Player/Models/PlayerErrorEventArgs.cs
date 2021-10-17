using PTHPlayer.Player.Enums;
using System;

namespace PTHPlayer.VideoPlayer.Models
{
    public class PlayerErrorEventArgs : EventArgs
    {
        public PlayerErrorType Type { get; set; }
        public string ErrorMessage { get; set; }
    }
}
