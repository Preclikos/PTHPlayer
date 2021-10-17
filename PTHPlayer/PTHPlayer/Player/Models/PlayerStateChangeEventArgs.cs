using PTHPlayer.Player.Enums;
using System;

namespace PTHPlayer.VideoPlayer.Models
{
    public class PlayerStateChangeEventArgs : EventArgs
    {
        public PlayerState State { get; set; }
    }
}
