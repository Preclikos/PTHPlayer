using PTHPlayer.HTSP.Helpers;
using System.Threading;

namespace PTHPlayer.HTSP.Responses
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

        public HTSMessage getResponse(CancellationToken cancellationToken)
        {
            return _responseDataQueue.Dequeue(cancellationToken);
        }
    }
}
