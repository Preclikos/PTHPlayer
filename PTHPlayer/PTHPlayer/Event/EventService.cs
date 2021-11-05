using PTHPlayer.Event.Enums;
using PTHPlayer.Event.Listeners;
using PTHPlayer.Event.Models;
using System;

namespace PTHPlayer.Event
{
    public class EventService : IEventListener
    {
        public event EventHandler<NotificationEventArgs> EventHandler;

        public void SendNotification(string title, string message, EventId eventId = EventId.Generic, EventType eventType = EventType.Info)
        {
            var eventArgs = new NotificationEventArgs
            {
                Id = eventId,
                Type = eventType,
                Title = title,
                Message = message
            };

            EventHandler?.Invoke(this, eventArgs);
        }
    }
}
