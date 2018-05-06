using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlubLib.DotNetty.Handlers.MessageHandling;
using BlubLib.Threading.Tasks;
using Dapper.FastCrud;
using ExpressMapper.Extensions;
using NeoNetsphere.Database.Auth;
using NeoNetsphere.Database.Game;
using NeoNetsphere.Network.Data.Chat;
using NeoNetsphere.Network.Data.Club;
using NeoNetsphere.Network.Data.Game;
using NeoNetsphere.Network.Message.Chat;
using NeoNetsphere.Network.Message.Club;
using NeoNetsphere.Network.Message.Game;
using Netsphere.Game.Systems;
using ProudNetSrc.Handlers;
using Serilog;
using Serilog.Core;

namespace NeoNetsphere.Network.Services
{
    internal class ClubService : ProudMessageHandler
    {
        // ReSharper disable once InconsistentNaming
        private static readonly ILogger Logger =
            Log.ForContext(Constants.SourceContextPropertyName, nameof(ClubService));

        private readonly AsyncLock _sync = new AsyncLock();

        public static async Task Update(GameSession session = null, bool broadcast = false)
        {
            if (session == null && broadcast == false)
                return;
            var targets = new List<GameSession>();
            if (broadcast)
                targets.AddRange(GameServer.Instance.Sessions.Values.Cast<GameSession>());
            else
                targets.Add(session);

            foreach (var proudSession in targets)
            {
                var plr = proudSession?.Player;

                if (plr != null)
                {
                    Club.LogOff(plr, true);
                    plr.Club = GameServer.Instance.ClubManager.GetClubByAccount(plr.Account.Id);
                    await proudSession?.SendAsync(new ClubMyInfoAckMessage(plr.Map<Player, MyInfoDto>()));
                    Club.LogOn(plr, true);
                }
            }

            foreach (var channel in GameServer.Instance.ChannelManager)
            {
                foreach (var room in channel.RoomManager.Where(x => x.Players.Any(y => y.Value.Club?.Id != 0)))
                {
                    room.Broadcast(new RoomPlayerInfoListForEnterPlayerAckMessage(room.TeamManager.Players
                        .Select(r => r.Map<Player, RoomPlayerDto>()).ToArray()));
                    var clubList = new List<PlayerClubInfoDto>();

                    foreach (var player in room.TeamManager.Players.Where(p => p.Club != null))
                        if (clubList.All(club => club.Id != player.Club.Id))
                            clubList.Add(player.Map<Player, PlayerClubInfoDto>());
                    
                    room.Broadcast(new RoomClubInfoListForEnterPlayerAckMessage(clubList.ToArray()));
                }
            }
        }

        [MessageHandler(typeof(ClubClubInfoReqMessage))]
        public void ClubClubInfoReq(GameSession session, ClubClubInfoReqMessage message)
        {
            var plr = session.Player;
            if (plr == null)
                return;
            session.SendAsync(new ClubClubInfoAckMessage(plr.Map<Player, ClubInfoDto>()));
        }

        [MessageHandler(typeof(ClubClubInfoReq2Message))]
        public void ClubClubInfoReq2(GameSession session, ClubClubInfoReq2Message message)
        {
            var plr = session.Player;
            if (plr == null)
                return;
            session.SendAsync(new ClubClubInfoAck2Message(plr.Map<Player, ClubInfoDto2>()));
        }

        [MessageHandler(typeof(ClubInfoReqMessage))]
        public void ClubInfoReq(GameSession session, ClubInfoReqMessage message)
        {
            var plr = session.Player;
            if (plr == null)
                return;
            session.SendAsync(new ClubInfoAckMessage(plr.Map<Player, PlayerClubInfoDto>()));
        }

        [MessageHandler(typeof(ClubJoinWaiterInfoReqMessage))]
        public void ClubJoinWaiterInfoReq(GameSession session, ClubJoinWaiterInfoReqMessage message)
        {
            //Todo
            session.SendAsync(new ClubJoinWaiterInfoAckMessage());
        }

