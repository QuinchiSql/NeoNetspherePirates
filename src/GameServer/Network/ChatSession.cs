﻿using DotNetty.Transport.Channels;
using ProudNetSrc;

namespace NeoNetsphere.Network
{
    internal class ChatSession : ProudSession
    {
        public ChatSession(uint hostId, IChannel channel, ProudServer server)
            : base(hostId, channel, server)
        {
        }

        public GameSession GameSession { get; set; }
        public Player Player => GameSession.Player;
    }

    internal class ChatSessionFactory : ISessionFactory
    {
        public ProudSession Create(uint hostId, IChannel channel, ProudServer server)
        {
            return new ChatSession(hostId, channel, server);
        }
    }
}
