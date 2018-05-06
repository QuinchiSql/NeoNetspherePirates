using System;
using System.IO;
using System.Linq;
using NeoNetsphere.Network.Data.GameRule;
using NeoNetsphere.Network.Message.GameRule;
using Netsphere;
using Netsphere.Game;
using Netsphere.Game.GameRules;

namespace NeoNetsphere.Game.GameRules
{
    internal sealed class TouchdownTrainingGameRule : GameRuleBase
    {
        private static readonly TimeSpan TouchdownTrainingWaitTime = TimeSpan.FromSeconds(12);

        private readonly TouchdownTrainingAssistHelper _TouchdownTrainingAssistHelper =
            new TouchdownTrainingAssistHelper();

        private TimeSpan _TouchdownTrainingTime;

        public TouchdownTrainingGameRule(Room room)
            : base(room)
        {
            Briefing = new Briefing(this);

            StateMachine.Configure(GameRuleState.Waiting)
                .PermitIf(GameRuleStateTrigger.StartPrepare, GameRuleState.Preparing, CanStartGame);

            StateMachine.Configure(GameRuleState.Preparing)
                .Permit(GameRuleStateTrigger.StartGame, GameRuleState.FirstHalf);

            StateMachine.Configure(GameRuleState.FirstHalf)
                .SubstateOf(GameRuleState.Playing)
                .Permit(GameRuleStateTrigger.StartHalfTime, GameRuleState.EnteringHalfTime)
                .Permit(GameRuleStateTrigger.StartResult, GameRuleState.EnteringResult);

            StateMachine.Configure(GameRuleState.EnteringHalfTime)
                .SubstateOf(GameRuleState.Playing)
                .Permit(GameRuleStateTrigger.StartHalfTime, GameRuleState.HalfTime)
                .Permit(GameRuleStateTrigger.StartResult, GameRuleState.EnteringResult);

            StateMachine.Configure(GameRuleState.HalfTime)
                .SubstateOf(GameRuleState.Playing)
                .Permit(GameRuleStateTrigger.StartSecondHalf, GameRuleState.SecondHalf)
                .Permit(GameRuleStateTrigger.StartResult, GameRuleState.EnteringResult);

            StateMachine.Configure(GameRuleState.SecondHalf)
                .SubstateOf(GameRuleState.Playing)
                .Permit(GameRuleStateTrigger.StartResult, GameRuleState.EnteringResult);

            StateMachine.Configure(GameRuleState.EnteringResult)
                .SubstateOf(GameRuleState.Playing)
                .Permit(GameRuleStateTrigger.StartResult, GameRuleState.Result);

            StateMachine.Configure(GameRuleState.Result)
                .SubstateOf(GameRuleState.Playing)
                .Permit(GameRuleStateTrigger.EndGame, GameRuleState.Waiting);
        }

        public Player BallOwner { get; private set; }

        public override bool CountMatch => false;
        public override GameRule GameRule => GameRule.CombatTrainingTD;
        public override Briefing Briefing { get; }
        public bool IsInTouchdownTraining { get; private set; }

        public override void Initialize()
        {
            var teamMgr = Room.TeamManager;
            teamMgr.Add(Team.Alpha, (uint) (Room.Options.PlayerLimit / 2), (uint) (Room.Options.SpectatorLimit / 2));
            teamMgr.Add(Team.Beta, (uint) (Room.Options.PlayerLimit / 2), (uint) (Room.Options.SpectatorLimit / 2));

            base.Initialize();
        }

