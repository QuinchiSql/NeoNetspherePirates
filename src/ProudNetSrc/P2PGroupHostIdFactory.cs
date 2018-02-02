using System.Collections.Concurrent;
using System.Threading;

namespace ProudNetSrc
{
    public interface IP2PGroupHostIdFactory
    {
        uint New();
        void Free(uint hostId);
    }

    public class P2PGroupHostIdFactory : IP2PGroupHostIdFactory
    {
        private readonly ConcurrentStack<uint> _pool = new ConcurrentStack<uint>();
        private long _counter = 20000;

        public uint New()
        {
            uint hostId;
            return _pool.TryPop(out hostId) ? hostId : (uint) Interlocked.Increment(ref _counter);
        }

        public void Free(uint hostId)
        {
            _pool.Push(hostId);
        }
    }
}
