using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using AuthServer.ServiceModel;
using BlubLib.DotNetty.Handlers.MessageHandling;
using BlubLib.Threading;
using ExpressMapper;
using ExpressMapper.Extensions;
using NeoNetsphere.Commands;
using NeoNetsphere.Network.Data.Chat;
using NeoNetsphere.Network.Data.Club;
using NeoNetsphere.Network.Data.Game;
using NeoNetsphere.Network.Data.GameRule;
using NeoNetsphere.Network.Message.Club;
using NeoNetsphere.Network.Message.Game;
using NeoNetsphere.Network.Message.GameRule;
using NeoNetsphere.Network.Message.Relay;
using NeoNetsphere.Network.Services;
using NeoNetsphere.Resource;
using Netsphere;
using ProudNetSrc;
using ProudNetSrc.Serialization;
using Serilog;
using Constants = Serilog.Core.Constants;

namespace NeoNetsphere.Network
{
    internal class GameServer : ProudServer
    {
        // ReSharper disable once InconsistentNaming
        private static readonly ILogger
            Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(GameServer));

        private readonly ServerlistManager _serverlistManager;

        private readonly ILoop _worker;
        private TimeSpan _saveTimer;

        private GameServer(Configuration config)
            : base(config)
        {
            RegisterMappings();
            CommandManager = new CommandManager(this);
            CommandManager.Add(new ServerCommand())
                .Add(new ReloadCommand())
                .Add(new GameCommands())
                .Add(new BanCommands())
                .Add(new UnbanCommands())
                .Add(new KickCommand())
                .Add(new UserkickCommand())
                .Add(new AdminCommands())
                .Add(new NoticeCommand())
                .Add(new ClanCommands())
                .Add(new InventoryCommands());

            PlayerManager = new PlayerManager();
            ResourceCache = new ResourceCache();
            ChannelManager = new ChannelManager(ResourceCache.GetChannels());
            ClubManager = new ClubManager(ResourceCache.GetClubs());

            _worker = new ThreadLoop(TimeSpan.FromMilliseconds(100), Worker);
            _serverlistManager = new ServerlistManager();
        }

        public static GameServer Instance { get; private set; }

        public CommandManager CommandManager { get; }
        public PlayerManager PlayerManager { get; }
        public ChannelManager ChannelManager { get; }
        public ClubManager ClubManager { get; set; }
        public ResourceCache ResourceCache { get; }