        public override void Cleanup()
        {
            var teamMgr = Room.TeamManager;
            teamMgr.Remove(Team.Alpha);
            teamMgr.Remove(Team.Beta);

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
            {
                // Still have enough players?
                var min = teamMgr.Values.Min(team =>
                    team.Values.Count(plr =>
                        plr.RoomInfo.State != PlayerState.Lobby &&
                        plr.RoomInfo.State != PlayerState.Spectating));
                if (min == 0 && !Room.Options.IsFriendly)
                    StateMachine.Fire(GameRuleStateTrigger.StartResult);

                var isFirstHalf = StateMachine.IsInState(GameRuleState.FirstHalf);
                var isSecondHalf = StateMachine.IsInState(GameRuleState.SecondHalf);
                if (isFirstHalf || isSecondHalf)
                {
                    var scoreLimit = isFirstHalf ? Room.Options.ScoreLimit / 2 : Room.Options.ScoreLimit;
                    var trigger = isFirstHalf ? GameRuleStateTrigger.StartHalfTime : GameRuleStateTrigger.StartResult;

                    // Did we reach ScoreLimit?
                    if (teamMgr.Values.Any(team => team.Score >= scoreLimit))
                        StateMachine.Fire(trigger);

                    // Did we reach round limit?
                    var roundTimeLimit = TimeSpan.FromMilliseconds(Room.Options.TimeLimit.TotalMilliseconds / 2);
                    if (RoundTime >= roundTimeLimit)
                        StateMachine.Fire(trigger);
                }

                if (IsInTouchdownTraining)
                {
                    _TouchdownTrainingTime += delta;
                    if (!StateMachine.IsInState(GameRuleState.EnteringHalfTime) &&
                        !StateMachine.IsInState(GameRuleState.HalfTime) &&
                        !StateMachine.IsInState(GameRuleState.EnteringResult) &&
                        !StateMachine.IsInState(GameRuleState.Result))
                    {
                        if (_TouchdownTrainingTime >= TouchdownTrainingWaitTime)
                        {
                            IsInTouchdownTraining = false;
                            _TouchdownTrainingTime = TimeSpan.Zero;
                            Room.Broadcast(new GameEventMessageAckMessage(GameEventMessage.ResetRound, 0, 0, 0, ""));
                        }
                    }
                    else
                    {
                        IsInTouchdownTraining = false;
                    }
                }
            }
        }

        public override PlayerRecord GetPlayerRecord(Player plr)
        {
            return new TouchdownTrainingPlayerRecord(plr);
        }

        public void OnScoreOffense(Player killer, Player assist, Player target, AttackAttribute attackAttribute,
            LongPeerId Target, LongPeerId Killer, LongPeerId Assist)
        {
            if (IsInTouchdownTraining)
                return;

            var realplayer = Room.Players.Values.Where(p => p.RoomInfo.Slot == Target.PeerId.Slot
                                                            && p.RoomInfo.PeerId == Target.PeerId
                                                            && p.RoomInfo.PeerId.PeerId.Id == Target.PeerId.Id
                                                            && p.RoomInfo.PeerId.AccountId == Target.AccountId
                                                            && p.RoomInfo.PeerId.PeerId.Category ==
                                                            Target.PeerId.Category);
            if (realplayer.Any())
                Respawn(realplayer.First());
            //GetRecord(killer).OffenseScore++;
            //if (assist != null)
            //    GetRecord(assist).OffenseAssistScore++;

            if (assist != null)
                Room.Broadcast(
                    new ScoreOffenseAssistAckMessage(new ScoreAssistDto(Killer, Assist, Target, attackAttribute)));
            else
                Room.Broadcast(new ScoreOffenseAckMessage(new ScoreDto(Killer, Target, attackAttribute)));
        }

        public void OnScoreDefense(Player killer, Player assist, Player target, AttackAttribute attackAttribute,
            LongPeerId Target, LongPeerId Killer, LongPeerId Assist)
        {
            if (IsInTouchdownTraining)
                return;

            var realplayer = Room.Players.Values.Where(p => p.RoomInfo.Slot == Target.PeerId.Slot
                                                            && p.RoomInfo.PeerId == Target.PeerId
                                                            && p.RoomInfo.PeerId.PeerId.Id == Target.PeerId.Id
                                                            && p.RoomInfo.PeerId.AccountId == Target.AccountId
                                                            && p.RoomInfo.PeerId.PeerId.Category ==
                                                            Target.PeerId.Category);
            if (realplayer.Any())
                Respawn(realplayer.First());
            //GetRecord(killer).DefenseScore++;
            //if (assist != null)
            //    GetRecord(assist).DefenseAssistScore++;

            if (assist != null)
                Room.Broadcast(
                    new ScoreDefenseAssistAckMessage(new ScoreAssistDto(Killer, Assist, Target, attackAttribute)));
            else
                Room.Broadcast(new ScoreDefenseAckMessage(new ScoreDto(Killer, Target, attackAttribute)));
        }

