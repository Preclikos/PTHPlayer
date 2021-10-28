using System;

namespace PTHPlayer.HTSP
{
    public class HTSPErrorArgs : EventArgs
    {
        public string Message = string.Empty;
        public HTSPErrorArgs(string message = "")
        {
            Message = message;
        }
    }
}
