﻿using PTHPlayer.HTSP.Helpers;

namespace PTHPlayer.HTSP.HTSP_Responses
{
    public class LoopBackResponseHandler : HTSResponseHandler
    {
        private readonly SizeQueue<HTSMessage> _responseDataQueue;

        public LoopBackResponseHandler()
        {
            _responseDataQueue = new SizeQueue<HTSMessage>(1);
        }

        public void handleResponse(HTSMessage response)
        {
            _responseDataQueue.Enqueue(response);
        }

        public HTSMessage getResponse()
        {
            return _responseDataQueue.Dequeue();
        }
    }
}
