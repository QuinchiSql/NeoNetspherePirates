using System;
using System.IO;
using System.Linq;
using NeoNetsphere;
using NeoNetsphere.Network.Message.GameRule;

// ReSharper disable once CheckNamespace
namespace Netsphere.Game.GameRules
{
    internal class SiegeGameRule : GameRuleBase
    {
        private const uint PlayersNeededToStart = 1;

        private Player _first;

        public SiegeGameRule(Room room)
            : base(room)
        {
            Briefing = new Briefing(this);

            StateMachine.Configure(GameRuleState.Waiting)
                .PermitIf(GameRuleStateTrigger.StartPrepare, GameRuleState.Preparing, CanStartGame);

            StateMachine.Configure(GameRuleState.Preparing)
                .Permit(GameRuleStateTrigger.StartGame, GameRuleState.FirstHalf);

            StateMachine.Configure(GameRuleState.FirstHalf)
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

        public override bool CountMatch => true;
        public override GameRule GameRule => GameRule.Siege;
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

        public override void Initialize()
        {
            Room.TeamManager.Add(Team.Alpha, Room.Options.PlayerLimit, (uint) Room.Options.SpectatorLimit);

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
                if (StateMachine.IsInState(GameRuleState.FirstHalf))
                {
                    // Still have enough players?
                    if (teamMgr.PlayersPlaying.Count() < PlayersNeededToStart)
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
            return new SiegePlayerRecord(plr);
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

            var countReady = Room.TeamManager.Values.Sum(team => team.Values.Count(plr => plr.RoomInfo.IsReady));
            if (countReady < PlayersNeededToStart - 1) // Sum doesn't include master so decrease players needed by 1
                return false;
            return true;
        }


        private static SiegePlayerRecord GetRecord(Player plr)
        {
            return (SiegePlayerRecord) plr.RoomInfo.Stats;
        }
    }

    internal class SiegePlayerRecord : PlayerRecord
    {
        public SiegePlayerRecord(Player plr)
            : base(plr)
        {
        }

        public override uint TotalScore => GetTotalScore();

        public uint GetTotalScore()
        {
            return 0;
        }

        public override void Serialize(BinaryWriter w, bool isResult)
        {
            base.Serialize(w, isResult);

            //missing
        }
    }
}