        public void OnScoreRebound(Player newPlr, Player oldPlr, LongPeerId newid, LongPeerId oldId)
        {
            if (IsInTouchdownTraining || newPlr == BallOwner)
                return;

            if (newPlr != null)
                GetRecord(newPlr).OffenseReboundScore++;
            BallOwner = newPlr;
            if (oldId == 0)
                oldId = newid;
            Room.Broadcast(new ScoreReboundAckMessage(newid, oldId));
        }

        public void OnScoreGoal(Player plr, LongPeerId Target)
        {
            IsInTouchdownTraining = true;

            Player assist = null;
            //if (_TouchdownTrainingAssistHelper.IsAssist(plr))
            //{
            //    assist = _TouchdownTrainingAssistHelper.LastPlayer;
            //    GetRecord(assist).TdAssistScore++;
            //}

            plr.RoomInfo.Team.Score++;
            GetRecord(plr).TDScore++;

            if (assist != null)
                Room.Broadcast(new ScoreGoalAssistAckMessage(Target, assist.RoomInfo.PeerId));
            else
                Room.Broadcast(new ScoreGoalAckMessage(Target));

            var halfTime = TimeSpan.FromSeconds(Room.Options.TimeLimit.TotalSeconds / 2);
            var diff = halfTime - RoundTime;
            if (diff <= TimeSpan.FromSeconds(10)) // ToDo use const
                return;

            Room.Broadcast(new GameEventMessageAckMessage(GameEventMessage.NextRoundIn,
                (ulong) TouchdownTrainingWaitTime.TotalMilliseconds, 0, 0, ""));
            _TouchdownTrainingTime = TimeSpan.Zero;
        }

        public override void OnScoreHeal(Player plr, LongPeerId scoreTarget)
        {
            if (IsInTouchdownTraining)
                return;

            Room.Broadcast(new ScoreHealAssistAckMessage(scoreTarget));
            //base.OnScoreHeal(plr);
        }

        public override void OnScoreKill(Player killer, Player assist, Player target, AttackAttribute attackAttribute,
            LongPeerId scoreTarget, LongPeerId scoreKiller, LongPeerId scoreAssist)
        {
            if (IsInTouchdownTraining)
                return;

            var realplayer = Room.Players.Values.Where(p => p.RoomInfo.Slot == scoreTarget.PeerId.Slot
                                                            && p.RoomInfo.PeerId == scoreTarget.PeerId
                                                            && p.RoomInfo.PeerId.PeerId.Id == scoreTarget.PeerId.Id
                                                            && p.RoomInfo.PeerId.AccountId == scoreTarget.AccountId
                                                            && p.RoomInfo.PeerId.PeerId.Category ==
                                                            scoreTarget.PeerId.Category);
            if (realplayer.Any())
                Respawn(realplayer.First());

            if (scoreAssist != null)
                Room.Broadcast(
                    new ScoreKillAssistAckMessage(new ScoreAssistDto(scoreKiller, scoreAssist,
                        scoreTarget, attackAttribute)));
            else
                Room.Broadcast(
                    new ScoreKillAckMessage(new ScoreDto(scoreKiller, scoreTarget,
                        attackAttribute)));
        }

        public override void OnScoreTeamKill(Player killer, Player target, AttackAttribute attackAttribute,
            LongPeerId scoreKiller, LongPeerId scoreTarget)
        {
            if (IsInTouchdownTraining)
                return;

            var realplayer = Room.Players.Values.Where(p => p.RoomInfo.Slot == scoreTarget.PeerId.Slot
                                                            && p.RoomInfo.PeerId == scoreTarget.PeerId
                                                            && p.RoomInfo.PeerId.PeerId.Id == scoreTarget.PeerId.Id
                                                            && p.RoomInfo.PeerId.AccountId == scoreTarget.AccountId
                                                            && p.RoomInfo.PeerId.PeerId.Category ==
                                                            scoreTarget.PeerId.Category);
            if (realplayer.Any())
                Respawn(realplayer.First());

            Room.Broadcast(
                new ScoreTeamKillAckMessage(new Score2Dto(scoreKiller, scoreTarget,
                    attackAttribute)));
        }

