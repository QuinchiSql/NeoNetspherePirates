using System;
using NeoNetsphere;
using Netsphere.Game.Systems;

// ReSharper disable once CheckNamespace
namespace Netsphere
{
    internal class ChannelPlayerJoinedEventArgs : EventArgs
    {
        public ChannelPlayerJoinedEventArgs(Channel channel, Player player)
        {
            Channel = channel;
            Player = player;
        }

        public Channel Channel { get; }
        public Player Player { get; }
    }

    internal class ChannelPlayerLeftEventArgs : EventArgs
    {
        public ChannelPlayerLeftEventArgs(Channel channel, Player player)
        {
            Channel = channel;
            Player = player;
        }

        public Channel Channel { get; }
        public Player Player { get; }
    }

    internal class ChannelMessageEventArgs : EventArgs
    {
        public ChannelMessageEventArgs(Channel channel, Player player, string message)
        {
            Channel = channel;
            Player = player;
            Message = message;
        }

        public Channel Channel { get; }
        public Player Player { get; }
        public string Message { get; }
    }

    internal class RoomPlayerEventArgs : EventArgs
    {
        public RoomPlayerEventArgs(Player plr)
        {
            Player = plr;
        }

        public Player Player { get; }
    }

    internal class TeamChangedEventArgs : EventArgs
    {
        public TeamChangedEventArgs(PlayerTeam from, PlayerTeam to, Player player)
        {
            From = from;
            To = to;
            Player = player;
        }

        public PlayerTeam From { get; }
        public PlayerTeam To { get; }
        public Player Player { get; }
    }
}
