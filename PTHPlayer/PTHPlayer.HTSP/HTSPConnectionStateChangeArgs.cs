using System;

namespace PTHPlayer.HTSP
{
    public enum ConnectionState
    {
        Disconnected,
        Connecting,
        ConnectingFail,
        Connected,
        Reconnecting,
        Authenticated,
        Authenticating,
        AuthenticatingFail
    }
    public class HTSPConnectionStateChangeArgs : EventArgs
    {
        public ConnectionState ConnectionChangeState;
        public HTSPConnectionStateChangeArgs(ConnectionState connectionState )
        {
            ConnectionChangeState = connectionState;
        }
    }
}
