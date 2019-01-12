using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NeoNetsphere.Network.Data.GameRule;
using NeoNetsphere.Network.Message.GameRule;
using Netsphere;
using Netsphere.Game;
using Netsphere.Game.GameRules;
using Netsphere.Game.Systems;

namespace NeoNetsphere.Game.GameRules
{
    internal class CaptainGameRule : GameRuleBase
    {
        private static readonly TimeSpan s_captainNextroundTime = TimeSpan.FromSeconds(12);
        private static readonly TimeSpan s_captainRoundTime = TimeSpan.FromMinutes(5);
        private readonly CaptainHelper _captainHelper;
        private uint _currentRound;
        private TimeSpan _nextRoundTime = TimeSpan.Zero;
        private TimeSpan _subRoundTime = TimeSpan.Zero;
        private bool _waitingNextRoom;

        public CaptainGameRule(Room room)
            : base(room)
        {
            Briefing = new CaptainBriefing(this);
            _captainHelper = new CaptainHelper(room);

            StateMachine.Configure(GameRuleState.Waiting)
                .PermitIf(GameRuleStateTrigger.StartPrepare, GameRuleState.Preparing, CanStartGame);

            StateMachine.Configure(GameRuleState.Preparing)
                .Permit(GameRuleStateTrigger.StartGame, GameRuleState.FullGame);

            StateMachine.Configure(GameRuleState.FullGame)
                .SubstateOf(GameRuleState.Playing)
                .Permit(GameRuleStateTrigger.StartResult, GameRuleState.EnteringResult)
                .OnEntry(_captainHelper.Reset);

            StateMachine.Configure(GameRuleState.EnteringResult)
                .SubstateOf(GameRuleState.Playing)
                .Permit(GameRuleStateTrigger.StartResult, GameRuleState.Result);

            StateMachine.Configure(GameRuleState.Result)
                .SubstateOf(GameRuleState.Playing)
                .Permit(GameRuleStateTrigger.EndGame, GameRuleState.Waiting)
                .OnEntry(UpdatePlayerStats);
        }

        public override GameRule GameRule => GameRule.Captain;
        public override Briefing Briefing { get; }
        public override bool CountMatch => true;

        public override void Initialize()
        {
            var teamMgr = Room.TeamManager;
            teamMgr.Add(Team.Alpha, (uint) (Room.Options.PlayerLimit / 2), (uint) (Room.Options.SpectatorLimit / 2));
            teamMgr.Add(Team.Beta, (uint) (Room.Options.PlayerLimit / 2), (uint) (Room.Options.SpectatorLimit / 2));
            _currentRound = 0;
            base.Initialize();
        }

        public override void Update(TimeSpan delta)
        {
            base.Update(delta);

            var teamMgr = Room.TeamManager;

            if (StateMachine.IsInState(GameRuleState.Playing) &&
                !StateMachine.IsInState(GameRuleState.EnteringResult) &&
                !StateMachine.IsInState(GameRuleState.Result) &&
                RoundTime >= TimeSpan.FromSeconds(5)
            ) // Let the round run for at least 5 seconds - Fixes StartResult trigger on game start(race condition)
            {
                // Still have enough players?
                var min = teamMgr.Values.Min(team =>
                    team.Values.Count(plr =>
                        plr.RoomInfo.State != PlayerState.Lobby &&
                        plr.RoomInfo.State != PlayerState.Spectating));
                if (min == 0)
                    StateMachine.Fire(GameRuleStateTrigger.StartResult);

                var isFirstHalf = StateMachine.IsInState(GameRuleState.FullGame);
                if (isFirstHalf)
                {
                    // Did we reach ScoreLimit?
                    if (teamMgr.Values.Any(team => team.Score >= Room.Options.ScoreLimit))
                        StateMachine.Fire(GameRuleStateTrigger.StartResult);

                    // Did we reach round limit?
                    if (_currentRound >= Room.Options.TimeLimit.Minutes)
                        StateMachine.Fire(GameRuleStateTrigger.StartResult);

                    _captainHelper.Update(delta);

                    if (_captainHelper.Any())
                        SubRoundEnd();
                }

                if (_waitingNextRoom)
                {
                    _nextRoundTime += delta;
                    if (_nextRoundTime >= s_captainNextroundTime)
                    {
                        _captainHelper.Reset();
                        _waitingNextRoom = false;
                    }
                }
                else
                {
                    _subRoundTime += delta;
                    if (_subRoundTime >= s_captainRoundTime)
                        SubRoundEnd();
                }
            }
        }