        public static void Initialize(Configuration config)
        {
            if (Instance != null)
                throw new InvalidOperationException("Server is already initialized");

#if LATESTS4
            config.Version = new Guid("{14229beb-3338-7114-ab92-9b4af78c688f}");
#else
            config.Version = new Guid("{beb92241-8333-4117-ab92-9b4af78c688f}");
#endif

#if OLDUI
            config.Version = new Guid("{beb92241-8333-4117-ab92-9b4af78c688f}");
#endif

            config.MessageFactories = new MessageFactory[]
            {
                new RelayMessageFactory(), new GameMessageFactory(), new GameRuleMessageFactory(),
                new ClubMessageFactory()
            };
            config.SessionFactory = new GameSessionFactory();

            // ReSharper disable InconsistentNaming
            bool MustBeLoggedIn(GameSession session)
            {
                return session.IsLoggedIn();
            }

            bool MustNotBeLoggedIn(GameSession session)
            {
                return !session.IsLoggedIn();
            }

            bool MustBeInChannel(GameSession session)
            {
                return session.Player.Channel != null;
            }

            bool MustBeInRoom(GameSession session)
            {
                return session.Player.Room != null;
            }

            bool MustNotBeInRoom(GameSession session)
            {
                return session.Player.Room == null;
            }

            bool MustBeRoomHost(GameSession session)
            {
                return session.Player.Room.Host == session.Player;
            }

            bool MustBeRoomMaster(GameSession session)
            {
                return session.Player.Room.Master == session.Player;
            }
            // ReSharper restore InconsistentNaming

            config.MessageHandlers = new IMessageHandler[]
            {
                new FilteredMessageHandler<GameSession>()
                    .AddHandler(new AuthService())
                    .AddHandler(new CharacterService())
                    .AddHandler(new GeneralService())
                    .AddHandler(new AdminService())
                    .AddHandler(new ChannelService())
                    .AddHandler(new ShopService())
                    .AddHandler(new InventoryService())
                    .AddHandler(new RoomService())
                    .AddHandler(new ClubService())
                    .RegisterRule<LoginRequestReqMessage>(MustNotBeLoggedIn)
                    .RegisterRule<CharacterCreateReqMessage>(MustBeLoggedIn)
                    .RegisterRule<CharacterSelectReqMessage>(MustBeLoggedIn)
                    .RegisterRule<CharacterDeleteReqMessage>(MustBeLoggedIn)
                    .RegisterRule<AdminShowWindowReqMessage>(MustBeLoggedIn)
                    .RegisterRule<AdminActionReqMessage>(MustBeLoggedIn)
                    .RegisterRule<ChannelInfoReqMessage>(MustBeLoggedIn)
                    .RegisterRule<NewShopUpdateCheckReqMessage>(MustBeLoggedIn)
                    .RegisterRule<ChannelEnterReqMessage>(MustBeLoggedIn)
                    .RegisterRule<ChannelLeaveReqMessage>(MustBeLoggedIn, MustBeInChannel)
                    .RegisterRule<ItemBuyItemReqMessage>(MustBeLoggedIn)
                    .RegisterRule<RandomShopRollingStartReqMessage>(MustBeLoggedIn)
                    .RegisterRule<ItemUseItemReqMessage>(MustBeLoggedIn)
                    .RegisterRule<ItemRepairItemReqMessage>(MustBeLoggedIn)
                    .RegisterRule<ItemRefundItemReqMessage>(MustBeLoggedIn)
                    .RegisterRule<ItemDiscardItemReqMessage>(MustBeLoggedIn)
                    .RegisterRule<RoomQuickJoinReqMessage>(MustBeLoggedIn, MustBeInChannel, MustNotBeInRoom)
                    .RegisterRule<RoomEnterPlayerReqMessage>(MustBeLoggedIn, MustBeInChannel, MustBeInRoom)
                    .RegisterRule<RoomMakeReqMessage>(MustBeLoggedIn, MustBeInChannel, MustNotBeInRoom)
                    .RegisterRule<RoomMakeReq2Message>(MustBeLoggedIn, MustBeInChannel, MustNotBeInRoom)
                    .RegisterRule<RoomEnterReqMessage>(MustBeLoggedIn, MustBeInChannel, MustNotBeInRoom)
                    .RegisterRule<RoomLeaveReqMessage>(MustBeLoggedIn, MustBeInChannel, MustBeInRoom)
                    .RegisterRule<RoomTeamChangeReqMessage>(MustBeLoggedIn, MustBeInChannel, MustBeInRoom)
                    .RegisterRule<RoomPlayModeChangeReqMessage>(MustBeLoggedIn, MustBeInChannel, MustBeInRoom)
                    .RegisterRule<ScoreKillReqMessage>(MustBeLoggedIn, MustBeInChannel, MustBeInRoom)
                    .RegisterRule<ScoreKillAssistReqMessage>(MustBeLoggedIn, MustBeInChannel, MustBeInRoom)
                    .RegisterRule<ScoreOffenseReqMessage>(MustBeLoggedIn, MustBeInChannel, MustBeInRoom)
                    .RegisterRule<ScoreOffenseAssistReqMessage>(MustBeLoggedIn, MustBeInChannel, MustBeInRoom)
                    .RegisterRule<ScoreDefenseReqMessage>(MustBeLoggedIn, MustBeInChannel, MustBeInRoom)
                    .RegisterRule<ScoreDefenseAssistReqMessage>(MustBeLoggedIn, MustBeInChannel, MustBeInRoom)
                    .RegisterRule<ScoreTeamKillReqMessage>(MustBeLoggedIn, MustBeInChannel, MustBeInRoom)
                    .RegisterRule<ScoreHealAssistReqMessage>(MustBeLoggedIn, MustBeInChannel, MustBeInRoom)
                    .RegisterRule<ScoreSuicideReqMessage>(MustBeLoggedIn, MustBeInChannel, MustBeInRoom)
                    .RegisterRule<ScoreReboundReqMessage>(MustBeLoggedIn, MustBeInChannel, MustBeInRoom, MustBeRoomHost,
                        session => session.Player.RoomInfo.State != PlayerState.Lobby &&
                                   session.Player.RoomInfo.State != PlayerState.Spectating)
                    .RegisterRule<ScoreGoalReqMessage>(MustBeLoggedIn, MustBeInChannel, MustBeInRoom, MustBeRoomHost,
                        session => session.Player.RoomInfo.State != PlayerState.Lobby &&
                                   session.Player.RoomInfo.State != PlayerState.Spectating)
                    .RegisterRule<RoomBeginRoundReqMessage>(MustBeLoggedIn, MustBeInChannel, MustBeInRoom,
                        MustBeRoomMaster)
                    .RegisterRule<RoomReadyRoundReqMessage>(MustBeLoggedIn, MustBeInChannel, MustBeInRoom,
                        session => session.Player.RoomInfo.State == PlayerState.Lobby)
                    .RegisterRule<RoomBeginRoundReq2Message>(MustBeLoggedIn, MustBeInChannel, MustBeInRoom,
                        MustBeRoomMaster)
                    .RegisterRule<GameLoadingSuccessReqMessage>(MustBeLoggedIn, MustBeInChannel)
                    .RegisterRule<RoomReadyRoundReq2Message>(MustBeLoggedIn, MustBeInChannel, MustBeInRoom,
                        session => session.Player.RoomInfo.State == PlayerState.Lobby)
                    .RegisterRule<GameEventMessageReqMessage>(MustBeLoggedIn, MustBeInChannel, MustBeInRoom)
                    .RegisterRule<RoomItemChangeReqMessage>(MustBeLoggedIn, MustBeInChannel, MustBeInRoom,
                        session => session.Player.RoomInfo.State == PlayerState.Lobby)
                    .RegisterRule<GameAvatarChangeReqMessage>(MustBeLoggedIn, MustBeInChannel, MustBeInRoom,
                        session => session.Player.RoomInfo.State == PlayerState.Lobby ||
                                   session.Player.Room.GameRuleManager.GameRule.StateMachine.IsInState(
                                       GameRuleState.HalfTime))
                    .RegisterRule<RoomChangeRuleNotifyReqMessage>(MustBeLoggedIn, MustBeInChannel, MustBeInRoom,
                        MustBeRoomMaster,
                        session =>
                            session.Player.Room.GameRuleManager.GameRule.StateMachine.IsInState(GameRuleState.Waiting))
                    .RegisterRule<RoomChangeRuleNotifyReq2Message>(MustBeLoggedIn, MustBeInChannel, MustBeInRoom,
                        MustBeRoomMaster,
                        session =>
                            session.Player.Room.GameRuleManager.GameRule.StateMachine.IsInState(GameRuleState.Waiting))
                    .RegisterRule<ClubAddressReqMessage>(MustBeLoggedIn, MustBeInChannel)
                    .RegisterRule<RoomLeaveReguestReqMessage>(MustBeLoggedIn, MustBeInChannel, MustBeInRoom)
            };

            Instance = new GameServer(config);
        }

