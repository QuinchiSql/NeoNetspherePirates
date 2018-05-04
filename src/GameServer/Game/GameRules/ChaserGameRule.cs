using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BlubLib.IO;
using NeoNetsphere.Network.Message.GameRule;
using Netsphere;
using Netsphere.Game;
using Netsphere.Game.GameRules;

namespace NeoNetsphere.Game.GameRules
{
    internal class ChaserGameRule : GameRuleBase
    {
        private const uint PlayersNeededToStart = 4;

        private static readonly TimeSpan SNextChaserWaitTime = TimeSpan.FromSeconds(10);
        private readonly SecureRandom _random = new SecureRandom();

        private TimeSpan _chaserRoundTime;
        private TimeSpan _chaserTimer;

        private bool _disallowactions;
        private bool _ischangingChaser;
        private TimeSpan _nextChaserTimer;
        private bool _waitingNextChaser;
        public int ChaserIndex = 0;

        public ChaserGameRule(Room room)
            : base(room)
        {
            Briefing = new ChaserBriefing(this);

            StateMachine.Configure(GameRuleState.Waiting)
                .PermitIf(GameRuleStateTrigger.StartPrepare, GameRuleState.Preparing, CanStartGame);

            StateMachine.Configure(GameRuleState.Preparing)
                .Permit(GameRuleStateTrigger.StartGame, GameRuleState.FullGame);

            StateMachine.Configure(GameRuleState.FullGame)
                .SubstateOf(GameRuleState.Playing)
                .Permit(GameRuleStateTrigger.StartResult, GameRuleState.EnteringResult)
                .OnEntry(NextChaser);
            
            StateMachine.Configure(GameRuleState.EnteringResult)
                .SubstateOf(GameRuleState.Playing)
                .Permit(GameRuleStateTrigger.StartResult, GameRuleState.Result);

            StateMachine.Configure(GameRuleState.Result)
                .SubstateOf(GameRuleState.Playing)
                .Permit(GameRuleStateTrigger.EndGame, GameRuleState.Waiting)
                .OnEntry(() =>
                {
                    Chaser = null;
                    Room.Broadcast(new SlaughterChangeSlaughterAckMessage(0));
                });
        }

        public override bool CountMatch => true;
        public override GameRule GameRule => GameRule.Chaser;
        public override Briefing Briefing { get; }

        public Player OldChaser { get; private set; }
        public Player Chaser { get; private set; }
        public Player ChaserTarget { get; private set; }

        public override void IntrudeCompleted(Player plr)
        {
            plr.RoomInfo.State = PlayerState.Dead;
            base.IntrudeCompleted(plr);
        }

        public override void Initialize()
        {
            Room.TeamManager.Add(Team.Alpha, (uint)Room.Options.PlayerLimit / 2, (uint) Room.Options.SpectatorLimit /2);
            Room.TeamManager.Add(Team.Beta, (uint)Room.Options.PlayerLimit / 2, (uint) Room.Options.SpectatorLimit / 2);
            base.Initialize();
        }

        public override void Cleanup()
        {
            Room.TeamManager.Remove(Team.Alpha);
            Room.TeamManager.Remove(Team.Beta);
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
                    if (teamMgr.PlayersPlaying.Count() < PlayersNeededToStart && !Room.Options.IsFriendly)
                        StateMachine.Fire(GameRuleStateTrigger.StartResult);

                    if (RoundTime >= Room.Options.TimeLimit)
                        StateMachine.Fire(GameRuleStateTrigger.StartResult);

                    if (_waitingNextChaser)
                    {
                        _disallowactions = true;
                        _nextChaserTimer += delta;
                        if (!_ischangingChaser)
                        {
                            _ischangingChaser = true;
                            Room.Broadcast(new GameEventMessageAckMessage(GameEventMessage.ChaserIn,
                                (ulong) SNextChaserWaitTime.TotalMilliseconds, 0, 0, ""));
                        }
                        if (_nextChaserTimer >= SNextChaserWaitTime)
                        {
                            NextChaser();
                            _ischangingChaser = false;
                            _nextChaserTimer = TimeSpan.Zero;
                        }
                    }
                    else
                    {
                        _chaserTimer += delta;
                        if (_chaserTimer >= _chaserRoundTime)
                        {
                            var diff = Room.Options.TimeLimit - RoundTime;
                            if (diff >= _chaserRoundTime + SNextChaserWaitTime)
                                ChaserLose();
                        }
                        else if (!ArePlayersAlive() && Room.TeamManager.PlayersPlaying.ToList().Count > 1)
                        {
                            ChaserWin();
                        }
                    }
                }
        }

        public override PlayerRecord GetPlayerRecord(Player plr)
        {
            return new ChaserPlayerRecord(plr);
        }
        
