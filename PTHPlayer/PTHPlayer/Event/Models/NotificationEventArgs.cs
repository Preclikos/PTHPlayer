using PTHPlayer.Event.Enums;
using System;

namespace PTHPlayer.Event.Models
{
    public class NotificationEventArgs : EventArgs
    {
        public EventId Id { get; set; }
        public EventType Type { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
    }
}
