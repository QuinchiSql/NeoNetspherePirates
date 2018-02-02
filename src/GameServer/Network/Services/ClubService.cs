using System.Collections.Generic;
using BlubLib.DotNetty.Handlers.MessageHandling;
using NeoNetsphere.Network.Data.Club;
using NeoNetsphere.Network.Data.Game;
using NeoNetsphere.Network.Message.Chat;
using NeoNetsphere.Network.Message.Club;
using NeoNetsphere.Network.Message.Game;
using ProudNetSrc.Handlers;
using Serilog;
using Serilog.Core;
using System;
using System.Linq;
using System.Threading.Tasks;
using ExpressMapper.Extensions;
using NeoNetsphere.Network.Data.Chat;
using ProudNetSrc;

namespace NeoNetsphere.Network.Services
{
    internal class ClubService : ProudMessageHandler
    {
        // ReSharper disable once InconsistentNaming
        private static readonly ILogger Logger =
            Log.ForContext(Constants.SourceContextPropertyName, nameof(ClubService));

        public static async Task Update(GameSession session = (GameSession) null, bool broadcast = false)
        {
            if (session == null && broadcast == false)
                return;
            var targets = new List<GameSession>();
            if (broadcast)
                foreach (var sessionsValue in GameServer.Instance.Sessions.Values)
                    targets.Add((GameSession)sessionsValue);
            else
                targets.Add(session);


            foreach (var proudSession in targets)
            {
                var plr = proudSession?.Player;

                if (plr != null)
                {
                    plr.Club = GameServer.Instance.ClubManager.GetClubByAccount(plr.Account.Id);
                    proudSession.SendAsync(new Network.Message.Game.ClubInfoAckMessage(plr.Map<Player, PlayerClubInfoDto>()));
                    proudSession.SendAsync(new ClubMyInfoAckMessage(plr.Map<Player, MyInfoDto>()));
                }
            }
        }

        [MessageHandler(typeof(Message.Club.ClubInfoReq2Message))]
        public void ClubInfoReq2(GameSession session, Message.Club.ClubInfoReq2Message message)
        {
            session.SendAsync(new ServerResultAckMessage(ServerResult.CantReadClanInfo));
            //var plr = session.Player;
            //
            //if (plr.Club != null)
            //{
            //    var x = plr.Club.Players.Values.Where(p => p.IsMod).FirstOrDefault();
            //    string nick = "";
            //    if (x != null)
            //        nick = x.account.Nickname;
            //
            //    session.SendAsync(new Message.Club.ClubInfoAck2Message(new ClubSearchInfoDto()
            //    {
            //        ID = plr.Club.Clan_ID,
            //        Name = plr.Club.Clan_Name,
            //        Type = plr.Club.Clan_Icon,
            //        MasterName = nick,
            //        MemberCount = plr.Club.Count,
            //        CreationDate = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss") //Todo
            //    }));
            //}
            //else
            //{
            //    session.SendAsync(new ServerResultAckMessage(ServerResult.CantReadClanInfo));
            //}
        }
        [MessageHandler(typeof(Message.Club.ClubInfoReqMessage))]
        public void ClubInfoReq(GameSession session, Message.Club.ClubInfoReqMessage message)
        {
            session.SendAsync(new ServerResultAckMessage(ServerResult.CantReadClanInfo));
            //var plr = session.Player;
            //
            //if (plr.Club != null)
            //{
            //    var x = plr.Club.Players.Values.Where(p => p.IsMod).FirstOrDefault();
            //    string nick = "";
            //    if (x != null)
            //        nick = x.account.Nickname;
            //
            //    session.SendAsync(new Message.Club.ClubInfoAck2Message(new ClubSearchInfoDto()
            //    {
            //        ID = plr.Club.Clan_ID,
            //        Name = plr.Club.Clan_Name,
            //        Type = plr.Club.Clan_Icon,
            //        MasterName = nick,
            //        MemberCount = plr.Club.Count,
            //        CreationDate = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss") //Todo
            //    }));
            //}
            //else
            //{
            //    session.SendAsync(new ServerResultAckMessage(ServerResult.CantReadClanInfo));
            //}
        }

