using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using BlubLib.Collections.Concurrent;
using BlubLib.Threading.Tasks;
using ExpressMapper.Extensions;
using NeoNetsphere.Network;
using NeoNetsphere.Network.Data.Game;
using NeoNetsphere.Network.Data.GameRule;
using NeoNetsphere.Network.Message.Chat;
using NeoNetsphere.Network.Message.Game;
using NeoNetsphere.Network.Message.GameRule;
using Netsphere;
using Netsphere.Game.Systems;
using ProudNetSrc;
using Serilog;
using Serilog.Core;

namespace NeoNetsphere
{
    internal class Room
    {
        private const uint PingDifferenceForChange = 20;

        // ReSharper disable once InconsistentNaming
        private static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(Room));

        private readonly TimeSpan _changingRulesTime = TimeSpan.FromSeconds(2);
        private readonly TimeSpan _hostUpdateTime = TimeSpan.FromSeconds(30);
        private readonly ConcurrentDictionary<ulong, object> _kickedPlayers = new ConcurrentDictionary<ulong, object>();

        private readonly ConcurrentDictionary<ulong, Player> _players = new ConcurrentDictionary<ulong, Player>();
        private readonly AsyncLock _slotIdSync = new AsyncLock();
        private TimeSpan _changingRulesTimer;

        private TimeSpan _hostUpdateTimer;

        public Room(RoomManager roomManager, uint id, RoomCreationOptions options, P2PGroup group, Player creator)
        {
            RoomManager = roomManager;
            Id = id;
            Options = options;
            TimeCreated = DateTime.Now;
            TeamManager = new TeamManager(this);
            GameRuleManager = new GameRuleManager(this);
            Group = group;
            Creator = creator;
            Master = creator;
            TeamManager.TeamChanged += TeamManager_TeamChanged;

            GameRuleManager.GameRuleChanged += GameRuleManager_OnGameRuleChanged;
            GameRuleManager.MapInfo = GameServer.Instance.ResourceCache.GetMaps()[options.MapID];
            GameRuleManager.GameRule = RoomManager.GameRuleFactory.Get(Options.GameRule, this);
        }

        public RoomManager RoomManager { get; }
        public uint Id { get; }
        public RoomCreationOptions Options { get; }
        public DateTime TimeCreated { get; }

        public TeamManager TeamManager { get; }
        public GameRuleManager GameRuleManager { get; }
        public bool hasStarted { get; set; } = false;
        public bool isPreparing { get; set; } = false;
        public GameState GameState { get; set; } = GameState.Waiting;
        public GameTimeState SubGameState { get; set; } = GameTimeState.None;
        public TimeSpan RoundTime { get; set; } = TimeSpan.Zero;

        public IReadOnlyDictionary<ulong, Player> Players => _players;

        public Player Master { get; private set; }
        public Player Host { get; private set; }
        public Player Creator { get; }

        public P2PGroup Group { get; }

        public bool IsChangingRules { get; private set; } = false;
        private bool IsChangingRulesCooldown { get; set; } = false;

