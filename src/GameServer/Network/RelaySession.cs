using DotNetty.Transport.Channels;
using ProudNetSrc;

namespace NeoNetsphere.Network
{
    internal class RelaySession : ProudSession
    {
        public RelaySession(uint hostId, IChannel channel)
            : base(hostId, channel)
        {
        }

        public GameSession GameSession { get; set; }
        public Player Player => GameSession?.Player;
    }

    internal class RelaySessionFactory : ISessionFactory
    {
        public ProudSession Create(uint hostId, IChannel channel)
        {
            return new RelaySession(hostId, channel);
        }
    }
}
