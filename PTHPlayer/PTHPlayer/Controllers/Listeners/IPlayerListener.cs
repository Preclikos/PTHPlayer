using PTHPlayer.HTSP;

namespace PTHPlayer.Controllers.Listeners
{
    public interface IPlayerListener
    {
        void OnSubscriptionStart(HTSMessage message);
        void OnSubscriptionStop(HTSMessage message);
        void OnSubscriptionSkip(HTSMessage message);
        void OnMuxPkt(HTSMessage message);
    }
}
