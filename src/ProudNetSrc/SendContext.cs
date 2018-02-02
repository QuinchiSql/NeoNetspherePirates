using System.Net;

namespace ProudNetSrc
{
    internal class SendContext
    {
        public SendContext()
        {
        }

        public SendContext(object message, SendOptions sendOptions)
        {
            Message = message;
            SendOptions = sendOptions;
        }

        public object Message { get; set; }
        public SendOptions SendOptions { get; set; }
        public IPEndPoint UdpEndPoint { get; set; }
    }
}