        public override void OnScoreKill(Player killer, Player assist, Player target, AttackAttribute attackAttribute,
            LongPeerId scoreTarget = null, LongPeerId scoreKiller = null, LongPeerId scoreAssist = null)
        {
            if (_disallowactions)
                return;
            var stats = GetRecord(killer);
            base.OnScoreKill(killer, assist, target, attackAttribute, scoreTarget, scoreKiller, scoreAssist);
            if (scoreTarget != null && scoreTarget.PeerId.Category != PlayerCategory.Player)
            {
            }
            else
            {
                if (!_waitingNextChaser)
                {
                    stats.Kills++;

                    if (killer == Chaser && target == ChaserTarget)
                        stats.BonusKills++;

                    if (target != Chaser)
                        target.RoomInfo.State = PlayerState.Dead;

                    if (!ArePlayersAlive())
                        ChaserWin();

                    if (Chaser == target)
                        ChaserLose();

                    NextTarget();
                }
                else
                {
                    killer.RoomInfo.Stats.Kills--;
                    target.RoomInfo.Stats.Deaths--;
                }
            }
        }

        public override void OnScoreSuicide(Player plr, LongPeerId scoreTarget = null)
        {
            if (_disallowactions)
                return;
            if (!_waitingNextChaser)
            {
                base.OnScoreSuicide(plr, scoreTarget);
                if (Chaser == plr)
                    ChaserLose();

                if (!ArePlayersAlive())
                    ChaserWin();

                if (plr != Chaser)
                    plr.RoomInfo.State = PlayerState.Dead;

                NextTarget();
            }
        }

        public void NextTarget()
        {
            var targetfound = false;

            foreach (var plr in Room.TeamManager.PlayersPlaying.OrderBy(g => Guid.NewGuid())
                .Where(x => x.RoomInfo.State == PlayerState.Alive && x != Chaser))
                if (!targetfound)
                {
                    ChaserTarget = plr;
                    targetfound = true;
                    Room.Broadcast(new SlaughterChangeBonusTargetAckMessage(ChaserTarget.Account.Id));
                }
        }

        public async void NextChaser()
        {
            try
            {
                _waitingNextChaser = false;
                _chaserRoundTime = Room.TeamManager.Players.Count() < 4
                    ? TimeSpan.FromSeconds(60)
                    : TimeSpan.FromSeconds(Room.TeamManager.Players.Count() * 15);
                _chaserRoundTime += TimeSpan.FromSeconds(Chaser != null ? 6 : 3);

                foreach (var plr in Room.TeamManager.PlayersPlaying)
                    plr.RoomInfo.State = PlayerState.Alive;

                OldChaser = Chaser;
                _chaserTimer = TimeSpan.Zero;

                if (Room.TeamManager.PlayersPlaying.Count() > 1)
                {
                    Chaser = Room.Players.Values.ElementAt(_random.Next(0, Room.Players.Count));
                }
                else if (Room.TeamManager.PlayersPlaying.Count() == 1)
                    Chaser = Room.TeamManager.PlayersPlaying.ToList()[0];

                GetRecord(Chaser).ChaserCount++;

                Room.Broadcast(new SlaughterChangeSlaughterAckMessage(Chaser?.Account.Id ?? 0,
                    new[] {Chaser?.Account.Id ?? 0}));
                NextTarget();

                await Task.Delay(2000);
                _disallowactions = false;
            }
            catch (Exception)
            {
                GetRecord(Chaser).ChaserCount++;
                Room.Broadcast(new SlaughterChangeSlaughterAckMessage(Chaser?.Account.Id ?? 0,
                    new[] {Chaser?.Account.Id ?? 0}));
                NextTarget();
                // ignored
            }
        }

        public void ChaserWin()
        {
            if (_disallowactions)
                return;

            GetRecord(Chaser).Wins++;
            Room.Broadcast(new SlaughterSLRoundWinAckMessage());

            _waitingNextChaser = true;
        }

        public void ChaserLose()
        {
            if (_disallowactions)
                return;

            if (Chaser != null)
            {
                foreach (var plr in Room.TeamManager.PlayersPlaying.Where(plr => plr != Chaser))
                {
                    GetRecord(plr).Survived++;
                    plr.Session.SendAsync(new SlaughterRoundWinAckMessage());
                }
            }
            
            _waitingNextChaser = true;
        }

        private bool CanStartGame()
        {
            if (!StateMachine.IsInState(GameRuleState.Waiting))
                return false;
            if (Room.Options.IsFriendly)
                return true;
            var teams = Room.TeamManager.Values.ToArray();
            if (Room.Players.Where(plr => plr.Value.RoomInfo.IsReady).ToArray().Length + 1 < PlayersNeededToStart)
                return false;
            // Is atleast one player per team ready?
            return teams.All(team => team.Players.Any(plr => plr.RoomInfo.IsReady || Room.Master == plr));
        }
        

        private bool ArePlayersAlive()
        {
            if(Room.TeamManager.PlayersPlaying.Any(plr => plr.RoomInfo.State == PlayerState.Alive && plr != Chaser))
                return true;
            return false;
        }
        
