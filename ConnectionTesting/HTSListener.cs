using PTHPlayer.HTSP;
using System;

namespace ConnectionTesting
{
    public class HTSListener : HTSConnectionListener
    {
        public void onError(Exception ex)
        {
           // throw new NotImplementedException();
        }

        public void onMessage(HTSMessage response)
        {
            //throw new NotImplementedException();
        }
    }
}