        public override void Cleanup()
        {
            var teamMgr = Room.TeamManager;
            teamMgr.Remove(Team.Alpha);
            teamMgr.Remove(Team.Beta);
            base.Cleanup();
        }
        
        public override PlayerRecord GetPlayerRecord(Player plr)
        {
            return new CaptainPlayerRecord(plr);
        }

        public override void OnScoreKill(Player killer, Player assist, Player target, AttackAttribute attackAttribute,
            LongPeerId scoreTarget = null, LongPeerId scoreKiller = null, LongPeerId scoreAssist = null)
        {
            if (_captainHelper.Dead(target))
            {
                if (_captainHelper.Any())
                    SubRoundEnd();

                GetRecord(killer).KillCaptains++;
                if (assist != null)
                    GetRecord(assist).KillAssistCaptains++;
            }
            else
            {
                GetRecord(killer).Kills++;
                if (assist != null)
                    GetRecord(assist).KillAssists++;
            }

            GetRecord(target).Deaths++;

            base.OnScoreKill(killer, null, target, attackAttribute);
        }

        public override void OnScoreSuicide(Player plr, LongPeerId scoreTarget)
        {
            if (_captainHelper.Dead(plr) && _captainHelper.Any())
                SubRoundEnd();

            GetPlayerRecord(plr).Suicides++;

            base.OnScoreSuicide(plr);
        }

        private bool CanStartGame()
        {
            if (!StateMachine.IsInState(GameRuleState.Waiting))
                return false;

            if (Room.Options.IsFriendly)
                return true;

            var teams = Room.TeamManager.Values.ToArray();
            if (teams.Any(team => team.Count == 0)) // Do we have enough players?
                return false;

            // Is atleast one player per team ready?
            return teams.All(team => team.Players.Any(plr => plr.RoomInfo.IsReady || Room.Master == plr));
        }

        private void SubRoundEnd()
        {
            var teamwin = _captainHelper.TeamWin();
            _currentRound++;

            var teamMgr = Room.TeamManager;

            // Did we reach ScoreLimit or Round Limit?
            if (_currentRound >= Room.Options.TimeLimit.Minutes
                || teamMgr.Values.Any(team => team.Score >= Room.Options.ScoreLimit))
            {
                StateMachine.Fire(GameRuleStateTrigger.StartResult);
            }
            else
            {
                Room.Broadcast(
                    new CaptainSubRoundWinAckMessage
                    {
                        Unk1 = 0,
                        Unk2 = (byte) (teamwin.Team == Team.Alpha ? 1 : 2)
                    });
                Room.Broadcast(
                    new GameEventMessageAckMessage(GameEventMessage.NextRoundIn,
                        (ulong) s_captainNextroundTime.TotalMilliseconds, 0, 0, ""));

                _nextRoundTime = TimeSpan.Zero;
            }

            teamwin.Players.First().RoomInfo.Team.Score++;

            _subRoundTime = TimeSpan.Zero;
        }

        private static CaptainPlayerRecord GetRecord(Player plr)
        {
            return (CaptainPlayerRecord) plr.RoomInfo.Stats;
        }

        private void UpdatePlayerStats()
        {
            var WinTeam = Room
                .TeamManager
                .PlayersPlaying
                .Aggregate(
                    (highestTeam, player) =>
                        highestTeam == null || player.RoomInfo.Team.Score > highestTeam.RoomInfo.Team.Score
                            ? player
                            : highestTeam).RoomInfo.Team;
        }
    }

    internal class CaptainHelper
    {
        private IEnumerable<Player> _alpha;
        private IEnumerable<Player> _beta;
        private float _teamLife;

        public CaptainHelper(Room room)
        {
            Room = room;
            _alpha = from plr in Room.TeamManager.PlayersPlaying
                where plr.RoomInfo.Team.Team == Team.Alpha
                select plr;

            _beta = from plr in Room.TeamManager.PlayersPlaying
                where plr.RoomInfo.Team.Team == Team.Beta
                select plr;
        }

        public Room Room { get; }

        public void Reset()
        {
            _alpha = from plr in Room.TeamManager.PlayersPlaying
                where plr.RoomInfo.Team.Team == Team.Alpha
                select plr;

            _beta = from plr in Room.TeamManager.PlayersPlaying
                where plr.RoomInfo.Team.Team == Team.Beta
                select plr;

            float max = _alpha.Count() > _beta.Count() ? _alpha.Count() : _beta.Count();

            _teamLife = max * 500.0f;

            var players = (from plr in Room.TeamManager.PlayersPlaying
                    select new CaptainLifeDto {AccountId = plr.Account.Id, HP = _teamLife / plr.RoomInfo.Team.Count()})
                .ToArray();

            foreach (var plr in Room.TeamManager.PlayersPlaying) plr.RoomInfo.State = PlayerState.Alive;

            Room.Broadcast(new CaptainRoundCaptainLifeInfoAckMessage {Players = players});
            Room.Broadcast(new GameEventMessageAckMessage(GameEventMessage.ResetRound, 0, 0, 0, ""));
        }