        public override void OnScoreSuicide(Player target, LongPeerId scoreTarget)
        {
            if (IsInTouchdownTraining)
                return;

            var realplayer = Room.Players.Values.Where(p => p.RoomInfo.Slot == scoreTarget.PeerId.Slot
                                                            && p.RoomInfo.PeerId == scoreTarget.PeerId
                                                            && p.RoomInfo.PeerId.PeerId.Id == scoreTarget.PeerId.Id
                                                            && p.RoomInfo.PeerId.AccountId == scoreTarget.AccountId
                                                            && p.RoomInfo.PeerId.PeerId.Category ==
                                                            scoreTarget.PeerId.Category);
            if (realplayer.Any())
                Respawn(realplayer.First());

            Room.Broadcast(new ScoreSuicideAckMessage(scoreTarget, AttackAttribute.KillOneSelf));
            //base.OnScoreSuicide(plr);
        }

        private bool CanStartGame()
        {
            if (!StateMachine.IsInState(GameRuleState.Waiting))
                return false;
            return true;
        }

        private static TouchdownTrainingPlayerRecord GetRecord(Player plr)
        {
            return (TouchdownTrainingPlayerRecord) plr.RoomInfo.Stats;
        }
    }

    internal class TouchdownTrainingPlayerRecord : PlayerRecord
    {
        public TouchdownTrainingPlayerRecord(Player plr)
            : base(plr)
        {
        }

        public override uint TotalScore => GetTotalScore();

        public uint TDScore { get; set; }
        public uint TDAssistScore { get; set; }
        public uint OffenseScore { get; set; }
        public uint OffenseAssistScore { get; set; }
        public uint DefenseScore { get; set; }
        public uint DefenseAssistScore { get; set; }
        public uint HealScore { get; set; }
        public uint Unk1 { get; set; }
        public uint Unk2 { get; set; }
        public uint Unk3 { get; set; }
        public uint OffenseReboundScore { get; set; }
        public uint Unk4 { get; set; } // increases total score x*4
        public uint Unk5 { get; set; }

        public override void Serialize(BinaryWriter w, bool isResult)
        {
            base.Serialize(w, isResult);

            w.Write(TDScore);
            w.Write(TDAssistScore);
            w.Write(Kills);
            w.Write(KillAssists);
            w.Write(OffenseScore);
            w.Write(OffenseAssistScore);
            w.Write(DefenseScore);
            w.Write(DefenseAssistScore);
            w.Write(HealScore);
            w.Write(Unk1);
            w.Write(Unk2);
            w.Write(Unk3);
            w.Write(OffenseReboundScore);
            w.Write(Unk4);
            w.Write(Unk5);
            var Unk6 = new byte[16];
            w.Write(Unk6);
        }

        public override void Reset()
        {
            base.Reset();
            TDScore = 0;
            TDAssistScore = 0;
            OffenseScore = 0;
            OffenseAssistScore = 0;
            DefenseScore = 0;
            DefenseAssistScore = 0;
            HealScore = 0;
            OffenseReboundScore = 0;

            Unk1 = 0;
            Unk2 = 0;
            Unk3 = 0;
            Unk4 = 0;
            Unk5 = 0;
        }

        private uint GetTotalScore()
        {
            return TDScore * 10 + TDAssistScore * 5
                                + Kills * 2 + KillAssists
                                + OffenseScore * 4 + OffenseAssistScore * 2
                                + DefenseScore * 4 + DefenseAssistScore * 2
                                + HealScore * 2
                                + OffenseReboundScore * 2;
        }

        public override int GetExpGain(out int bonusExp)
        {
            base.GetExpGain(out bonusExp);

            var config = Config.Instance.Game.TouchdownExpRates;
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

    internal class TouchdownTrainingAssistHelper
    {
        private static readonly TimeSpan TouchdownTrainingAssistTimer = TimeSpan.FromSeconds(10);

        public DateTime LastTime { get; set; }
        public Player LastPlayer { get; set; }

        public void Update(Player plr)
        {
            LastTime = DateTime.Now;
            LastPlayer = plr;
        }

        public bool IsAssist(Player plr)
        {
            if (LastPlayer == null)
                return false;

            if (plr.RoomInfo.Team != LastPlayer.RoomInfo.Team)
                return false;

            return DateTime.Now - LastTime < TouchdownTrainingAssistTimer;
        }
    }
}