        [MessageHandler(typeof(Message.Game.ClubInfoReqMessage))]
        public void ClubInfoReq(GameSession session, Message.Game.ClubInfoReqMessage message)
        {
            var plr = session.Player;
            session.SendAsync(new Message.Game.ClubInfoAckMessage(plr.Map<Player, PlayerClubInfoDto>()));
        }

        [MessageHandler(typeof(ClubSearchReqMessage))]
        public void CClubAddressReq(GameSession session, ClubSearchReqMessage message)
        {
            session.SendAsync(new ServerResultAckMessage(ServerResult.CantReadClanInfo));
            //Todo
        }


        [MessageHandler(typeof(ClubNameCheckReqMessage))]
        public void ClubNameCheckReq(GameSession session, ClubNameCheckReqMessage message)
        {
            session.SendAsync(new ServerResultAckMessage(ServerResult.CantReadClanInfo));
            //session.SendAsync(new ClubNameCheckAckMessage(0));
        }

        [MessageHandler(typeof(ClubCreateReqMessage))]
        public void ClubCreateReq(GameSession session, ClubCreateReqMessage message)
        {
            session.SendAsync(new ServerResultAckMessage(ServerResult.CantReadClanInfo));
            //session.SendAsync(new ClubCreateAckMessage(0));
        }

        [MessageHandler(typeof(ClubCreateReq2Message))]
        public void ClubCreateReq2(GameSession session, ClubCreateReq2Message message)
        {
            session.SendAsync(new ServerResultAckMessage(ServerResult.CantReadClanInfo));
            //session.SendAsync(new ClubCreateAck2Message(0));
        }

        [MessageHandler(typeof(ClubRankListReqMessage))]
        public void ClubRankListReq(GameSession session, ClubRankListReqMessage message)
        {
            session.SendAsync(new ServerResultAckMessage(ServerResult.CantReadClanInfo));
            //session.SendAsync(new ClubCreateAck2Message(0));
        }

        [MessageHandler(typeof(ClubAddressReqMessage))]
        public void CClubAddressReq(GameSession session, ClubAddressReqMessage message)
        {
            Logger.ForAccount(session)
                .Debug("ClubAddressReq: {message}", message);

            session.SendAsync(new ClubAddressAckMessage("", 0));
        }

        [MessageHandler(typeof(ClubClubMemberInfoReq2Message))]
        public void ClubClubMemberInfoReq2(ChatSession session, ClubClubMemberInfoReq2Message message)
        {
            if (session.GameSession != null)
                session.GameSession.SendAsync(new ServerResultAckMessage(ServerResult.CantReadClanInfo));
            //session.SendAsync(new ClubNewJoinMemberInfoAckMessage());
        }

        [MessageHandler(typeof(ClubMemberListReq2Message))]
        public void ClubMemberListReq2(ChatSession session, ClubMemberListReq2Message message)
        {
            //Todo Wrong Struct
            //var plr = session.Player;
            //if (plr.Club != null)
            //    plr.ChatSession.SendAsync(new ClubMemberListAck2Message(GameServer.Instance.PlayerManager.Where(p =>
            //        plr.Club.Players.Keys.Contains(p.Account.Id)).Select(p => p.Map<Player, ClubMemberDto>()).ToArray()));
            if (session.GameSession != null)
                session.GameSession.SendAsync(new ServerResultAckMessage(ServerResult.CantReadClanInfo));
        }

        [MessageHandler(typeof(ClubNoteSendReq2Message))]
        public void ClubNoteSendReq2(ChatSession session, ClubNoteSendReq2Message message)
        {
            if (session.GameSession != null)
                session.GameSession.SendAsync(new ServerResultAckMessage(ServerResult.CantReadClanInfo));
        }
    }
}
