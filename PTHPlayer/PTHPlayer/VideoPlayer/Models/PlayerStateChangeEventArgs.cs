using PTHPlayer.VideoPlayer.Enums;
using System;

namespace PTHPlayer.VideoPlayer.Models
{
    public class PlayerStateChangeEventArgs : EventArgs
    {
        public PlayerStates State { get; set; }
    }
}
