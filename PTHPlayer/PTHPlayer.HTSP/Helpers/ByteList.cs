using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace PTHPlayer.HTSP.Helpers
{
    public class ByteList
    {
        private readonly List<byte> _data;

        public ByteList()
        {
            _data = new List<byte>();
        }

        public byte[] getFromStart(int count, CancellationToken cancellationToken)
        {
            lock (_data)
            {
                while (_data.Count < count && !cancellationToken.IsCancellationRequested)
                {
                    Monitor.Wait(_data, 2000);
                }

                if(cancellationToken.IsCancellationRequested)
                {
                    return new byte[0];
                }

                //byte[] result = new byte[count];
                byte[] result = _data.GetRange(0, count).ToArray();
                /*for (int ii = 0; ii < count; ii++)
                {
                    result[ii] = _data[ii];
                }*/
                return result;
            }
        }

        public byte[] extractFromStart(long count)
        {
            lock (_data)
            {
                while (_data.Count < count)
                {
                    Monitor.Wait(_data);
                }
                byte[] result = new byte[count];
                result = _data.GetRange(0, (int)count).ToArray();
                _data.RemoveRange(0, (int)count);
                /*
                for (int ii = 0; ii < count; ii++)
                {
                    result[ii] = _data[0];
                    _data.RemoveAt(0);
                }*/
                return result;
            }
        }

        public void appendAll(byte[] data)
        {
            lock (_data)
            {
                _data.AddRange(data);
                if (_data.Count >= 1)
                {
                    // wake up any blocked dequeue
                    Monitor.PulseAll(_data);
                }
            }
        }

        public void appendCount(byte[] data, long count)
        {
            lock (_data)
            {
                var byteArray = new List<byte>(data).Take((int)count);
                _data.AddRange(byteArray);
                /*
                for (long ii = 0; ii < count; ii++)
                {
                    _data.Add(data[ii]);
                }*/
                if (_data.Count >= 1)
                {
                    // wake up any blocked dequeue
                    Monitor.PulseAll(_data);
                }
            }
        }

        public int Count()
        {
            lock (_data)
            {
                return _data.Count;
            }
        }
    }
}
