using System;

namespace PTHPlayer.HTSP
{
    public interface IHTSConnectionListener
    {
        void OnMessage(HTSMessage response);
        void OnError(Exception ex);
    }
}