        private static ChaserPlayerRecord GetRecord(Player plr)
        {
            return (ChaserPlayerRecord) plr.RoomInfo.Stats;
        }
    }

    internal class ChaserBriefing : Briefing
    {
        public ChaserBriefing(GameRuleBase gameRule)
            : base(gameRule)
        {
            Unk7 = new List<int>();
            Unk8 = new List<long>();
            Unk9 = new List<long>();
        }

        public long CurrentChaser { get; set; }
        public long CurrentChaserTarget { get; set; }

        public int Unk3 { get; set; }
        public int Unk4 { get; set; }
        public int Unk5 { get; set; }
        public int Unk6 { get; set; }

        public IList<int> Unk7 { get; set; }
        public IList<long> Unk8 { get; set; }
        public IList<long> Unk9 { get; set; }

        protected override void WriteData(BinaryWriter w, bool isResult)
        {
            base.WriteData(w, isResult);
            var gameRule = (ChaserGameRule) GameRule;
            CurrentChaser = (long) (gameRule.Chaser?.Account.Id ?? 0);
            CurrentChaserTarget = (long) (gameRule.ChaserTarget?.Account.Id ?? 0);
            Unk8 = new List<long>();
            Unk9 = new List<long>();
            Unk8.Add(CurrentChaser);
            Unk9.Add(CurrentChaser);


            Unk6 = 1;
            w.Write(CurrentChaser);
            w.Write(CurrentChaser);
            w.Write(Unk3);
            w.Write(Unk4);
            w.Write(Unk5);
            w.Write(Unk6);
            w.Write(Unk7.Count);
            w.Write(Unk7);
            w.Write(Unk8.Count);
            w.Write(Unk8);
            w.Write(Unk9.Count);
            w.Write(Unk9);
        }
    }

    internal class ChaserPlayerRecord : PlayerRecord
    {
        public ChaserPlayerRecord(Player plr)
            : base(plr)
        {
        }

        public override uint TotalScore => GetTotalScore();

        public uint Unk1 { get; set; }
        public uint Unk2 { get; set; }
        public uint Unk3 { get; set; }
        public uint Unk4 { get; set; }
        public uint BonusKills { get; set; }
        public uint Unk5 { get; set; } //Increases points
        public uint Unk6 { get; set; } //Increases points
        public uint Unk7 { get; set; } //Increases points and did at some point instanced a second chaser
        public uint Unk8 { get; set; } //Increases points
        public uint Wins { get; set; } //Wins
        public uint Survived { get; set; }
        public uint Unk9 { get; set; } //Increases points
        public uint Unk10 { get; set; } //Increases points 
        public uint ChaserCount { get; set; }
        public uint Unk11 { get; set; } //Increases points
        public uint Unk12 { get; set; }
        public uint Unk13 { get; set; }
        public uint Unk14 { get; set; }
        public uint Unk15 { get; set; }
        public uint Unk16 { get; set; }

        public float Unk17 { get; set; }
        public float Unk18 { get; set; }
        public float Unk19 { get; set; }
        public float Unk20 { get; set; }

        public byte Unk21 { get; set; }

        public override void Serialize(BinaryWriter w, bool isResult)
        {
            base.Serialize(w, isResult);

            w.Write(Unk1);
            w.Write(Unk2);
            w.Write(Unk3);
            w.Write(Unk4);
            w.Write(Kills);
            w.Write(BonusKills);
            w.Write(Unk5);
            w.Write(Unk6);
            w.Write(Unk7);
            w.Write(Unk8);
            w.Write(Wins);
            w.Write(Survived);
            w.Write(Unk9);
            w.Write(Unk10);
            w.Write(ChaserCount);
            w.Write(Unk11);
            w.Write(Unk12);
            w.Write(Unk13);
            w.Write(Unk14);
            w.Write(Unk15);
            w.Write(Unk16);

            w.Write(Unk17);
            w.Write(Unk18);
            w.Write(Unk19);
            w.Write(Unk20);

            w.Write(Unk21);
        }

        public override void Reset()
        {
            base.Reset();

            Unk1 = 0;
            Unk2 = 0;
            Unk3 = 0;
            Unk4 = 0;
            Kills = 0;
            BonusKills = 0;
            Unk5 = 0;
            Unk6 = 0;
            Unk7 = 0;
            Unk8 = 0;
            Wins = 0;
            Survived = 0;
            Unk9 = 0;
            Unk10 = 0;
            ChaserCount = 0;
            Unk11 = 0;
            Unk12 = 0;
            Unk13 = 0;
            Unk14 = 0;
            Unk15 = 0;
            Unk16 = 0;
            Unk17 = 0;
            Unk18 = 0;
            Unk19 = 0;
            Unk20 = 0;
            Unk21 = 0;
        }

        private uint GetTotalScore()
        {
            return Kills * 2 +
                   BonusKills * 4 +
                   Wins * 5 +
                   Survived * 10;
        }
    }
}