        public void BroadcastNotice(string message)
        {
            Broadcast(new NoticeAdminMessageAckMessage(message));
        }

        private void Worker(TimeSpan delta)
        {
            try
            {
                ChannelManager.Update(delta);
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }

            _saveTimer = _saveTimer.Add(delta);
            if (_saveTimer < Config.Instance.SaveInterval) return;
            {
                _saveTimer = TimeSpan.Zero;

                var players = PlayerManager.Where(plr => plr.IsLoggedIn());
                var enumerable = players as Player[] ?? players.ToArray();
                if (!enumerable.Any()) return;
                {
                    Logger.Information("Saving playerdata...");
                    foreach (var plr in enumerable)
                        try
                        {
                            plr.Save();
                        }
                        catch (Exception ex)
                        {
                            Logger.ForAccount(plr)
                                .Error(ex, "Failed to save playerdata");
                        }

                    Logger.Information("Saving playerdata completed");
                }
            }


            foreach (var plr in PlayerManager.Where(plr => !plr.IsLoggedIn() &&
                                                           plr.Session?.ConnectDate.Add(TimeSpan.FromMinutes(5)) <
                                                           DateTimeOffset.Now))
                plr.Disconnect();

            foreach (var channel in ChannelManager)
            {
                foreach (var room in channel.RoomManager)
                {
                    foreach (var player in room.TeamManager.Players)
                        if (!player.IsLoggedIn())
                            room.Leave(player);

                    if (!room.TeamManager.Any())
                        channel.RoomManager.Remove(room);
                }

                foreach (var player in channel.Players.Values)
                    if (!player.IsLoggedIn())
                        channel.Leave(player);
            }
        }

