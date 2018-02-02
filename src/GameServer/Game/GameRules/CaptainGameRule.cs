using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NeoNetsphere;
using NeoNetsphere.Network.Data.GameRule;
using NeoNetsphere.Network.Message.GameRule;

// ReSharper disable once CheckNamespace
namespace Netsphere.Game.GameRules
{
    internal class CaptainGameRule : GameRuleBase
    {
        public override bool CountMatch => true;
        public override GameRule GameRule => GameRule.Captain;
        public override Briefing Briefing { get; }
        public int AlphaPoints { get; set; }
        public int BetaPoints { get; set; }
        public int Rounds { get; set; }


        public uint AlphaCaptain { get; set; }
        public uint BetaCaptain { get; set; }

        CaptainTeam _Alpha;
        CaptainTeam _Beta;

        private bool _firstround = true;
        private bool _waitingNextRound;
        private TimeSpan _nextRoundTimer;
        private static readonly TimeSpan s_nextRoundWaitTime = TimeSpan.FromSeconds(10);


        public CaptainGameRule(Room room)
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
                .Permit(GameRuleStateTrigger.EndGame, GameRuleState.Waiting);
        }

        public override void Initialize()
        {
            var teamMgr = Room.TeamManager;
            teamMgr.Add(Team.Alpha,6,0);
            teamMgr.Add(Team.Beta,6,0);

            base.Initialize();
        }

        public override void Cleanup()
        {
            var teamMgr = Room.TeamManager;
            teamMgr.Remove(Team.Alpha);
            teamMgr.Remove(Team.Beta);

            AlphaPoints = 0;
            BetaPoints = 0;
            Rounds = 0;

            base.Cleanup();
        }

        public void NextRound()
        {
            if(Rounds == 10)
            {
                StateMachine.Fire(GameRuleStateTrigger.StartResult);
                return;
            }
            if(AlphaPoints == Room.Options.ScoreLimit || BetaPoints == Room.Options.ScoreLimit)
            {
                StateMachine.Fire(GameRuleStateTrigger.StartResult);
                return;
            }

            if (/*!AreCaptainsAlive() && */!_waitingNextRound)
            {
                _waitingNextRound = true;
                _nextRoundTimer = TimeSpan.Zero;
                Room.Broadcast(new GameEventMessageAckMessage(GameEventMessage.NextRoundIn, (ulong)s_nextRoundWaitTime.TotalMilliseconds, 0, 0, ""));
                return;
            }
            _waitingNextRound = false;


            AlphaCaptain = 0;
            BetaCaptain = 0;

            uint count = 0;
            foreach (Player plr in Room.TeamManager.PlayersPlaying)
            {
                count++;
                plr.RoomInfo.Team.Score = count;

                if (plr.RoomInfo.Team.Team == Team.Alpha)
                {
                    AlphaCaptain++;
                }
                else if (plr.RoomInfo.Team.Team == Team.Beta)
                {
                    BetaCaptain++;
                }
            }

            Dictionary<Player, bool> AlphaTeam = new Dictionary<Player, bool>();
            Dictionary<Player, bool> BetaTeam = new Dictionary<Player, bool>();
            _Alpha = new CaptainTeam(AlphaTeam);
            _Beta = new CaptainTeam(AlphaTeam);
            _Alpha.players = new Dictionary<Player, bool>();
            _Beta.players = new Dictionary<Player, bool>();


            {
                int i = 0;
                foreach (Player plr in Room.TeamManager.PlayersPlaying)
                {
                    if (plr.RoomInfo.Team.Team == Team.Alpha)
                    {
                        AlphaTeam.Add(plr, true);
                        _Alpha.players.Add(plr,true);
                    }
                    if (plr.RoomInfo.Team.Team == Team.Beta)
                    {
                        BetaTeam.Add(plr, true);
                        _Beta.players.Add(plr, true);
                    }
                    i++;
                }
            }

            CaptainLifeDto[] player = new CaptainLifeDto[count];
            {
                int i = 0;
                foreach (Player plr in Room.TeamManager.PlayersPlaying)
                {
                    if (i < 16)
                    {
                        player[i] = new CaptainLifeDto()
                        {
                            AccountId = plr.Account.Id,
                            HP = 300,
                        };
                        plr.Session.SendAsync(new CaptainRoundCaptainLifeInfoAckMessage());
                        i++;
                    }
                }
            }

            Rounds++;
        }
        

        public override void Update(TimeSpan delta)
        {base.Update(delta);

            var teamMgr = Room.TeamManager;
            if(_firstround)
            {
                NextRound();
                _firstround = false;
            }


            if (StateMachine.IsInState(GameRuleState.Playing) &&
                !StateMachine.IsInState(GameRuleState.EnteringResult) &&
                !StateMachine.IsInState(GameRuleState.Result))
            {
               
            }
        }

