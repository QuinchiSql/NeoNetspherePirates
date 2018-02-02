using DotNetty.Transport.Channels;

namespace ProudNetSrc
{
    public interface ISessionFactory
    {
        ProudSession Create(uint hostId, IChannel channel);
    }
}