        [MessageHandler(typeof(ClubStuffListReqMessage))]
        public void ClubStuffListReq(GameSession session, ClubStuffListReqMessage message)
        {
            //Todo
            session.SendAsync(new ClubStuffListAckMessage());
        }

        [MessageHandler(typeof(ClubStuffListReq2Message))]
        public void ClubStuffListReq2(GameSession session, ClubStuffListReq2Message message)
        {
            //Todo
            session.SendAsync(new ClubStuffListAck2Message());
        }

        [MessageHandler(typeof(ClubSearchReqMessage))]
        public void ClubSearchReq(GameSession session, ClubSearchReqMessage message)
        {
            //Todo
            session.SendAsync(new ClubSearchAckMessage {Unk1 = 1});
        }

        [MessageHandler(typeof(ClubNameCheckReqMessage))]
        public void ClubNameCheckReq(GameSession session, ClubNameCheckReqMessage message)
        {
            //Todo
            session.SendAsync(GameServer.Instance.ClubManager.Any(c => c.ClanName == message.Name)
                ? new ClubNameCheckAckMessage(2)
                : new ClubNameCheckAckMessage(0));
        }

        [MessageHandler(typeof(ClubCreateReqMessage))]
        public void ClubCreateReq(GameSession session, ClubCreateReqMessage message)
        {
            ClubCreateReq2(session, message.Map<ClubCreateReqMessage, ClubCreateReq2Message>());
        }

        [MessageHandler(typeof(ClubCreateReq2Message))]
        public void ClubCreateReq2(GameSession session, ClubCreateReq2Message message)
        {
            var plr = session.Player;
            if (plr == null)
                return;

            if (GameServer.Instance.ClubManager.Any(c =>
                c.ClanName == message.Name || c.Players.ContainsKey(plr.Account.Id)))
            {
                Logger.Information($" {plr.Account.Nickname} : Couldnt create Clan : {message.Name}");
                session.SendAsync(new ClubCreateAck2Message(1));
            }
            else
            {
                //using (_sync.Lock())
                {
                    var clubDto = new ClubDto
                    {
                        Name = message.Name,
                        Icon = ""
                    };

                    using (var db = GameDatabase.Open())
                    {
                        try
                        {
                            using (var transaction = db.BeginTransaction())
                            {
                                var playerAcc = db.Find<AccountDto>(statement => statement
                                    .Where($"{nameof(AccountDto.Id):C} = @Id")
                                    .WithParameters(new {session.Player.Account.Id})).FirstOrDefault();

                                db.Insert(clubDto, statement => statement.AttachToTransaction(transaction));

                                var club = new Club(clubDto, new[]
                                {
                                    new ClubPlayerInfo
                                    {
                                        AccountId = session.Player.Account.Id,
                                        Account = playerAcc,
                                        State = ClubState.Member,
                                        IsMod = true
                                    }
                                });
                                GameServer.Instance.ClubManager.Add(club);
                                transaction.Commit();

                                db.Insert(new ClubPlayerDto
                                {
                                    PlayerId = (int) session.Player.Account.Id,
                                    ClubId = club.Id,
                                    IsMod = true,
                                    State = (int) ClubState.Member
                                });

                                session.Player.Club = club;
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex.ToString());
                            session.SendAsync(new ClubCreateAck2Message(1));
                            return;
                        }

                        session.SendAsync(new ClubCreateAck2Message(0));
                        session.SendAsync(new ClubMyInfoAckMessage(plr.Map<Player, MyInfoDto>()));
                        Club.LogOn(plr);
                    }
                }
            }
        }

        [MessageHandler(typeof(ClubRankListReqMessage))]
        public void ClubRankListReq(GameSession session, ClubRankListReqMessage message)
        {
            //Todo
            var plr = session.Player;
            if (plr == null)
                return;
            session.SendAsync(new ClubRankListAckMessage());
        }

        [MessageHandler(typeof(ClubAddressReqMessage))]
        public void CClubAddressReq(GameSession session, ClubAddressReqMessage message)
        {
            //Todo
            session.SendAsync(new ClubAddressAckMessage("", 0));
        }

