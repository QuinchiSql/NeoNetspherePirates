using System.Net;

namespace ProudNetSrc
{
    public class RecvContext
    {
        public object Message { get; set; }
        public IPEndPoint UdpEndPoint { get; set; }
    }
}
