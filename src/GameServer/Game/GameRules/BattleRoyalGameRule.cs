using System;
using System.IO;
using System.Linq;
using NeoNetsphere;
using NeoNetsphere.Network.Message.GameRule;

// ReSharper disable once CheckNamespace
namespace Netsphere.Game.GameRules
{
    internal class BattleRoyalGameRule : GameRuleBase
    {
        private const uint PlayersNeededToStart = 2;

        private Player _first;

        public BattleRoyalGameRule(Room room)
            : base(room)
        {
            Briefing = new Briefing(this);

            StateMachine.Configure(GameRuleState.Waiting)
                .PermitIf(GameRuleStateTrigger.StartPrepare, GameRuleState.Preparing, CanStartGame);

            StateMachine.Configure(GameRuleState.Preparing)
                .Permit(GameRuleStateTrigger.StartGame, GameRuleState.FullGame);

            StateMachine.Configure(GameRuleState.FullGame)
                .SubstateOf(GameRuleState.Playing)
                .Permit(GameRuleStateTrigger.StartResult, GameRuleState.EnteringResult);

            StateMachine.Configure(GameRuleState.EnteringResult)
                .SubstateOf(GameRuleState.Playing)
                .Permit(GameRuleStateTrigger.StartResult, GameRuleState.Result);

            StateMachine.Configure(GameRuleState.Result)
                .SubstateOf(GameRuleState.Playing)
                .Permit(GameRuleStateTrigger.EndGame, GameRuleState.Waiting)
                .OnEntry(() => { First = null; });
        }

        public override GameRule GameRule => GameRule.BattleRoyal;
        public override bool CountMatch => true;
        public override Briefing Briefing { get; }

        public Player First
        {
            get => _first;
            private set
            {
                if (_first == value)
                    return;
                _first = value;
                if (StateMachine.IsInState(GameRuleState.Playing))
                    Room.Broadcast(new FreeAllForChangeTheFirstAckMessage(_first?.Account.Id ?? 0));
            }
        }

        public override void IntrudeCompleted(Player plr)
        {
            base.IntrudeCompleted(plr);
            plr.Session.SendAsync(new FreeAllForChangeTheFirstAckMessage(First?.Account.Id ?? 0));
        }

        public override void Initialize()
        {
            Room.TeamManager.Add(Team.Alpha, Room.Options.PlayerLimit, (uint) Room.Options.Spectator);

            base.Initialize();
        }

        public override void Cleanup()
        {
            Room.TeamManager.Remove(Team.Alpha);

            base.Cleanup();
        }

        public override void Update(TimeSpan delta)
        {
            base.Update(delta);

            var teamMgr = Room.TeamManager;

            if (StateMachine.IsInState(GameRuleState.Playing) &&
                !StateMachine.IsInState(GameRuleState.EnteringResult) &&
                !StateMachine.IsInState(GameRuleState.Result) &&
                RoundTime >= TimeSpan.FromSeconds(5))
                if (StateMachine.IsInState(GameRuleState.FullGame))
                {
                    // Still have enough players?
                    if (!Room.Options.IsFriendly && teamMgr.PlayersPlaying.Count() < PlayersNeededToStart &&
                        !Room.Options.IsFriendly)
                        StateMachine.Fire(GameRuleStateTrigger.StartResult);

                    // Did we reach ScoreLimit?
                    if (teamMgr.PlayersPlaying.Any(plr => plr.RoomInfo.Stats.TotalScore >= Room.Options.ScoreLimit))
                        StateMachine.Fire(GameRuleStateTrigger.StartResult);

                    // Did we reach round limit?
                    var roundTimeLimit = TimeSpan.FromMilliseconds(Room.Options.TimeLimit.TotalMilliseconds);
                    if (RoundTime >= roundTimeLimit)
                        StateMachine.Fire(GameRuleStateTrigger.StartResult);
                }
        }

        public override PlayerRecord GetPlayerRecord(Player plr)
        {
            return new BattleRoyalPlayerRecord(plr);
        }

