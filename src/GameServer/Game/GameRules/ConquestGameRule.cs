using System;
using System.IO;
using System.Linq;
using NeoNetsphere;
using NeoNetsphere.Network.Data.GameRule;
using NeoNetsphere.Network.Message.GameRule;

// ReSharper disable once CheckNamespace
namespace Netsphere.Game.GameRules //placeholder for real Conquest, c&p of deathmatch&br
{
    internal class ConquestGameRule : GameRuleBase
    {
        public uint DropCount = 0;

        public ConquestGameRule(Room room)
            : base(room)
        {
            Briefing = new ConquestBriefing(this);

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
                .Permit(GameRuleStateTrigger.EndGame, GameRuleState.Waiting);
        }

        public override bool CountMatch => false;
        public override GameRule GameRule => GameRule.Horde;
        public override Briefing Briefing { get; }

        public override void Initialize()
        {
            var teamMgr = Room.TeamManager;
            teamMgr.Add(Team.Alpha, Room.Options.PlayerLimit, 0);
            teamMgr.Add(Team.Beta, 0, 0);

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
                if (StateMachine.IsInState(GameRuleState.FullGame))
                {
                    // Did we reach round limit?
                    var roundTimeLimit = TimeSpan.FromMilliseconds(Room.Options.TimeLimit.TotalMilliseconds);
                    if (RoundTime >= roundTimeLimit)
                        StateMachine.Fire(GameRuleStateTrigger.StartResult);
                }
        }

        public override PlayerRecord GetPlayerRecord(Player plr)
        {
            return new ConquestPlayerRecord(plr);
        }

        public override void OnScoreHeal(Player plr, LongPeerId scoreTarget)
        {
            Room.Broadcast(new ScoreHealAssistAckMessage(scoreTarget));
            //base.OnScoreHeal(plr);
        }

        public override void OnScoreKill(Player killer, Player assist, Player target, AttackAttribute attackAttribute,
            LongPeerId scoreTarget, LongPeerId scoreKiller, LongPeerId scoreAssist)
        {
            var realplayer = Room.Players.Values.Where(p => p.RoomInfo.Slot == scoreTarget.PeerId.Slot
                                                            && p.RoomInfo.PeerId == scoreTarget.PeerId
                                                            && p.RoomInfo.PeerId.PeerId.Id == scoreTarget.PeerId.Id
                                                            && p.RoomInfo.PeerId.AccountId == scoreTarget.AccountId
                                                            && p.RoomInfo.PeerId.PeerId.Category == scoreTarget.PeerId.Category);
            if (realplayer.Any())
                Respawn(realplayer.First());

            var realplayer2 = Room.Players.Values.Where(p => p.RoomInfo.Slot == scoreKiller.PeerId.Slot
                                                            && p.RoomInfo.PeerId == scoreKiller.PeerId
                                                            && p.RoomInfo.PeerId.PeerId.Id == scoreKiller.PeerId.Id
                                                            && p.RoomInfo.PeerId.AccountId == scoreKiller.AccountId
                                                            && p.RoomInfo.PeerId.PeerId.Category == scoreKiller.PeerId.Category);
            if (realplayer2.Any())
                GetRecord(realplayer2.First()).Kills++;

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
            var realplayer = Room.Players.Values.Where(p => p.RoomInfo.Slot == scoreTarget.PeerId.Slot
                                                            && p.RoomInfo.PeerId == scoreTarget.PeerId
                                                            && p.RoomInfo.PeerId.PeerId.Id == scoreTarget.PeerId.Id
                                                            && p.RoomInfo.PeerId.AccountId == scoreTarget.AccountId
                                                            && p.RoomInfo.PeerId.PeerId.Category == scoreTarget.PeerId.Category);
            if (realplayer.Any())
                Respawn(realplayer.First());
            Room.Broadcast(
                new ScoreTeamKillAckMessage(new Score2Dto(scoreKiller, scoreTarget,
                    attackAttribute)));
        }

        public override void OnScoreSuicide(Player plr, LongPeerId scoreTarget)
        {
            var realplayer = Room.Players.Values.Where(p => p.RoomInfo.Slot == scoreTarget.PeerId.Slot
                                                            && p.RoomInfo.PeerId == scoreTarget.PeerId
                                                            && p.RoomInfo.PeerId.PeerId.Id == scoreTarget.PeerId.Id
                                                            && p.RoomInfo.PeerId.AccountId == scoreTarget.AccountId
                                                            && p.RoomInfo.PeerId.PeerId.Category == scoreTarget.PeerId.Category);
            if (realplayer.Any())
                Respawn(realplayer.First());
            Room.Broadcast(new ScoreSuicideAckMessage(scoreTarget, AttackAttribute.KillOneSelf));
            //base.OnScoreSuicide(plr);
        }
        
        private bool CanStartGame()
        {
            //if (Room.TeamManager.Players.ToList().Count > 1)
            //    return false;
            return true;
        }

        private static ConquestPlayerRecord GetRecord(Player plr)
        {
            return (ConquestPlayerRecord) plr.RoomInfo.Stats;
        }
    }

    internal class ConquestBriefing : Briefing
    {
        public ConquestBriefing(GameRuleBase gameRule)
            : base(gameRule)
        {
        }

        protected override void WriteData(BinaryWriter w, bool isResult)
        {
            base.WriteData(w, isResult);
        }
    }

    internal class ConquestPlayerRecord : PlayerRecord
    {
        public ConquestPlayerRecord(Player plr)
            : base(plr)
        {
        }

        public override uint TotalScore => GetTotalScore();

        public override void Serialize(BinaryWriter w, bool isResult)
        {
            base.Serialize(w, isResult);
            w.Write(0);
            w.Write(0);
            w.Write(0);
            w.Write(0);
            w.Write(0);
            w.Write(0);
            w.Write(0);
        }
        
        private uint GetTotalScore()
        {
            return 0;
        }

        public override int GetExpGain(out int bonusExp)
        {
            bonusExp = 0;
            return 0;
        }
    }
}
