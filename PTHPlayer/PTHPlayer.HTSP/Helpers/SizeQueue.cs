using System;
using System.Collections.Generic;
using System.Threading;

namespace PTHPlayer.HTSP.Helpers
{
    public class SizeQueue<T>
    {
        private readonly TimeSpan _timeOut = new TimeSpan(0, 0, 5);
        private readonly Queue<T> _queue = new Queue<T>();
        private readonly int _maxSize;
        public SizeQueue(int maxSize) { _maxSize = maxSize; }

        public void Enqueue(T item)
        {
            lock (_queue)
            {
                while (_queue.Count >= _maxSize)
                {
                    Monitor.Wait(_queue, _timeOut);
                }
                _queue.Enqueue(item);
                if (_queue.Count == 1)
                {
                    // wake up any blocked dequeue
                    Monitor.PulseAll(_queue);
                }
            }
        }

        public T Dequeue(CancellationToken cancellationToken)
        {
            lock (_queue)
            {
                while (_queue.Count == 0 && !cancellationToken.IsCancellationRequested)
                {
                    Monitor.Wait(_queue, _timeOut);
                }
                if(cancellationToken.IsCancellationRequested)
                {
                    return default(T);
                }

                T item = _queue.Dequeue();
                if (_queue.Count == _maxSize - 1)
                {
                    // wake up any blocked enqueue
                    Monitor.PulseAll(_queue);
                }
                return item;
            }
        }

        public void Clear()
        {
            _queue.Clear();
        }
    }
}
