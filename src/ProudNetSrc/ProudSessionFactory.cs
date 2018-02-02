using DotNetty.Transport.Channels;

namespace ProudNetSrc
{
    public class ProudSessionFactory : ISessionFactory
    {
        public ProudSession Create(uint hostId, IChannel channel)
        {
            return new ProudSession(hostId, channel);
        }
    }
}
