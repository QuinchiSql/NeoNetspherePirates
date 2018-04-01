using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Netsphere;
using Netsphere.Game;
using Netsphere.Game.GameRules;
using Serilog;
using Serilog.Core;

namespace NeoNetsphere.Game.GameRules
{
    internal class PracticeGameRule : GameRuleBase
    {
        // ReSharper disable once InconsistentNaming
        private static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(PracticeGameRule));

        public override GameRule GameRule => GameRule.Practice;
        public override Briefing Briefing { get; }
        public override bool CountMatch => false;

        public PracticeGameRule(Room room)
            : base(room)
        {
            Briefing = new Briefing(this);

            StateMachine.Configure(GameRuleState.Waiting)
                .Permit(GameRuleStateTrigger.StartPrepare, GameRuleState.Preparing);

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

        public override void Initialize()
        {
            var teamMgr = Room.TeamManager;
            teamMgr.Add(Team.Alpha, 1, 0);
            base.Initialize();
        }

        public override void Update(TimeSpan delta)
        {
            if (StateMachine.IsInState(GameRuleState.Playing) &&
                !StateMachine.IsInState(GameRuleState.EnteringResult) &&
                !StateMachine.IsInState(GameRuleState.Result) &&
                RoundTime >= TimeSpan.FromSeconds(5)) // Let the round run for at least 5 seconds - Fixes StartResult trigger on game start(race condition)
            {
                if (RoundTime >= Room.Options.TimeLimit)
                    StateMachine.Fire(GameRuleStateTrigger.StartResult);
            }

            base.Update(delta);
        }

        public override void Cleanup()
        {
            Room.TeamManager.Remove(Team.Alpha);
            base.Cleanup();
        }

        public override PlayerRecord GetPlayerRecord(Player plr)
        {
            return new PracticeRecord(plr);
        }

        public override void OnScoreKill(Player killer, Player assist, Player target, AttackAttribute attackAttribute,
            LongPeerId scoreTarget = null, LongPeerId scoreKiller = null, LongPeerId scoreAssist = null)
        {
            GetRecord(killer).Kills++;

            base.OnScoreKill(killer, null, target, attackAttribute);
        }

        private static PracticeRecord GetRecord(Player plr)
        {
            return (PracticeRecord)plr.RoomInfo.Stats;
        }

        private bool CanStart()
        {
            return true;
        }
    }

    internal class PracticeRecord : PlayerRecord
    {
        public override uint TotalScore => Kills;
        public uint Unk1 { get; set; }
        public uint Unk2 { get; set; }
        public uint Unk3 { get; set; }
        public uint Unk4 { get; set; }
        public uint Unk5 { get; set; }
        public uint Unk6 { get; set; }
        public uint Unk7 { get; set; }
        public uint Unk8 { get; set; }

        public PracticeRecord(Player plr)
            : base(plr)
        {
            Unk1 = 1;
            Unk2 = 2;
            Unk3 = 3;
            Unk4 = 4;
            Unk5 = 5;
            Unk6 = 6;
            Unk7 = 7;
            Unk8 = 8;
        }

        public override void Serialize(BinaryWriter w, bool isResult)
        {
            base.Serialize(w, isResult);

            w.Write(Unk1);
            w.Write(Unk2);
            w.Write(Unk3);
            w.Write(Unk4);
            w.Write(Unk5);
            w.Write(Unk6);
            w.Write(Unk7);
            w.Write(Unk8);
            w.Write(Unk1);
            w.Write(Unk2);
            w.Write(Unk3);
            w.Write(Unk4);
            w.Write(Unk5);
            w.Write(Unk6);
            w.Write(Unk7);
            w.Write(Unk8);
            w.Write(Unk1);
            w.Write(Unk2);
            w.Write(Unk3);
            w.Write(Unk4);
            w.Write(Unk5);
            w.Write(Unk6);
            w.Write(Unk7);
            w.Write(Unk8);
        }
    }
}