        [MessageHandler(typeof(ClubClubMemberInfoReq2Message))]
        public void ClubClubMemberInfoReq2(ChatSession session, ClubClubMemberInfoReq2Message message)
        {
            var targetplr = GameServer.Instance.PlayerManager[message.AccountId];
            if (targetplr?.Club != null)
            {
                var isMod = targetplr.Club.Players.Any(x => x.Value.IsMod && x.Key == targetplr.Account.Id);
                session.SendAsync(new ClubClubMemberInfoAck2Message
                {
                    ClanId = message.ClanId,
                    AccountId = targetplr.Account.Id,
                    Nickname = targetplr.Account.Nickname,
                    IsModerator = isMod ? 1 : 0
                });
            }
            else if (session.Player != null && targetplr != null)
            {
                session.SendAsync(new ClubClubMemberInfoAck2Message
                {
                    ClanId = message.ClanId,
                    AccountId = targetplr.Account.Id,
                    Nickname = targetplr.Account.Nickname
                });
            }
            else
            {
                session.SendAsync(new ClubClubMemberInfoAck2Message
                {
                    ClanId = message.ClanId,
                    AccountId = 0,
                    Nickname = "n/A"
                });
            }
        }

        [MessageHandler(typeof(ClubMemberListReqMessage))]
        public void ClubMemberListReq(ChatSession session, ClubMemberListReqMessage message)
        {
            var plr = session.Player;
            if (plr?.Club != null)
            {
                var clanMembers = new List<ClubMemberDto>();

                clanMembers.AddRange(GameServer.Instance.PlayerManager
                    .Where(p => plr.Club.Players.Keys.Contains(p.Account.Id))
                    .Select(p => p.Map<Player, ClubMemberDto>()));
                clanMembers.AddRange(plr.Club.Players.Select(x => x.Value.Map<ClubPlayerInfo, ClubMemberDto>()));

                plr.ChatSession.SendAsync(new ClubMemberListAckMessage(plr.Club.Id, clanMembers.ToArray()));
            }
            else
            {
                plr?.ChatSession.SendAsync(new ClubMemberListAckMessage());
            }
        }

        [MessageHandler(typeof(ClubMemberListReq2Message))]
        public void ClubMemberListReq2(ChatSession session, ClubMemberListReq2Message message)
        {
            var plr = session.Player;
            if (plr?.Club != null)
            {
                var clanMembers = new List<ClubMemberDto>();

                clanMembers.AddRange(GameServer.Instance.PlayerManager
                    .Where(p => plr.Club.Players.Keys.Contains(p.Account.Id))
                    .Select(p => p.Map<Player, ClubMemberDto>()));
                clanMembers.AddRange(plr.Club.Players.Select(x => x.Value.Map<ClubPlayerInfo, ClubMemberDto>()));

                plr.ChatSession.SendAsync(new ClubMemberListAck2Message(plr.Club.Id, clanMembers.ToArray()));
            }
            else
            {
                plr?.ChatSession.SendAsync(new ClubMemberListAck2Message());
            }
        }

        [MessageHandler(typeof(ClubNoteSendReq2Message))]
        public void ClubNoteSendReq2(ChatSession session, ClubNoteSendReq2Message message)
        {
            //Todo
            session.GameSession?.SendAsync(new ClubNoteSendAckMessage {Unk = 1});
        }

        [MessageHandler(typeof(ClubNoticePointRefreshReqMessage))]
        public void ClubNoticePointRefreshReq(GameSession session, ClubNoticePointRefreshReqMessage message)
        {
            //Todo
            session.SendAsync(new ClubNoticePointRefreshAckMessage());
        }

        [MessageHandler(typeof(ClubNoticeRecordRefreshReqMessage))]
        public void ClubNoticeRecordRefreshReq(GameSession session, ClubNoticeRecordRefreshReqMessage message)
        {
            //Todo
            session.SendAsync(new ClubNoticeRecordRefreshAckMessage());
        }


        [MessageHandler(typeof(ClubUnjoinReqMessage))]
        public void ClubUnjoinReq(GameSession session, ClubUnjoinReqMessage message)
        {
            ClubUnjoinReq2(session, message.Map<ClubUnjoinReqMessage, ClubUnjoinReq2Message>());
        }

