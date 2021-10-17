using PTHPlayer.Player.Enums;
using System;

namespace PTHPlayer.VideoPlayer.Models
{
    public class PlayerErrorEventArgs : EventArgs
    {
        public PlayerErrorType Type { get; set; }
        public PlayerError PlayerError { get; set; }
        public BufferStatus BufferStatus { get; set; }
    }
}