        public override void OnScoreKill(Player killer, Player assist, Player target, AttackAttribute attackAttribute,
            LongPeerId scoreTarget = null, LongPeerId scoreKiller = null, LongPeerId scoreAssist = null)
        {
            base.OnScoreKill(killer, assist, target, attackAttribute, scoreTarget, scoreKiller, scoreAssist);

            if (scoreTarget != null && scoreTarget.PeerId.Category != PlayerCategory.Player)
            {
            }
            else if (target == First)
            {
                GetRecord(killer).BonusKills++;

                if (assist != null)
                    GetRecord(assist).BonusKillAssists++;

                // Remove kill scores when this was a bonus kill
                GetRecord(killer).Kills--;
                if (assist != null)
                    GetRecord(assist).KillAssists--;
            }

            First = GetFirst();
        }

        private Player GetFirst()
        {
            return Room.TeamManager.PlayersPlaying.Aggregate((highestPlayer, player) =>
                highestPlayer == null || player.RoomInfo.Stats.TotalScore > highestPlayer.RoomInfo.Stats.TotalScore
                    ? player
                    : highestPlayer);
        }

        private bool CanStartGame()
        {
            if (!StateMachine.IsInState(GameRuleState.Waiting))
                return false;

            var teams = Room.TeamManager.Values.ToArray();
            if (Room.Options.IsFriendly)
                return true;
            if (teams.Any(team => team.Count == 0)) // Do we have enough players?
                return false;
            if (Room.Players.Where(plr => plr.Value.RoomInfo.IsReady).ToArray().Length + 1 < PlayersNeededToStart)
                return false;

            // Is atleast one player per team ready?
            return teams.All(team => team.Players.Any(plr => plr.RoomInfo.IsReady || Room.Master == plr));
        }
        

        private static BattleRoyalPlayerRecord GetRecord(Player plr)
        {
            return (BattleRoyalPlayerRecord) plr.RoomInfo.Stats;
        }
    }

    internal class BattleRoyalPlayerRecord : PlayerRecord
    {
        public BattleRoyalPlayerRecord(Player plr)
            : base(plr)
        {
        }

        public override uint TotalScore => GetTotalScore();

        public uint BonusKills { get; set; }
        public uint BonusKillAssists { get; set; }
        public int Unk5 { get; set; }
        public int Unk6 { get; set; }
        public int Unk7 { get; set; } // Increases kill score
        public int Unk8 { get; set; } // increases kill assist score
        public int Unk9 { get; set; }
        public int Unk10 { get; set; }
        public int Unk11 { get; set; }

        public override void Serialize(BinaryWriter w, bool isResult)
        {
            base.Serialize(w, isResult);

            w.Write(Kills);
            w.Write(KillAssists);
            w.Write(BonusKills);
            w.Write(BonusKillAssists);
            w.Write(Unk5);
            w.Write(Unk6);
            w.Write(Unk7);
            w.Write(Unk8);
            w.Write(Unk9);
            w.Write(Unk10);
            w.Write(Unk11);
        }

        public override void Reset()
        {
            base.Reset();

            KillAssists = 0;
            BonusKills = 0;
            BonusKillAssists = 0;
            Unk5 = 0;
            Unk6 = 0;
            Unk7 = 0;
            Unk8 = 0;
            Unk9 = 0;
            Unk10 = 0;
            Unk11 = 0;
        }

        private uint GetTotalScore()
        {
            return Kills * 2 +
                   KillAssists +
                   BonusKills * 5 +
                   BonusKillAssists;
        }

        public override int GetExpGain(out int bonusExp)
        {
            base.GetExpGain(out bonusExp);

            var config = Config.Instance.Game.BRExpRates;
            var place = 1;

            var plrs = Player.Room.TeamManager.Players
                .Where(plr => plr.RoomInfo.State == PlayerState.Waiting &&
                              plr.RoomInfo.Mode == PlayerGameMode.Normal)
                .ToArray();

            foreach (var plr in plrs.OrderByDescending(plr => plr.RoomInfo.Stats.TotalScore))
            {
                if (plr == Player)
                    break;

                place++;
                if (place > 3)
                    break;
            }

            var rankingBonus = 0f;
            switch (place)
            {
                case 1:
                    rankingBonus = config.FirstPlaceBonus;
                    break;

                case 2:
                    rankingBonus = config.SecondPlaceBonus;
                    break;

                case 3:
                    rankingBonus = config.ThirdPlaceBonus;
                    break;
            }

            return (int) (TotalScore * config.ScoreFactor +
                          rankingBonus +
                          plrs.Length * config.PlayerCountFactor +
                          Player.RoomInfo.PlayTime.TotalMinutes * config.ExpPerMin);
        }
    }
}