        public void Update(TimeSpan delta)
        {
            try
            {
                if (!(RoomManager?.Contains(this) ?? false))
                    return;

                if (Players.Count == 0 || !TeamManager.Players.Any())
                {
                    if (Master.Room == this && !Master.IsLoggedIn()) RoomManager?.Remove(this);
                    return;
                }

                if (!(Master?.IsLoggedIn() ?? true) || Master.Room != this)
                {
                    ChangeMasterIfNeeded(GetPlayerWithLowestPing(), true);
                    ChangeHostIfNeeded(GetPlayerWithLowestPing(), true);
                }

                //if (Host != null)
                //{
                //    _hostUpdateTimer += delta;
                //    if (_hostUpdateTimer >= _hostUpdateTime)
                //    {
                //        var lowest = GetPlayerWithLowestPing();
                //        if (Host != lowest)
                //        {
                //            var diff = Math.Abs(Host.Session.UnreliablePing - lowest.Session.UnreliablePing);
                //            if (diff >= PingDifferenceForChange)
                //                ChangeHostIfNeeded(lowest, true);
                //        }
                //
                //        _hostUpdateTimer = TimeSpan.Zero;
                //    }
                //}

                if (IsChangingRules)
                {
                    _changingRulesTimer += delta;
                    if (_changingRulesTimer >= _changingRulesTime && IsChangingRulesCooldown != true)
                    {
                        RoomManager.Channel.BroadcastCencored(new RoomChangeRoomInfoAck2Message(GetRoomInfo()));
                        Broadcast(new RoomChangeRuleAckMessage(Options.Map<RoomCreationOptions, ChangeRuleDto>()));
                        Broadcast(new GameChangeStateAckMessage(GameState));
                        IsChangingRulesCooldown = true;
                    }

                    if (_changingRulesTimer >= _changingRulesTime.Add(TimeSpan.FromSeconds(3)))
                    {
                        IsChangingRules = false;
                        IsChangingRulesCooldown = false;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }

            GameRuleManager.Update(delta);
        }

        public void Join(Player plr)
        {
            if (plr.Room != null)
                throw new RoomException("Player is already inside a room");

            if(plr.Room.Options.IsNoIntrusion && plr.Room.GameState != GameState.Waiting)
                throw new RoomLimitIsNoIntrutionException();

            if (Math.Max((int)Options.PlayerLimit, 1) < TeamManager.Players.Count())
                throw new RoomLimitReachedException();

            if (_kickedPlayers.ContainsKey(plr.Account.Id))
                throw new RoomAccessDeniedException();

            using (_slotIdSync.Lock())
            {
                byte id = 3;
                while (Players.Values.Any(p => p.RoomInfo.Slot == id))
                    id++;

                plr.RoomInfo.Slot = id;
            }

            if (plr.Channel != null)
            {
                plr.LocationInfo = new PlayerLocationInfo(plr.Channel.Id) {Invisible = true};
                plr.Channel.Broadcast(new ChannelLeavePlayerAckMessage(plr.Account.Id));
            }

            plr.RoomInfo.Reset();
            plr.RoomInfo.State = PlayerState.Lobby;
            plr.RoomInfo.Mode = PlayerGameMode.Normal;
            plr.RoomInfo.Stats = GameRuleManager.GameRule.GetPlayerRecord(plr);
            TeamManager.Join(plr);

            _players.TryAdd(plr.Account.Id, plr);
            plr.Room = this;
            plr.RoomInfo.IsConnecting = true;

            if (Options.GameRule != GameRule.Horde)
            {
                plr.Session.SendAsync(new RoomEnterRoomInfoAck2Message
                {
                    RoomID = Id,
                    GameRule = Options.GameRule,
                    MapID = (byte)Options.MapID,
                    PlayerLimit = Options.PlayerLimit,
                    GameTimeState = SubGameState,
                    GameState = (uint)GameState,
                    TimeLimit = (uint)Options.TimeLimit.TotalMilliseconds,
                    mUnknow01 = 0,
                    Time_Sync = (uint)GameRuleManager.GameRule.RoundTime.TotalMilliseconds,
                    Score_Limit = Options.ScoreLimit,
                    mUnknow02 = 0,
                    IP = new IPEndPoint(IPAddress.Parse(Config.Instance.IP), Config.Instance.RelayListener.Port),
                    Spectator = Options.hasSpectator ? (byte)1 : (byte)0,
                    mUnknow03 = (uint)Options.Spectator,
                    FMBURNMode = GetFMBurnModeInfo()
                });
            }
            else
            {
                plr.Session.SendAsync(new RoomEnterRoomInfoAckMessage
                {
                    RoomID = Id,
                    GameRule = Options.GameRule,
                    MapID = (byte)Options.MapID,
                    PlayerLimit = Options.PlayerLimit,
                    GameTimeState = SubGameState,
                    GameState = (uint)GameState,
                    TimeLimit = (uint)Options.TimeLimit.TotalMilliseconds,
                    mUnknow01 = 1,
                    Time_Sync = (uint)GameRuleManager.GameRule.RoundTime.TotalMilliseconds,
                    Score_Limit = Options.ScoreLimit,
                    mUnknow02 = 1,
                    IP = new IPEndPoint(IPAddress.Parse(Config.Instance.IP), Config.Instance.RelayListener.Port),
                    Spectator = Options.hasSpectator ? (byte)1 : (byte)0,
                    mUnknow03 = (uint)Options.Spectator,
                });
            }

            plr.Session.SendAsync(new RoomCurrentCharacterSlotAckMessage(1, plr.RoomInfo.Slot));
            BroadcastExcept(plr, new RoomEnterPlayerInfoAckMessage(plr.Map<Player, RoomPlayerDto>()));
            plr.Session.SendAsync(new RoomPlayerInfoListForEnterPlayerAckMessage(_players.Values.Select(r => r.Map<Player, RoomPlayerDto>()).ToArray()));
            plr.Session.SendAsync(new RoomPlayerInfoListForEnterPlayerForCollectBookAckMessage());

            var clubList = new List<PlayerClubInfoDto>();
            foreach (var player in _players.Values.Where(p => p.Club != null))
            {
                if (clubList.All(club => club.Id != player.Club.Clan_ID))
                {
                    clubList.Add(new PlayerClubInfoDto()
                    {
                        Id = player.Club.Clan_ID,
                        Name = player.Club.Clan_Name,
                        Type = player.Club.Clan_Icon,
                    });
                }
            }
            if (plr.Club != null)
            {
                BroadcastExcept(plr, new RoomEnterClubInfoAckMessage(new PlayerClubInfoDto()
                {
                    Id = plr.Club.Clan_ID,
                    Name = plr.Club.Clan_Name,
                    Type = plr.Club.Clan_Icon,
                }));
            }
            plr.Session.SendAsync(new RoomClubInfoListForEnterPlayerAckMessage(clubList.ToArray()));
            plr.Session.SendAsync(new ItemClearInvalidEquipItemAckMessage { Items = new InvalidateItemInfoDto[] { } });
            plr.Session.SendAsync(new ItemClearEsperChipAckMessage { Unk = new ClearEsperChipDto[] { } });
            
            OnPlayerJoining(new RoomPlayerEventArgs(plr));
        }

        public void Leave(Player plr, RoomLeaveReason roomLeaveReason = RoomLeaveReason.Left)
        {
            if (plr.Room != this)
                return;
            try
            {
                if (plr.RelaySession?.HostId != null)
                    Group?.Leave(plr.RelaySession.HostId);

                Broadcast(new RoomLeavePlayerAckMessage(plr.Account.Id, plr.Account.Nickname, roomLeaveReason));

                if (roomLeaveReason == RoomLeaveReason.Kicked ||
                    roomLeaveReason == RoomLeaveReason.ModeratorKick ||
                    roomLeaveReason == RoomLeaveReason.VoteKick)
                    _kickedPlayers.TryAdd(plr.Account.Id, null);
                
                plr.LocationInfo.Invisible = false;
                var curchannelid = (uint)plr.Channel.Id;
                plr.Channel.Leave(plr, true);
                GameServer.Instance.ChannelManager[curchannelid].Join(plr);

                plr.RoomInfo.PeerId = 0;
                plr.RoomInfo.Team.Leave(plr);
                _players?.Remove(plr.Account.Id);
                plr.Room = null;
                plr.Session?.SendAsync(new RoomLeavePlayerInfoAckMessage(plr.Account.Id));
                plr.Session?.SendAsync(new ItemClearInvalidEquipItemAckMessage { Items = new InvalidateItemInfoDto[] { } });
                plr.Session?.SendAsync(new ItemClearEsperChipAckMessage { Unk = new ClearEsperChipDto[] { } });

                if (_players != null && _players.Count > 0)
                {
                    ChangeMasterIfNeeded(GetPlayerWithLowestPing());
                    ChangeHostIfNeeded(GetPlayerWithLowestPing());
                    OnPlayerLeft(new RoomPlayerEventArgs(plr));
                }
                else
                {
                    RoomManager.Remove(this);
                }
            }
            catch (Exception ex)
            {
                Broadcast(new RoomLeavePlayerAckMessage(plr.Account.Id, plr.Account.Nickname, roomLeaveReason));
                Broadcast(new RoomLeavePlayerInfoAckMessage(plr.Account.Id));
                
                _players?.Remove(plr.Account.Id);
                Logger.Error(ex.ToString());
            }
        }
        
        public void SetCreator(Player plr)
        {
            Master = plr;
            Host = plr;
        }

        public bool ChangeMasterIfNeeded(Player plr, bool force = false)
        {
            if (plr.Room == this && plr.Room.TeamManager.Players.Count() != 1 && plr.Room.Master != null &&
                (!force || Master == plr))
                return false;

            Master = plr;
            Broadcast(new RoomChangeMasterAckMessage(Master.Account.Id));
            return true;
        }

        public bool ChangeHostIfNeeded(Player plr, bool force = false)
        {
            if (plr.Room == this && Host != null && plr.Room.TeamManager.Players.Count() != 1 && (!force || Host == plr))
                return false;

            // TODO Add Room extension?
            Logger.Debug("<Room {roomId}> Changing host to {nickname} - Ping:{ping} ms", Id, plr.Account.Nickname,
                plr.Session.UnreliablePing);
            Host = plr;
            Broadcast(new RoomChangeRefereeAckMessage(Host.Account.Id));
            return true;
        }

        public void ChangeRules(ChangeRuleDto options)
        {
            ChangeRules2(options.Map<ChangeRuleDto, ChangeRuleDto2>());
        }

        public void ChangeRules2(ChangeRuleDto2 options)
        {
            if (IsChangingRules)
            {
                Master?.Session?.SendAsync(new ServerResultAckMessage(ServerResult.RoomChangingRules));
                return;
            }

            if (!RoomManager.GameRuleFactory.Contains((GameRule) options.GameRule))
            {
                Logger.ForAccount(Master)
                    .Error("Game rule {gameRule} does not exist", options.GameRule);
                //throw new Exception("gamerule is not available");
                Master?.Session?.SendAsync(new ServerResultAckMessage(ServerResult.FailedToRequestTask));
                return;
            }

            // ToDo check if current player count is not above the new player limit
            var israndom = false;
            if ((GameRule) options.GameRule != GameRule.Practice &&
                (GameRule) options.GameRule != GameRule.CombatTrainingTD &&
                (GameRule) options.GameRule != GameRule.CombatTrainingDM)
            {
                if (options.Map_ID == 228 && (GameRule) options.GameRule == GameRule.BattleRoyal) //random br
                {
                    israndom = true;
                    options.Map_ID = 112;
                }
                else if (options.Map_ID == 229 && (GameRule) options.GameRule == GameRule.Chaser) //random chaser
                {
                    israndom = true;
                    options.Map_ID = 225;
                }
                else if (options.Map_ID == 231 && (GameRule) options.GameRule == GameRule.Deathmatch) //random deathmatch
                {
                    israndom = true;
                    options.Map_ID = 20;
                }
                else if (options.Map_ID == 230 && (GameRule) options.GameRule == GameRule.Touchdown) //random touchdown
                {
                    israndom = true;
                    options.Map_ID = 66;
                }

                var map = GameServer.Instance.ResourceCache.GetMaps().GetValueOrDefault(options.Map_ID);
                if (map == null)
                {
                    Logger.ForAccount(Master)
                        .Error($"Map {options.Map_ID} does not exist");
                    Master?.Session?.SendAsync(new ServerResultAckMessage(ServerResult.FailedToRequestTask));
                    return;
                }
                if (!map.GameRules.Contains((GameRule) options.GameRule))
                {
                    Logger.ForAccount(Master)
                        .Error($"Map {map.Id}({map.Name}) is not available for game rule {(GameRule)options.GameRule}");
                    Master?.Session?.SendAsync(new ServerResultAckMessage(ServerResult.FailedToRequestTask));
                    return;
                }
            }

            if (options.Player_Limit < 1)
                options.Player_Limit = 1;

            if ((GameRule) options.GameRule == GameRule.CombatTrainingTD ||
                (GameRule) options.GameRule == GameRule.CombatTrainingDM)
                options.Player_Limit = 12;

            var matchkey = new MatchKey();

            if ((GameRule) options.GameRule == GameRule.CombatTrainingDM ||
                (GameRule) options.GameRule == GameRule.CombatTrainingTD ||
                (GameRule) options.GameRule == GameRule.Practice)
                options.FMBurnMode = 1;

            var isfriendly = false;
            var isbalanced = true;
            var isburning = false;
            var isWithoutStats = false;
            switch (options.FMBurnMode)
            {
                case 0:
                    isbalanced = true;
                    isfriendly = false;
                    break;
                case 1:
                    isbalanced = isfriendly = true;
                    break;
                case 2:
                    isbalanced = true;
                    isfriendly = false;
                    isburning = true;
                    break;
                case 3:
                    isburning = true;
                    isbalanced = isfriendly = true;
                    break;
                case 4:
                    isWithoutStats = true;
                    break;
                case 5:
                    isWithoutStats = isfriendly = true;
                    break;
            }
            _changingRulesTimer = TimeSpan.Zero;
            IsChangingRules = true;
            Options.Name = options.Name;
            Options.MapID = options.Map_ID;
            Options.PlayerLimit = options.Player_Limit;
            Options.GameRule = (GameRule) options.GameRule;
            Options.TimeLimit = TimeSpan.FromMinutes(options.Time);
            Options.ScoreLimit = (ushort) options.Points;
            Options.Password = options.Password;
            Options.IsFriendly = isfriendly;
            Options.IsBalanced = isbalanced;
            Options.IsBurning = isburning;
            Options.IsRandom = israndom;
            Options.ItemLimit = (byte) options.Weapon_Limit;
            Options.hasSpectator = options.HasSpectator;
            Options.Spectator = options.SpectatorLimit;
            Options.IsWithoutStats = isWithoutStats;
            _players.Values.ToList().ForEach(playr => { playr.RoomInfo.IsReady = false; });

            GameRuleManager.MapInfo = GameServer.Instance.ResourceCache.GetMaps()[Options.MapID];
            GameRuleManager.GameRule = RoomManager.GameRuleFactory.Get(Options.GameRule, this);
            BroadcastExcept(Master, new RoomChangeRuleNotifyAck2Message(Options.Map<RoomCreationOptions, ChangeRuleDto2>()));
        }

        private Player GetPlayerWithLowestPing()
        {
            return _players.Values.Aggregate((lowestPlayer, player) =>
                lowestPlayer == null || player.Session.UnreliablePing < lowestPlayer.Session.UnreliablePing
                    ? player
                    : lowestPlayer);
        }

        private void TeamManager_TeamChanged(object sender, TeamChangedEventArgs e)
        {
            //RoomManager.Channel.Broadcast(new SUserDataAckMessage(e.Player.Map<Player, UserDataDto>()));
        }

        private void GameRuleManager_OnGameRuleChanged(object sender, EventArgs e)
        {
            GameRuleManager.GameRule.StateMachine.OnTransitioned(t => OnStateChanged());

            foreach (var plr in Players.Values)
            {
                plr.RoomInfo.Stats = GameRuleManager.GameRule.GetPlayerRecord(plr);
                TeamManager.Join(plr);
            }
            BroadcastBriefing();
        }

        #region Events

        public event EventHandler<RoomPlayerEventArgs> PlayerJoining;
        public event EventHandler<RoomPlayerEventArgs> PlayerJoined;
        public event EventHandler<RoomPlayerEventArgs> PlayerLeft;
        public event EventHandler StateChanged;

        internal virtual byte GetFMBurnModeInfo()
        {
            byte FMBurnMode = 0;
            if (Options.IsFriendly && Options.IsWithoutStats)
                FMBurnMode = 5;
            else if (Options.IsWithoutStats)
                FMBurnMode = 4;
            else if (Options.IsFriendly && Options.IsBurning)
                FMBurnMode = 3;
            else if (Options.IsBurning)
                FMBurnMode = 2;
            else if (Options.IsFriendly)
                FMBurnMode = 1;
            else if (!Options.IsFriendly && !Options.IsBurning)
                FMBurnMode = 0;
            return FMBurnMode;
        }

        internal virtual RoomDto GetRoomInfo()
        {
            var roomDto = new RoomDto
            {
                RoomId = (byte) Id,
                PlayerCount = (byte) Players.Count,
                PlayerLimit = Options.PlayerLimit,
                State = (byte) GameRuleManager.GameRule.StateMachine.State,
                GameRule = (int) Options.GameRule,
                Map = (byte) Options.MapID,
                WeaponLimit = Options.ItemLimit,
                Name = Options.Name,
                Password = Options.Password,
                FMBURNMode = GetFMBurnModeInfo(),
                SpectatorEnabled = Options.hasSpectator,
                IsRandom = Options.IsRandom ? 1 : 0
            };

            //roomDto.State2 = (byte) GameRuleManager.GameRule.StateMachine.State;
            //roomDto.Unk1 = (byte)GameRuleManager.GameRule.StateMachine.State;
            //roomDto.Unk1 = Options.UniqueID;
            //roomDto.Spectator = Options.Spectator;
            return roomDto;
        }

        internal virtual void OnPlayerJoining(RoomPlayerEventArgs e)
        {
            PlayerJoining?.Invoke(this, e);
            RoomManager.Channel.BroadcastCencored(new RoomChangeRoomInfoAck2Message(GetRoomInfo()));
        }

        internal virtual void OnPlayerJoined(RoomPlayerEventArgs e)
        {
            PlayerJoined?.Invoke(this, e);
            RoomManager.Channel.BroadcastCencored(new RoomChangeRoomInfoAck2Message(GetRoomInfo()));
        }

        protected virtual void OnPlayerLeft(RoomPlayerEventArgs e)
        {
            PlayerLeft?.Invoke(this, e);
            RoomManager.Channel.BroadcastCencored(new RoomChangeRoomInfoAck2Message(GetRoomInfo()));
        }

        protected virtual void OnStateChanged()
        {
            StateChanged?.Invoke(this, EventArgs.Empty);
            RoomManager.Channel.BroadcastCencored(new RoomChangeRoomInfoAck2Message(GetRoomInfo()));
        }

        #endregion

        #region Broadcast

        public void Broadcast(IGameMessage message)
        {
            foreach (var plr in _players.Values)
                plr.Session.SendAsync(message);
        }

        public void Broadcast(IGameRuleMessage message)
        {
            foreach (var plr in _players.Values)
                plr.Session.SendAsync(message);
        }

        public void BroadcastExcept(Player blacklisted, IGameRuleMessage message)
        {
            foreach (var plr in _players.Values.Where(x => x != blacklisted))
                plr.Session.SendAsync(message);
        }

        public void BroadcastExcept(Player blacklisted, IGameMessage message)
        {
            foreach (var plr in _players.Values.Where(x => x != blacklisted))
                plr.Session.SendAsync(message);
        }
        public void BroadcastExcept(List<Player> blacklist, IGameMessage message)
        {
            foreach (var plr in _players.Values.Where(x => !blacklist.Contains(x)))
                plr.Session.SendAsync(message);
        }
        public void BroadcastExcept(List<Player> blacklist, IGameRuleMessage message)
        {
            foreach (var plr in _players.Values.Where(x => !blacklist.Contains(x)))
                plr.Session.SendAsync(message);
        }

        public void Broadcast(IChatMessage message)
        {
            foreach (var plr in _players.Values)
                plr.ChatSession.SendAsync(message);
        }

        public void BroadcastBriefing(Player plr, bool isResult = false)
        {
            var gameRule = GameRuleManager.GameRule;
            plr.Session.SendAsync(new GameBriefingInfoAckMessage(isResult, false, gameRule.Briefing.ToArray(isResult)));
        }
        public void BroadcastBriefing(bool isResult = false)
        {
            var gameRule = GameRuleManager.GameRule;
            Broadcast(new GameBriefingInfoAckMessage(isResult, false, gameRule.Briefing.ToArray(isResult)));
        }

        #endregion
    }
}
