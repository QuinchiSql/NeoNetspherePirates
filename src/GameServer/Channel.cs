using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ExpressMapper.Extensions;
using NeoNetsphere.Network;
using NeoNetsphere.Network.Data.Chat;
using NeoNetsphere.Network.Message.Chat;
using NeoNetsphere.Network.Message.Game;
using Netsphere;

namespace NeoNetsphere
{
    internal class Channel
    {
        private readonly IDictionary<ulong, Player> _players = new ConcurrentDictionary<ulong, Player>();

        public Channel()
        {
            RoomManager = new RoomManager(this);
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int PlayerLimit { get; set; }
        public byte MinLevel { get; set; }
        public byte MaxLevel { get; set; }
        public Color Color { get; set; }
        public Color TooltipColor { get; set; }

        public IReadOnlyDictionary<ulong, Player> Players => (IReadOnlyDictionary<ulong, Player>) _players;
        public RoomManager RoomManager { get; }

        public void Update(TimeSpan delta)
        {
            RoomManager.Update(delta);
        }

        public void Join(Player plr, bool noMessage = false)
        {
            if (plr.Channel != null)
                throw new ChannelException("Player is already inside a channel");

            if (Players.Count >= PlayerLimit)
                throw new ChannelLimitReachedException();

            foreach (var playr in _players.Values.Where(p => p.LocationInfo.Invisible != true))
                playr.ChatSession.SendAsync(new ChannelEnterPlayerAckMessage(plr.Map<Player, PlayerInfoShortDto>()));


            _players.Add(plr.Account.Id, plr);
            plr.SentPlayerList = false;
            plr.Channel = this;

            if (!noMessage)
                plr.Session.SendAsync(new ServerResultAckMessage(ServerResult.ChannelEnter));
            OnPlayerJoined(new ChannelPlayerJoinedEventArgs(this, plr));

            plr.ChatSession.SendAsync(new NoteCountAckMessage((byte) plr.Mailbox.Count(mail => mail.IsNew), 0, 0));

            var visibleplayers = (IReadOnlyDictionary<ulong, Player>) plr.Channel.Players
                .Where(i => i.Value.LocationInfo.Invisible != true).ToDictionary(i => i.Key, i => i.Value);

            plr.ChatSession.SendAsync(new ChannelPlayerListAckMessage(visibleplayers.Values.Select(p => p.Map<Player, PlayerInfoShortDto>()).ToArray()));
            plr.ChatSession.SendAsync(new Chennel_PlayerNameTagList_AckMessage(visibleplayers.Values.Select(p => p.Map<Player, PlayerNameTagInfoDto>()).ToArray()));
            if (plr.Club != null)
                plr.ChatSession.SendAsync(new ClubMemberListAckMessage(GameServer.Instance.PlayerManager.Where(p =>
                    plr.Club.Players.Keys.Contains(p.Account.Id)).Select(p => p.Map<Player, ClubMemberDto>()).ToArray()));
        }

        public void Leave(Player plr, bool no_message = false)
        {
            if (plr.Channel != this)
                throw new ChannelException("Player is not in this channel");

            _players.Remove(plr.Account.Id);
            plr.Channel = null;

            foreach (var playr in _players.Values)
                if (playr.LocationInfo.Invisible != true)
                    Broadcast(new ChannelLeavePlayerAckMessage(plr.Account.Id));

            OnPlayerLeft(new ChannelPlayerLeftEventArgs(this, plr));
            if (!no_message)
                plr.Session?.SendAsync(new ServerResultAckMessage(ServerResult.ChannelLeave));
        }

        public void SendChatMessage(Player plr, string message)
        {
            OnMessage(new ChannelMessageEventArgs(this, plr, message));

            foreach (var p in Players.Values.Where(p => !p.DenyManager.Contains(plr.Account.Id) && p.Room == null))
                p.ChatSession?.SendAsync(new MessageChatAckMessage(ChatType.Channel, plr.Account.Id,
                    plr.Account.Nickname, message));
        }

        public void Broadcast(IGameMessage message, bool excludeRooms = false)
        {
            foreach (var plr in Players.Values.Where(plr => !excludeRooms || plr.Room == null))
                plr.Session?.SendAsync(message);
        }

        public void BroadcastCencored(RoomChangeRoomInfoAck2Message message)
        {
            foreach (var plr in Players.Values.Where(plr => plr.Room?.Id == message.Room.RoomId))
                plr.Session?.SendAsync(message).Wait();

            message.Room.Password = !string.IsNullOrWhiteSpace(message.Room.Password) || !string.IsNullOrEmpty(message.Room.Password) ? "nice try :)" : "";
            foreach (var plr in Players.Values.Where(plr => plr.Room?.Id != message.Room.RoomId || plr.Room == null))
            {
                plr.Session?.SendAsync(message);
            }
        }

        public void Broadcast(IChatMessage message, bool excludeRooms = false)
        {
            foreach (var plr in Players.Values.Where(plr => !excludeRooms || plr.Room == null))
                plr.ChatSession?.SendAsync(message);
        }

        #region Events

        public event EventHandler<ChannelPlayerJoinedEventArgs> PlayerJoined;
        public event EventHandler<ChannelPlayerLeftEventArgs> PlayerLeft;
        public event EventHandler<ChannelMessageEventArgs> Message;

        protected virtual void OnPlayerJoined(ChannelPlayerJoinedEventArgs e)
        {
            PlayerJoined?.Invoke(this, e);
        }

        protected virtual void OnPlayerLeft(ChannelPlayerLeftEventArgs e)
        {
            PlayerLeft?.Invoke(this, e);
        }

        protected virtual void OnMessage(ChannelMessageEventArgs e)
        {
            Message?.Invoke(this, e);
        }

        #endregion
    }
}