        private static void RegisterMappings()
        {
            Mapper.Register<GameServer, ServerInfoDto>()
                .Member(dest => dest.ApiKey, src => Config.Instance.AuthAPI.ApiKey)
                .Member(dest => dest.Id, src => Config.Instance.Id)
                .Member(dest => dest.Name,
                    src =>
                        $"{Config.Instance.Name}[{Program.GlobalVersion.Major}.{Program.GlobalVersion.Major / 2 + Program.GlobalVersion.Minor + Program.GlobalVersion.Build + Program.GlobalVersion.Revision}]")
                .Member(dest => dest.PlayerLimit, src => Config.Instance.PlayerLimit)
                .Member(dest => dest.PlayerOnline, src => src.Sessions.Count)
                .Member(dest => dest.EndPoint,
                    src => new IPEndPoint(IPAddress.Parse(Config.Instance.IP), Config.Instance.Listener.Port))
                .Member(dest => dest.ChatEndPoint,
                    src => new IPEndPoint(IPAddress.Parse(Config.Instance.IP), Config.Instance.ChatListener.Port));

            Mapper.Register<Player, PlayerAccountInfoDto>()
                .Function(dest => dest.IsGM, src => src.Account.SecurityLevel > SecurityLevel.Tester)
                .Member(dest => dest.GameTime, src => TimeSpan.Parse(src.PlayTime))
                .Member(dest => dest.TotalExp, src => src.TotalExperience)
                .Function(dest => dest.TutorialState,
                    src => (uint) (Config.Instance.Game.EnableTutorial ? src.TutorialState : 1))
                .Member(dest => dest.Nickname, src => src.Account.Nickname)
                .Member(dest => dest.TotalMatches, src => src.TotalLosses + src.TotalWins)
                .Member(dest => dest.MatchesWon, src => src.TotalWins)
                .Member(dest => dest.MatchesLost, src => src.TotalLosses);


            Mapper.Register<Channel, ChannelInfoDto>()
                .Member(dest => dest.PlayersOnline, src => src.Players.Count);

            Mapper.Register<PlayerItem, ItemDto>()
                .Member(dest => dest.Id, src => src.Id)
                .Function(dest => dest.ExpireTime,
                    src => src.ExpireDate == DateTimeOffset.MinValue ? -1 : src.ExpireDate.ToUnixTimeSeconds())
                .Function(dest => dest.Durability, src =>
                {
                    if (src.PeriodType == ItemPeriodType.Units) return (int) src.Count;
                    return src.Durability;
                })
                .Function(dest => dest.Effects, src =>
                {
                    var desteffects = new List<ItemEffectDto>();
                    src.Effects.ToList().ForEach(eff => { desteffects.Add(new ItemEffectDto {Effect = eff}); });
                    return desteffects.ToArray();
                });

            Mapper.Register<Deny, DenyDto>()
                .Member(dest => dest.AccountId, src => src.DenyId)
                .Member(dest => dest.Nickname, src => src.Nickname);

            Mapper.Register<Player, RoomPlayerDto>()
                .Member(dest => dest.ClanId, src => src.Club.Id)
                .Function(dest => dest.AccountId, src => src.Account?.Id ?? 0)
                .Function(dest => dest.Nickname, src => src.Account?.Nickname ?? "n/A")
                .Member(dest => dest.Unk2, src => (byte) src.Room.Players.Values.ToList().IndexOf(src))
                .Function(dest => dest.IsGM, src => src.Account?.SecurityLevel > SecurityLevel.Tester);

            Mapper.Register<PlayerItem, Data.P2P.ItemDto>()
                .Function(dest => dest.ItemNumber, src => src?.ItemNumber ?? 0);

            Mapper.Register<RoomCreationOptions, ChangeRuleDto>()
                .Function(dest => dest.GameRule, src => (int) src.GameRule)
                .Member(dest => dest.Map_ID, src => (byte) src.MapId)
                .Member(dest => dest.Player_Limit, src => src.PlayerLimit)
                .Member(dest => dest.Points, src => src.ScoreLimit)
                .Member(dest => dest.Time, src => (byte) src.TimeLimit.TotalMinutes)
                .Member(dest => dest.Weapon_Limit, src => src.ItemLimit)
                .Member(dest => dest.Password, src => src.Password)
                .Member(dest => dest.Name, src => src.Name)
                .Member(dest => dest.HasSpectator, src => src.HasSpectator)
                .Member(dest => dest.SpectatorLimit, src => src.SpectatorLimit);

            Mapper.Register<RoomCreationOptions, ChangeRuleDto2>()
                .Function(dest => dest.GameRule, src => (int) src.GameRule)
                .Member(dest => dest.Map_ID, src => (byte) src.MapId)
                .Member(dest => dest.Player_Limit, src => src.PlayerLimit)
                .Member(dest => dest.Points, src => src.ScoreLimit)
                .Value(dest => dest.Unk1, 0)
                .Member(dest => dest.Time, src => (byte) src.TimeLimit.TotalMinutes)
                .Member(dest => dest.Weapon_Limit, src => src.ItemLimit)
                .Member(dest => dest.Password, src => src.Password)
                .Member(dest => dest.Name, src => src.Name)
                .Member(dest => dest.HasSpectator, src => src.HasSpectator)
                .Member(dest => dest.SpectatorLimit, src => src.SpectatorLimit)
                .Member(dest => dest.FMBurnMode, src => src.GetFMBurnModeInfo());

            Mapper.Register<Mail, NoteDto>()
                .Function(dest => dest.ReadCount, src => src.IsNew ? 0 : 1)
                .Function(dest => dest.DaysLeft,
                    src => DateTimeOffset.Now < src.Expires ? (src.Expires - DateTimeOffset.Now).TotalDays : 0);

            Mapper.Register<Mail, NoteContentDto>()
                .Member(dest => dest.Id, src => src.Id)
                .Member(dest => dest.Message, src => src.Message);

            Mapper.Register<PlayerItem, ItemDurabilityInfoDto>()
                .Member(dest => dest.ItemId, src => src.Id)
                .Function(dest => dest.Durabilityloss, src =>
                {
                    var loss = src.DurabilityLoss;
                    src.DurabilityLoss = 0;
                    return loss;
                });

            Mapper.Register<Player, PlayerInfoShortDto>()
                .Function(dest => dest.AccountId, src => src.Account?.Id ?? 0)
                .Function(dest => dest.Nickname, src => src.Account?.Nickname ?? "n/A")
                .Function(dest => dest.IsGM, src => src.Account?.SecurityLevel > SecurityLevel.Tester)
                .Member(dest => dest.TotalExp, src => src.TotalExperience);

            Mapper.Register<Player, PlayerLocationDto>()
                .Function(dest => dest.ServerGroupId, src => (int) Config.Instance.Id)
                .Function(dest => dest.ChannelId, src => src.Channel?.Id > 0 ? (int) src.Channel?.Id : -1)
                .Function(dest => dest.RoomId, src => src.Room?.Id > 0 ? (int) src.Room?.Id : -1)
                .Function(dest => dest.GameServerId, src => Config.Instance.Id) // TODO Server ids
                .Function(dest => dest.ChatServerId, src => Config.Instance.Id);

            Mapper.Register<Player, PlayerInfoDto>()
                .Function(dest => dest.Info, src => src.Map<Player, PlayerInfoShortDto>())
                .Function(dest => dest.Location, src => src.Map<Player, PlayerLocationDto>());

            Mapper.Register<Player, UserDataDto>()
                .Member(dest => dest.TotalExp, src => src.TotalExperience)
                .Function(dest => dest.AccountId, src => src.Account?.Id ?? 0)
                .Function(dest => dest.Nickname, src => src.Account?.Nickname ?? "n/A")
                .Member(dest => dest.PlayTime, src => TimeSpan.Parse(src.PlayTime))
                .Member(dest => dest.TotalGames, src => src.TotalMatches)
                .Member(dest => dest.GamesWon, src => src.TotalWins)
                .Member(dest => dest.GamesLost, src => src.TotalLosses)
                .Member(dest => dest.Level, src => src.Level);

            Mapper.Register<Player, PlayerNameTagInfoDto>()
                .Member(dest => dest.AccountId, src => src.Account.Id);

            Mapper.Register<Player, ClubMyInfoDto>()
                .Function(dest => dest.Id, src => src.Club?.Id ?? 0)
                .Function(dest => dest.Name, src => src.Club?.ClanName ?? "")
                .Function(dest => dest.Level, src => src.Club?.Count ?? 0)
                .Function(dest => dest.Type, src => src.Club?.ClanIcon ?? "")
                .Function(dest => dest.State, src => src.Club?[src.Account?.Id ?? 0].State ?? 0);

            Mapper.Register<Player, PlayerClubInfoDto>()
                .Function(dest => dest.Id, src => src.Club?.Id ?? 0)
                .Function(dest => dest.Name, src => src.Club?.ClanName ?? "")
                .Function(dest => dest.Type, src => src.Club?.ClanIcon ?? "");

            Mapper.Register<Player, ClubMemberDto>()
                .Function(dest => dest.AccountId, src => src.Account?.Id ?? 0)
                .Function(dest => dest.Nickname, src => src.Account?.Nickname ?? "n/A")
                .Function(dest => dest.ServerId, src => (int) Config.Instance.Id)
                .Function(dest => dest.ChannelId, src => src.Channel?.Id > 0 ? src.Channel.Id : -1)
                .Function(dest => dest.RoomId, src => src.Room?.Id > 0 ? (int) src.Room.Id : -1)
                .Function(dest => dest.ClanRank, src => (int)(src.Club?.GetPlayer(src.Account.Id)?.Rank ?? 0))
                .Function(dest => dest.LastLogin, src => src.Club?.GetPlayer(src.Account.Id)?.Account.LastLogin ?? "");
            
            Mapper.Register<ClubPlayerInfo, ClubMemberDto>()
                .Function(dest => dest.AccountId, src => src.AccountId)
                .Function(dest => dest.Nickname, src => src.Account?.Nickname ?? "n/A")
                .Function(dest => dest.LastLogin, src => src.Account.LastLogin ?? "")
                .Function(dest => dest.ClanRank, src => (int)src.Rank)
                .Value(dest => dest.ServerId, -1)
                .Value(dest => dest.ChannelId, -1)
                .Value(dest => dest.RoomId, -1);

            Mapper.Register<Player, ClubInfoDto>()
                .Function(dest => dest.Id, src => src.Club?.Id ?? 0)
                .Function(dest => dest.Name, src => src.Club?.ClanName ?? "n/A")
                .Function(dest => dest.MasterName,
                    src => src.Club?.Players.Values.FirstOrDefault(x => x.Rank == ClubRank.Master)?.Account?.Nickname ?? "")
                .Function(dest => dest.MemberCount, src => src.Club?.Count + 5 ?? 0)
                .Function(dest => dest.Type, src => src.Club?.ClanIcon ?? "");

            Mapper.Register<Player, ClubInfoDto2>()
                .Function(dest => dest.Id, src => src.Club?.Id ?? 0)
                .Function(dest => dest.Id2, src => src.Club?.Id ?? 0)
                .Function(dest => dest.Name, src => src.Club?.ClanName ?? "n/A")
                .Function(dest => dest.MasterName,
                    src => src.Club?.Players.Values.FirstOrDefault(x => x.Rank == ClubRank.Master)?.Account?.Nickname ?? "")
                .Function(dest => dest.MemberCount, src => src.Club?.Count + 5 ?? 0)
                .Function(dest => dest.Type, src => src.Club?.ClanIcon ?? "");


            Mapper.Register<ClubPlayerInfo, PlayerInfoDto>()
                .Function(dest => dest.Info, src =>
                {
                    var plr = Instance.PlayerManager.FirstOrDefault(x => x.Account?.Id == src.AccountId);
                    return plr.Map<Player, PlayerInfoShortDto>();
                })
                .Function(dest => dest.Location, src =>
                {
                    var plr = Instance.PlayerManager.FirstOrDefault(x => x.Account?.Id == src.AccountId);
                    return plr.Map<Player, PlayerLocationDto>();
                });

            Mapper.Compile(CompilationTypes.Source);
        }