        public override PlayerRecord GetPlayerRecord(Player plr)
        {
            return new CaptainPlayerRecord(plr);
        }

        private bool AreCaptainsAlive(Player target = null)
        {
            if (target == null)
            {

                uint teamAlphaAlive = 0;
                uint teamBetaAlive = 0;

                foreach (Player plr in Room.TeamManager.PlayersPlaying)
                {

                    if (plr.RoomInfo.Team.Team == Team.Alpha)
                    {
                        if (plr.RoomInfo.State == PlayerState.Alive)
                            teamAlphaAlive++;
                    }
                    if (plr.RoomInfo.Team.Team == Team.Beta)
                    {
                        if (plr.RoomInfo.State == PlayerState.Alive)
                            teamBetaAlive++;
                    }
                }

                if (teamBetaAlive == 0)
                {
                    AlphaPoints++;
                    NextRound();
                    return false;
                }
                else if (teamAlphaAlive == 0)
                {
                    BetaPoints++;
                    NextRound();
                    return false;
                }
            }
            else
            {
                if (target.RoomInfo.Team.Team == Team.Alpha)
                {
                    _Alpha.players[target] = false;
                    target.RoomInfo.Team.Score -= 1;
                }
                else if (target.RoomInfo.Team.Team == Team.Beta)
                {
                    _Alpha.players[target] = false;
                    target.RoomInfo.Team.Score -= 1;
                }

                if (target.RoomInfo.Team.Team == Team.Alpha)
                {
                    AlphaCaptain--;
                }
                else if (target.RoomInfo.Team.Team == Team.Beta)
                {
                    BetaCaptain--;
                }

                if (!_waitingNextRound)
                {
                    if (BetaCaptain == 0)
                    {
                        target.RoomInfo.Team.Score = 0;
                        AlphaPoints += 1;
                        NextRound();
                    }
                    else if (AlphaCaptain == 0)
                    {
                        target.RoomInfo.Team.Score = 0;
                        BetaPoints += 1;
                        NextRound();
                    }
                }
            }
            return true;
        }

        public override void OnScoreKill(Player killer, Player assist, Player target, AttackAttribute attackAttribute,
            LongPeerId scoreTarget = null, LongPeerId scoreKiller = null, LongPeerId scoreAssist = null)
        {
            base.OnScoreKill(killer, assist, target, attackAttribute);
            Room.Broadcast(new GameEventMessageAckMessage(GameEventMessage.ChatMessage, target.Account.Id,0,0, $"[SERVER] Player {target.Account.Nickname} died")); 
            AreCaptainsAlive(target);
        }

        public override void OnScoreSuicide(Player plr, LongPeerId scoreTarget = null)
        {
            base.OnScoreSuicide(plr);
            AreCaptainsAlive(plr);
        }

        public override void OnScoreHeal(Player plr, LongPeerId scoreTarget = null)
        {
            GetRecord(plr).HealAssists++;
            base.OnScoreHeal(plr);
        }

        private bool CanStartGame()
        {
            if (!StateMachine.IsInState(GameRuleState.Waiting))
                return false;
            if (Room.Options.IsFriendly)
                return true;
            var teams = Room.TeamManager.Values.ToArray();
            
            if (teams.Any(team => team.Count == 0))
                return false;
            
            return teams.All(team => team.Players.Any(plr => plr.RoomInfo.IsReady || Room.Master == plr));
        }

        private static DeathmatchPlayerRecord GetRecord(Player plr)
        {
            return (DeathmatchPlayerRecord)plr.RoomInfo.Stats;
        }
    }

    //internal class CaptainPlayer
    //{
    //    public Player player { get; set; }
    //    public Team team { get; set; }
    //    public bool isCaptain { get; set; }
    //
    //    internal CaptainPlayer(Player Plr, Team Team)
    //    {
    //        player = Plr;
    //        team = Team;
    //        isCaptain = true;
    //    }
    //}

    internal class CaptainTeam
    {
        public Dictionary<Player, bool> players;
        internal CaptainTeam(Dictionary<Player,bool> Plrs)
        {
            players = Plrs;
        }
    }

    internal class CaptainPlayerRecord : PlayerRecord
    {
        public override uint TotalScore => GetTotalScore();

        public int HealAssists { get; set; }
        public int Unk { get; set; }
        public int Deaths2 { get; set; }
        public int Deaths3 { get; set; }

        public CaptainPlayerRecord(Player plr)
            : base(plr)
        { }

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

        public override void Reset()
        {
            base.Reset();

            HealAssists = 0;
            Unk = 0;
            Deaths2 = 0;
            Deaths3 = 0;
        }

        private uint GetTotalScore()
        {
            return (uint)(Kills * 2 + KillAssists + HealAssists * 2);
        }
    }
}
