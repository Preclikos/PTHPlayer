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
        public string Message;

        public HTSPConnectionStateChangeArgs(ConnectionState connectionState )
        {
            ConnectionChangeState = connectionState;
        }
        public HTSPConnectionStateChangeArgs(ConnectionState connectionState, string message)
        {
            ConnectionChangeState = connectionState;
            Message = message;
        }
    }
}