        #region Events

        protected override void OnStarted()
        {
            ResourceCache.PreCache();
            _worker.Start();
            _serverlistManager.Start();
        }

        protected override void OnStopping()
        {
            _worker.Stop(new TimeSpan(0));
            _serverlistManager.Dispose();
        }

        protected override void OnDisconnected(ProudSession session)
        {
            try
            {
                var gameSession = (GameSession) session;
                if (gameSession.Player != null)
                {
                    gameSession.Player.Room?.Leave(gameSession.Player);
                    gameSession.Player.Channel?.Leave(gameSession.Player);
                    gameSession.Player.Save();

                    PlayerManager.Remove(gameSession.Player);

                    Logger.ForAccount(gameSession)
                        .Debug($"Client {session.RemoteEndPoint} disconnected");

                    if (gameSession.Player.ChatSession != null)
                    {
                        Club.LogOff(gameSession.Player);
                        gameSession.Player.ChatSession.GameSession = null;
                        gameSession.Player.ChatSession.Dispose();
                    }

                    if (gameSession.Player.RelaySession != null)
                    {
                        gameSession.Player.RelaySession.GameSession = null;
                        gameSession.Player.RelaySession.Dispose();
                    }

                    gameSession.Player.Session = null;
                    gameSession.Player.ChatSession = null;
                    gameSession.Player.RelaySession = null;
                    gameSession.Player = null;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }

            base.OnDisconnected(session);
        }

        protected override void OnError(ErrorEventArgs e)
        {
            var gameSession = (GameSession) e.Session;
            var log = Logger;
            if (e.Session != null)
                log = log.ForAccount((GameSession) e.Session);

            if (e.Exception.ToString().ToLower().Contains("opcode") ||
                e.Exception.ToString().ToLower().Contains("bad format in"))
            {
                log.Warning(e.Exception.InnerException.Message);
                gameSession?.SendAsync(new ServerResultAckMessage(ServerResult.ServerError));
            }
            else if (gameSession.Player != null && (gameSession.Player.Room != null &&
                                                    gameSession.Player.Room.GameRuleManager.GameRule.StateMachine
                                                        .State == GameRuleState.Waiting
                                                    || gameSession.Player.Room == null))
            {
                log.Warning(e.Exception.ToString());
                gameSession?.SendAsync(new ServerResultAckMessage(ServerResult.ServerError));
            }
            else
            {
                log.Error(e.Exception, "Unhandled server error");
            }

            base.OnError(e);
        }

        //private void OnUnhandledMessage(object sender, MessageReceivedEventArgs e)
        //{
        //    var session = (GameSession)e.Session;
        //    Log.Warning()
        //        .Account(session)
        //        .Message($"Unhandled message {e.Message.GetType().Name}")
        //        .Write();
        //}

        #endregion
    }
}
