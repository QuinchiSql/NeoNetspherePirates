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
using Dapper.FastCrud;
using ExpressMapper.Extensions;
using NeoNetsphere.Database.Auth;
using NeoNetsphere.Database.Game;
using NeoNetsphere.Network.Data.Chat;
using ProudNetSrc;

namespace NeoNetsphere.Network.Services
{
    internal class ClubService : ProudMessageHandler
    {
        // ReSharper disable once InconsistentNaming
        private static readonly ILogger Logger =
            Log.ForContext(Constants.SourceContextPropertyName, nameof(ClubService));

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
                    plr.Club = GameServer.Instance.ClubManager.GetClubByAccount(plr.Account.Id);
                    await proudSession?.SendAsync(new Network.Message.Game.ClubInfoAckMessage(plr.Map<Player, PlayerClubInfoDto>()));
                    await proudSession?.SendAsync(new ClubMyInfoAckMessage(plr.Map<Player, MyInfoDto>()));
                }
            }
        }

        [MessageHandler(typeof(ClubClubInfoReq2Message))]
        public void ClubClubInfoReq2(GameSession session, ClubClubInfoReq2Message message)
        {
            var plr = session.Player;
            session.SendAsync(new ClubClubInfoAck2Message(plr.Map<Player, ClubSearchInfoDto>()));
        }

        [MessageHandler(typeof(ClubClubInfoReqMessage))]
        public void ClubClubInfoReq(GameSession session, ClubClubInfoReqMessage message)
        {
            var plr = session.Player;
            session.SendAsync(new ClubClubInfoAckMessage(plr.Map<Player, ClubSearchInfoDto>()));
        }

        [MessageHandler(typeof(ClubMemberListReqMessage))]
        public void ClubMemberListReq(ChatSession session, ClubMemberListReqMessage message)
        {
            var plr = session.Player;
            session.GameSession?.SendAsync(new Network.Message.Game.ClubInfoAckMessage(plr.Map<Player, PlayerClubInfoDto>()));
            session.GameSession?.SendAsync(new ClubMyInfoAckMessage(plr.Map<Player, MyInfoDto>()));
            session.SendAsync(new ClubMemberListAckMessage());
            //Todo
            //if (plr?.Club != null)
            //    plr.ChatSession.SendAsync(new ClubMemberListAckMessage(GameServer.Instance.PlayerManager.Where(p =>
            //        plr.Club.Players.Keys.Contains(p.Account.Id)).Select(p => p.Map<Player, ClubMemberDto>()).ToArray()));
        }

        [MessageHandler(typeof(ClubInfoReqMessage))]
        public void ClubInfoReq(GameSession session, ClubInfoReqMessage message)
        {
            var plr = session.Player;
            session.SendAsync(new Message.Game.ClubInfoAckMessage(plr.Map<Player, PlayerClubInfoDto>()));
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
            session.SendAsync(new ClubSearchAckMessage() {Unk1 = 1});
        }
        
        [MessageHandler(typeof(ClubNameCheckReqMessage))]
        public void ClubNameCheckReq(GameSession session, ClubNameCheckReqMessage message)
        {
            //Todo
            if (GameServer.Instance.ClubManager.Any(c => c.Clan_Name == message.Unk))
                session.SendAsync(new ClubNameCheckAckMessage(2));
            else
                session.SendAsync(new ClubNameCheckAckMessage(0));
        }
        
        [MessageHandler(typeof(ClubCreateReqMessage))]
        public async Task ClubCreateReq(GameSession session, ClubCreateReqMessage message)
        {
            await session.SendAsync(new ClubCreateAckMessage(1));
            return;
            if (GameServer.Instance.ClubManager.Any(c => c.Clan_Name == message.Unk1) || session.Player.Club != null)
                await session.SendAsync(new ClubCreateAckMessage(1));
            else
            {
                var club = new ClubDto()
                {
                    Name = message.Unk2,
                    Icon = ""
                };

                using (var db = GameDatabase.Open())
                {
                    try
                    {
                        using (var transaction = db.BeginTransaction())
                        {
                            var playeracc = (db.Find<AccountDto>(statement => statement
                           .Where($"{nameof(AccountDto.Id):C} = @Id")
                           .WithParameters(new { session.Player.Account.Id }))).FirstOrDefault();

                            var plrdto = (db.Find<PlayerDto>(statement => statement
                           .Where($"{nameof(PlayerDto.Id):C} = @Id")
                           .WithParameters(new { session.Player.Account.Id }))).FirstOrDefault();

                            db.Insert(club, statement => statement.AttachToTransaction(transaction));
                            var Club = new Club(club, new[] { new ClubPlayerInfo() { AccountId = session.Player.Account.Id, account = playeracc, State = ClubState.Member, IsMod = true } });
                            GameServer.Instance.ClubManager.Add(Club);
                            transaction.Commit();

                            db.Insert(new ClubPlayerDto()
                            {
                                PlayerId = (int)session.Player.Account.Id,
                                ClubId = club.Id,
                                IsMod = true,
                                State = (int)ClubState.Member
                            });

                            session.Player.Club = Club;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex.ToString());
                        await session.SendAsync(new ClubCreateAckMessage(1));
                        return;
                    }

                    await session.SendAsync(new ClubCreateAckMessage(0));
                    await session.SendAsync(new ClubMyInfoAckMessage(session.Player.Map<Player, MyInfoDto>()));
                    await session.SendAsync(new Message.Game.ClubInfoAckMessage(session.Player.Club.Map<Club, PlayerClubInfoDto>()));
                }
            }
        }

        [MessageHandler(typeof(ClubCreateReq2Message))]
        public async Task ClubCreateReq2(GameSession session, ClubCreateReq2Message message)
        {
            await session.SendAsync(new ClubCreateAck2Message(1));
            return;
            if (GameServer.Instance.ClubManager.Any(c => c.Clan_Name == message.Unk1) || session.Player.Club != null)
                await session.SendAsync(new ClubCreateAck2Message(1));
            else
            {
                var club = new ClubDto()
                {
                    Name = message.Unk2,
                    Icon = ""
                };

                using (var db = GameDatabase.Open())
                {
                    try
                    {
                        using (var transaction = db.BeginTransaction())
                        {
                            var playeracc = (db.Find<AccountDto>(statement => statement
                           .Where($"{nameof(AccountDto.Id):C} = @Id")
                           .WithParameters(new { session.Player.Account.Id }))).FirstOrDefault();

                            var plrdto = (db.Find<PlayerDto>(statement => statement
                           .Where($"{nameof(PlayerDto.Id):C} = @Id")
                           .WithParameters(new { session.Player.Account.Id }))).FirstOrDefault();

                            db.Insert(club, statement => statement.AttachToTransaction(transaction));
                            var Club = new Club(club, new[] { new ClubPlayerInfo() { AccountId = session.Player.Account.Id, account = playeracc, State = ClubState.Member, IsMod = true } });
                            GameServer.Instance.ClubManager.Add(Club);
                            transaction.Commit();

                            db.Insert(new ClubPlayerDto()
                            {
                                PlayerId = (int)session.Player.Account.Id,
                                ClubId = club.Id,
                                IsMod = true,
                                State = (int)ClubState.Member
                            });

                            session.Player.Club = Club;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex.ToString());
                        await session.SendAsync(new ClubCreateAck2Message(1));
                        return;
                    }

                    await session.SendAsync(new ClubCreateAck2Message(0));
                    await session.SendAsync(new ClubMyInfoAckMessage(session.Player.Map<Player, MyInfoDto>()));
                    await session.SendAsync(new Message.Game.ClubInfoAckMessage(session.Player.Club.Map<Club, PlayerClubInfoDto>()));
                }
            };
        }

        [MessageHandler(typeof(ClubRankListReqMessage))]
        public void ClubRankListReq(GameSession session, ClubRankListReqMessage message)
        {
            //Todo
            var plr = session.Player;
            session.SendAsync(new Network.Message.Game.ClubInfoAckMessage(plr.Map<Player, PlayerClubInfoDto>()));
            session.SendAsync(new ClubMyInfoAckMessage(plr.Map<Player, MyInfoDto>()));
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
            //Todo
            var targetplr = GameServer.Instance.PlayerManager[message.AccountId];
            if (session.Player?.Club != null && targetplr != null)
            {
                session.SendAsync(new ClubClubMemberInfoAck2Message()
                {
                    ClanId = message.ClanId,
                    AccountId = targetplr.Account.Id,
                    Nickname = targetplr.Account.Nickname,
                });
            }
            else if(session.Player != null && targetplr != null)
            {
                session.SendAsync(new ClubClubMemberInfoAck2Message()
                {
                    ClanId = message.ClanId,
                    AccountId = targetplr.Account.Id,
                    Nickname = targetplr.Account.Nickname,
                });
            }
            else
            {
                session.SendAsync(new ClubClubMemberInfoAck2Message()
                {
                    ClanId = message.ClanId,
                    AccountId = 0,
                    Nickname = "",
                });
            }
        }
        
        [MessageHandler(typeof(ClubMemberListReq2Message))]
        public void ClubMemberListReq2(ChatSession session, ClubMemberListReq2Message message)
        {
            var plr = session.Player;
            session.GameSession?.SendAsync(new Network.Message.Game.ClubInfoAckMessage(plr.Map<Player, PlayerClubInfoDto>()));
            session.GameSession?.SendAsync(new ClubMyInfoAckMessage(plr.Map<Player, MyInfoDto>()));
            session.SendAsync(new ClubMemberListAck2Message());
            //Todo
            //if (plr?.Club != null)
            //    plr.ChatSession.SendAsync(new ClubMemberListAck2Message(GameServer.Instance.PlayerManager.Where(p =>
            //        plr.Club.Players.Keys.Contains(p.Account.Id)).Select(p => p.Map<Player, ClubMemberDto>()).ToArray()));
        }

        [MessageHandler(typeof(ClubNoteSendReq2Message))]
        public void ClubNoteSendReq2(ChatSession session, ClubNoteSendReq2Message message)
        {
            //Todo
            session.GameSession?.SendAsync(new ClubNoteSendAckMessage() {Unk = 1});
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
    }
}