        [MessageHandler(typeof(ClubUnjoinReq2Message))]
        public void ClubUnjoinReq2(GameSession session, ClubUnjoinReq2Message message)
        {
            var plr = session.Player;
            if (plr?.Club == null || plr.Club.Id != message.ClanId)
            {
                session.SendAsync(new ClubUnjoinAck2Message(4));
                return;
            }

            //using (_sync.Lock())
            {
                if (plr.Club.Players.Values.Any(x => x.Account?.Id == (int) plr.Account.Id && !x.IsMod))
                {
                    using (var db = GameDatabase.Open())
                    {
                        var club = db.Find<ClubDto>(statement => statement
                            .Where($"{nameof(ClubDto.Id):C} = @Id")
                            .WithParameters(new {plr.Club.Id})).FirstOrDefault();

                        if (club != null)
                        {
                            var player = db.Find<ClubPlayerDto>(statement => statement
                                    .Where($"{nameof(ClubPlayerDto.ClubId):C} = @Id")
                                    .WithParameters(new {plr.Club.Id}))
                                .FirstOrDefault(x => x.PlayerId == (int) plr.Account.Id);

                            if (player != null)
                            {
                                Club.LogOff(plr);
                                plr.Club.Players.TryRemove(plr.Account.Id, out var _);
                                db.Delete(new ClubPlayerDto() { PlayerId = player.PlayerId });
                                plr.Club = null;
                                session.SendAsync(new ClubMyInfoAckMessage(plr.Map<Player, MyInfoDto>()));
                                session.SendAsync(new ClubUnjoinAck2Message());
                            }
                            else
                            {
                                session.SendAsync(new ServerResultAckMessage(ServerResult.CantReadClanInfo));
                            }
                        }
                        else
                        {
                            session.SendAsync(new ServerResultAckMessage(ServerResult.CantReadClanInfo));
                        }
                    }
                }
                else
                {
                    session.SendAsync(new ServerResultAckMessage(ServerResult.DBError));
                }
            }
        }

        [MessageHandler(typeof(ClubCloseReqMessage))]
        public void ClubCloseReq(GameSession session, ClubCloseReqMessage message)
        {
            ClubCloseReq2(session, message.Map<ClubCloseReqMessage, ClubCloseReq2Message>());
        }

        [MessageHandler(typeof(ClubCloseReq2Message))]
        public void ClubCloseReq2(GameSession session, ClubCloseReq2Message message)
        {
            var plr = session.Player;
            if (plr?.Club == null || plr.Club.Id != message.ClanId)
            {
                session.SendAsync(new ClubCloseAck2Message(1));
                return;
            }

            //using (_sync.Lock())
            {
                if (plr.Club.Players.Any(x => x.Key == plr.Account.Id && x.Value.IsMod))
                {
                    using (var db = GameDatabase.Open())
                    {
                        var club = db.Find<ClubDto>(statement => statement
                            .Where($"{nameof(ClubDto.Id):C} = @Id")
                            .WithParameters(new {plr.Club.Id})).FirstOrDefault();

                        if (club != null)
                        {
                            var players = db.Find<ClubPlayerDto>(statement => statement
                                .Where($"{nameof(ClubPlayerDto.ClubId):C} = @Id")
                                .WithParameters(new {plr.Club.Id}));

                            foreach (var member in players) db.Delete(member);
                            db.Delete(club);
                            foreach (var member in plr.Club.Players)
                            {
                                plr.Club.Players.TryRemove(member.Key, out _);
                            }
                            GameServer.Instance.ClubManager.Remove(plr.Club);

                            session.SendAsync(new ClubCloseAck2Message());
                            foreach (var member in GameServer.Instance.PlayerManager.Where(x => x.Club?.Id == club.Id))
                            {
                                Club.LogOff(member);
                                member.Club = null;
                                member.Session?.SendAsync(new ClubMyInfoAckMessage(member.Map<Player, MyInfoDto>()));
                            }
                        }
                    }
                }
            }
        }
    }
}
