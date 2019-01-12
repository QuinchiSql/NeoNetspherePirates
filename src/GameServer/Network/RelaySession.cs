using DotNetty.Transport.Channels;
using ProudNetSrc;

namespace NeoNetsphere.Network
{
    internal class RelaySession : ProudSession
    {
        public RelaySession(uint hostId, IChannel channel, ProudServer server)
            : base(hostId, channel, server)
        {
        }

        public GameSession GameSession { get; set; }
        public Player Player => GameSession?.Player;
    }

    internal class RelaySessionFactory : ISessionFactory
    {
        public ProudSession Create(uint hostId, IChannel channel, ProudServer server)
        {
            return new RelaySession(hostId, channel, server);
        }
    }
}
