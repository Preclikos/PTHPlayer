using PTHPlayer.Event.Enums;

namespace PTHPlayer.Event.Listeners
{
    public interface IEventListener
    {
        void SendNotification(string title, string message, EventId eventId = EventId.Generic, EventType eventType = EventType.Info);
    }
}