        public bool Dead(Player target)
        {
            if (target.RoomInfo.Team.Team == Team.Alpha)
            {
                var isCaptain = (from plr in _alpha
                    where plr == target
                    select plr).Any();

                _alpha = from plr in _alpha
                    where plr != target
                    select plr;

                target.Room.Broadcast(
                    new CaptainCurrentRoundInfoAckMessage {Unk1 = _alpha.Count(), Unk2 = _beta.Count()});

                return isCaptain;
            }

            if (target.RoomInfo.Team.Team == Team.Beta)
            {
                var isCaptain = (from plr in _beta
                    where plr == target
                    select plr).Any();

                _beta = from plr in _beta
                    where plr != target
                    select plr;

                target.Room.Broadcast(
                    new CaptainCurrentRoundInfoAckMessage {Unk1 = _alpha.Count(), Unk2 = _beta.Count()});

                return isCaptain;
            }

            return false; // we need this?
        }

        public bool Any()
        {
            return !_alpha.Any() || !_beta.Any();
        }

        public PlayerTeam TeamWin()
        {
            if (!_alpha.Any())
                return Room.TeamManager.GetValueOrDefault(Team.Beta);

            if (!_beta.Any())
                return Room.TeamManager.GetValueOrDefault(Team.Alpha);

            return _alpha.Count() > _beta.Count()
                ? Room.TeamManager.GetValueOrDefault(Team.Alpha)
                : Room.TeamManager.GetValueOrDefault(Team.Beta);
        }

        public void Update(TimeSpan delta)
        {
            _alpha = from plr in Room.TeamManager.PlayersPlaying
                join oplr in _alpha on plr equals oplr
                select plr;

            _beta = from plr in Room.TeamManager.PlayersPlaying
                join oplr in _beta on plr equals oplr
                select plr;
        }
    }

    internal class CaptainBriefing : Briefing
    {
        public CaptainBriefing(GameRuleBase RuleBase)
            : base(RuleBase)
        {
        }
    }

    internal class CaptainPlayerRecord : PlayerRecord
    {
        public CaptainPlayerRecord(Player plr)
            : base(plr)
        {
        }

        public override uint TotalScore => 5 * (WinRound + KillCaptains) + 2 * Kills + KillAssists + Heal - Suicides;
        public uint KillCaptains { get; set; }
        public uint KillAssistCaptains { get; set; }
        public uint WinRound { get; set; }
        public uint Heal { get; set; }

        public override void Serialize(BinaryWriter w, bool isResult)
        {
            base.Serialize(w, isResult);

            w.Write(KillCaptains);
            w.Write(KillAssistCaptains);
            w.Write(Kills);
            w.Write(KillAssists);
            w.Write(Heal);
            w.Write(WinRound);
        }

        public override void Reset()
        {
            base.Reset();
            KillCaptains = 0;
            KillAssistCaptains = 0;
            Heal = 0;
        }

        public override int GetExpGain(out int bonusExp)
        {
            return GetExpGain(out bonusExp);
            //base.GetExpGain(out bonusExp);

            //var config = Config.Instance.Game.TouchdownExpRates;
            //var place = 1;

            //var plrs = Player.Room.TeamManager.Players
            //    .Where(plr => plr.RoomInfo.State == PlayerState.Waiting &&
            //        plr.RoomInfo.Mode == PlayerGameMode.Normal)
            //    .ToArray();

            //foreach (var plr in plrs.OrderByDescending(plr => plr.RoomInfo.Stats.TotalScore))
            //{
            //    if (plr == Player)
            //        break;

            //    place++;
            //    if (place > 3)
            //        break;
            //}

            //var rankingBonus = 0f;
            //switch (place)
            //{
            //    case 1:
            //        rankingBonus = config.FirstPlaceBonus;
            //        break;

            //    case 2:
            //        rankingBonus = config.SecondPlaceBonus;
            //        break;

            //    case 3:
            //        rankingBonus = config.ThirdPlaceBonus;
            //        break;
            //}

            //return (uint)(TotalScore * config.ScoreFactor +
            //    rankingBonus +
            //    plrs.Length * config.PlayerCountFactor +
            //    Player.RoomInfo.PlayTime.TotalMinutes * config.ExpPerMin);
        }
    }
}
