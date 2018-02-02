using DotNetty.Transport.Channels;
using ProudNetSrc;

namespace NeoNetsphere.Network
{
    internal class ChatSession : ProudSession
    {
        public ChatSession(uint hostId, IChannel channel)
            : base(hostId, channel)
        {
        }

        public GameSession GameSession { get; set; }
        public Player Player => GameSession.Player;
    }

    internal class ChatSessionFactory : ISessionFactory
    {
        public ProudSession Create(uint hostId, IChannel channel)
        {
            return new ChatSession(hostId, channel);
        }
    }
}
