using DotNetty.Transport.Channels;
using ProudNetSrc;

namespace NeoNetsphere.Network
{
    internal class GameSession : ProudSession
    {
        //public ChatSession ChatSession { get; set; }

        public GameSession(uint hostId, IChannel channel)
            : base(hostId, channel)
        {
        }

        public Player Player { get; set; }
    }

    internal class GameSessionFactory : ISessionFactory
    {
        public ProudSession Create(uint hostId, IChannel channel)
        {
            return new GameSession(hostId, channel);
        }
    }
}
