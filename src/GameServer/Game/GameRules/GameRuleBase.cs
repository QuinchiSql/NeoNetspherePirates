using System;
using System.Collections.Generic;
using System.Linq;
using NeoNetsphere;
using NeoNetsphere.Network.Data.GameRule;
using NeoNetsphere.Network.Message.GameRule;
using Stateless;

// ReSharper disable once CheckNamespace

namespace Netsphere.Game.GameRules
{
    internal abstract class GameRuleBase
    {
        private static readonly TimeSpan PreHalfTimeWaitTime = TimeSpan.FromSeconds(9);
        private static readonly TimeSpan PreResultWaitTime = TimeSpan.FromSeconds(9);
        private static readonly TimeSpan HalfTimeWaitTime = TimeSpan.FromSeconds(24);
        private static readonly TimeSpan ResultWaitTime = TimeSpan.FromSeconds(14);
        public TimeSpan GameLoadMaxTime = TimeSpan.FromSeconds(10);

        protected GameRuleBase(Room room)
        {
            Room = room;
            StateMachine = new StateMachine<GameRuleState, GameRuleStateTrigger>(GameRuleState.Waiting);
            StateMachine.OnTransitioned(StateMachine_OnTransition);
        }

        public abstract GameRule GameRule { get; }
        public abstract bool CountMatch { get; }
        public Room Room { get; }
        public abstract Briefing Briefing { get; }
        public StateMachine<GameRuleState, GameRuleStateTrigger> StateMachine { get; }

        public TimeSpan GameTime { get; private set; }
        public TimeSpan RoundTime { get; private set; }

        public byte GameStartState { get; set; }
        public int GameStartTimeMs { get; set; } = 3500;
        public TimeSpan GameStartTime { get; set; }

        public virtual void Initialize()
        {
        }

        public virtual void Cleanup()
        {
        }

        public virtual void Reload()
        {
        }

        public virtual void UpdateTime(Player plr)
        {
            plr?.Session?.SendAsync(new GameRefreshGameRuleInfoAckMessage(Room.GameState, Room.SubGameState,
                (int) (Room.RoundTime.TotalMilliseconds + 100)));
        }

        public virtual async void UpdateTime()
        {
            foreach (var member in Room.Players.Values)
                if (member.RoomInfo.HasLoaded)
                    await member.Session.SendAsync(new GameRefreshGameRuleInfoAckMessage(Room.GameState,
                        Room.SubGameState, (int) (Room.RoundTime.TotalMilliseconds + 100)));
        }

        public virtual async void IntrudeCompleted(Player plr)
        {
            UpdateTime(plr);
            await plr.Session.SendAsync(new RoomGameStartAckMessage());
        }

        public virtual async void Update(TimeSpan delta)
        {
            RoundTime += delta;
            Room.RoundTime = RoundTime;

            #region PrepareGame

            if (GameStartState == 1)
            {
                if (Room.Players.Values.Count(x => x.RoomInfo.HasLoaded)
                    >= Room.Players.Values.Count(x => x.RoomInfo.IsReady || x == Room.Master))
                {
                    GameStartTime = Room.RoundTime;

                    if (GameRule == GameRule.Chaser || GameRule == GameRule.Practice ||
                        GameRule == GameRule.CombatTrainingDM || GameRule == GameRule.CombatTrainingTD)
                    {
                        GameStartState = 3;
                    }
                    else
                    {
                        GameStartState = 2;
                        if (Room.RoundTime > GameLoadMaxTime)
                            foreach (var player in Room.TeamManager.Players.Where(x =>
                                (x.RoomInfo.IsReady || Room.Master == x) && !x.RoomInfo.HasLoaded))
                                Room.Leave(player, RoomLeaveReason.AFK);
                        foreach (var member in Room.Players.Values)
                            if (member.RoomInfo.HasLoaded)
                            {
                                await member.Session.SendAsync(new RoomGamePlayCountDownAckMessage(GameStartTimeMs));
                                await member.Session.SendAsync(new GameRefreshGameRuleInfoAckMessage(Room.GameState,
                                    GameTimeState.StartGameCounter, (int) Room.Options.TimeLimit.TotalMilliseconds));
                            }
                    }
                }
            }
            else if (GameStartState == 2)
            {
                if ((Room.RoundTime - GameStartTime).TotalMilliseconds > GameStartTimeMs + 500)
                    GameStartState = 3;
            }
            else if (GameStartState == 3)
            {
                RoundTime = TimeSpan.Zero;
                GameStartState = 4;
                Room.isPreparing = false;
                if (StateMachine.CanFire(GameRuleStateTrigger.StartGame))
                    StateMachine.Fire(GameRuleStateTrigger.StartGame);
            }

            #endregion

            #region Playing

            if (StateMachine.IsInState(GameRuleState.Playing))
            {
                GameTime += delta;

                foreach (var plr in Room.TeamManager.PlayersPlaying)
                {
                    plr.RoomInfo.PlayTime += delta;
                    plr.RoomInfo.CharacterPlayTime[plr.CharacterManager.CurrentSlot] += delta;
                }
            }

            #endregion

            #region HalfTime

            if (StateMachine.IsInState(GameRuleState.EnteringHalfTime))
                if (RoundTime >= PreHalfTimeWaitTime)
                {
                    RoundTime = TimeSpan.Zero;
                    StateMachine.Fire(GameRuleStateTrigger.StartHalfTime);
                }
                else
                {
                    foreach (var plrx in Room.TeamManager.Players.Where(plr => plr.RoomInfo.State != PlayerState.Lobby))
                        await plrx.Session.SendAsync(new GameEventMessageAckMessage(GameEventMessage.HalfTimeIn, 2, 0,
                            0,
                            ((int) (PreHalfTimeWaitTime - RoundTime).TotalSeconds + 1).ToString()));
                }

            if (StateMachine.IsInState(GameRuleState.HalfTime))
                if (RoundTime >= HalfTimeWaitTime)
                    StateMachine.Fire(GameRuleStateTrigger.StartSecondHalf);

            #endregion

            #region Result

            if (StateMachine.IsInState(GameRuleState.EnteringResult))
                if (RoundTime >= PreResultWaitTime)
                {
                    RoundTime = TimeSpan.Zero;
                    StateMachine.Fire(GameRuleStateTrigger.StartResult);
                }
                else
                {
                    foreach (var plrx in Room.TeamManager.Players.Where(plr => plr.RoomInfo.State != PlayerState.Lobby))
                        await plrx.Session.SendAsync(new GameEventMessageAckMessage(GameEventMessage.ResultIn, 3, 0, 0,
                            (int) (PreResultWaitTime - RoundTime).TotalSeconds + 1 + " second(s)"));
                }

            if (!StateMachine.IsInState(GameRuleState.Result)) return;
            if (RoundTime >= ResultWaitTime) StateMachine.Fire(GameRuleStateTrigger.EndGame);

            #endregion
        }

        public abstract PlayerRecord GetPlayerRecord(Player plr);

        private async void StateMachine_OnTransition(
            StateMachine<GameRuleState, GameRuleStateTrigger>.Transition transition)
        {
            RoundTime = TimeSpan.Zero;

            switch (transition.Trigger)
            {
                case GameRuleStateTrigger.StartPrepare:
                    Room.isPreparing = true;

                    foreach (var plr in Room.Players.Values.Where(plr =>
                        plr.RoomInfo.IsReady || Room.Master == plr ||
                        plr.RoomInfo.Mode == PlayerGameMode.Spectate))
                    {
                        await plr.Session.SendAsync(new RoomGameLoadingAckMessage());
                        await plr.Session.SendAsync(new RoomBeginRoundAckMessage());
                        plr.RoomInfo.State = PlayerState.Waiting;
                    }

                    GameStartState = 1;
                    Room.GameState = GameState.Loading;
                    Room.Broadcast(new GameChangeStateAckMessage(Room.GameState));
                    break;
            }

            switch (transition.Destination)
            {
                case GameRuleState.FullGame:
                    GameTime = TimeSpan.Zero;
                    Room.hasStarted = true;
                    Room.GameState = GameState.Playing;

                    foreach (var team in Room.TeamManager.Values)
                        team.Score = 0;
                    foreach (var plr in Room.TeamManager.Values.SelectMany(team =>
                        team.Values.Where(plr => plr.RoomInfo.HasLoaded)))
                    {
                        await plr.Session.SendAsync(new RoomGameStartAckMessage());
                        plr.RoomInfo.State = plr.RoomInfo.Mode == PlayerGameMode.Spectate
                            ? PlayerState.Spectating
                            : PlayerState.Alive;
                    }

                    Room.Broadcast(new GameChangeStateAckMessage(Room.GameState));
                    break;
                case GameRuleState.FirstHalf:
                    GameTime = TimeSpan.Zero;
                    Room.hasStarted = true;
                    Room.GameState = GameState.Playing;
                    Room.SubGameState = GameTimeState.FirstHalf;

                    UpdateTime();
                    foreach (var team in Room.TeamManager.Values)
                        team.Score = 0;
                    foreach (var plr in Room.TeamManager.Values.SelectMany(team =>
                        team.Values.Where(plr => plr.RoomInfo.HasLoaded)))
                    {
                        await plr.Session.SendAsync(new RoomGameStartAckMessage());
                        plr.RoomInfo.State = plr.RoomInfo.Mode == PlayerGameMode.Spectate
                            ? PlayerState.Spectating
                            : PlayerState.Alive;
                    }

                    Room.Broadcast(new GameChangeStateAckMessage(Room.GameState));
                    break;
                case GameRuleState.HalfTime:
                    foreach (var member in Room.TeamManager.PlayersPlaying)
                        member.RoomInfo.State = PlayerState.Waiting;

                    Room.SubGameState = GameTimeState.HalfTime;
                    Room.Broadcast(new GameChangeSubStateAckMessage(Room.SubGameState));
                    break;
                case GameRuleState.SecondHalf:
                    foreach (var member in Room.TeamManager.PlayersPlaying)
                        member.RoomInfo.State = PlayerState.Alive;

                    Room.SubGameState = GameTimeState.SecondHalf;
                    Room.Broadcast(new GameChangeSubStateAckMessage(Room.SubGameState));
                    break;
                case GameRuleState.Result:
                    foreach (var plr in Room.TeamManager.Players.Where(plr => plr.RoomInfo.State != PlayerState.Lobby))
                        plr.RoomInfo.State = PlayerState.Waiting;
                    var winners = new List<Player>();
                    foreach (var plr in Room.GameRuleManager.GameRule.Briefing.GetWinnerTeam().Values)
                    {
                        if (CountMatch)
                            plr.TotalWins++;
                        winners.Add(plr);
                    }

                    if (CountMatch)
                        foreach (var plr in Room.TeamManager.PlayersPlaying)
                        {
                            if (!winners.Contains(plr))
                                plr.TotalLosses++;

                            foreach (var @char in plr.CharacterManager)
                            {
                                var loss = (int) (plr.RoomInfo.CharacterPlayTime[@char.Slot].TotalMinutes *
                                                  Config.Instance.Game.DurabilityLossPerMinute +
                                                  plr.RoomInfo.Stats.Deaths *
                                                  Config.Instance.Game.DurabilityLossPerDeath);

                                foreach (var item in @char.Weapons.GetItems()
                                    .Where(item => item != null && item.Durability != -1))
                                    await item.LoseDurabilityAsync(loss);
                                foreach (var item in @char.Costumes.GetItems()
                                    .Where(item => item != null && item.Durability != -1))
                                    await item.LoseDurabilityAsync(loss);
                                foreach (var item in @char.Skills.GetItems()
                                    .Where(item => item != null && item.Durability != -1))
                                    await item.LoseDurabilityAsync(loss);
                            }
                        }

                    Room.hasStarted = false;
                    Room.GameState = GameState.Result;
                    Room.SubGameState = GameTimeState.None;
                    Room.Broadcast(new GameChangeStateAckMessage(Room.GameState));
                    Room.BroadcastBriefing(true);
                    break;
                case GameRuleState.Waiting:
                    GameTime = TimeSpan.Zero;
                    foreach (var team in Room.TeamManager.Values)
                        team.Score = 0;
                    foreach (var plr in Room.TeamManager.Players)
                    {
                        plr.RoomInfo.Reset();
                        plr.RoomInfo.State = PlayerState.Lobby;
                    }

                    Room.hasStarted = false;
                    Room.GameState = GameState.Waiting;
                    Room.SubGameState = GameTimeState.None;
                    Room.Broadcast(new GameChangeStateAckMessage(Room.GameState));
                    Room.BroadcastBriefing();
                    break;
            }
        }

        #region Scores

        public virtual void Respawn(Player victim)
        {
            victim.RoomInfo.State = PlayerState.Dead;
            victim.Session.SendAsync(new InGamePlayerResponseOfDeathAckMessage());
        }

        public virtual void OnScoreKill(Player killer, Player assist, Player target, AttackAttribute attackAttribute,
            LongPeerId scoreTarget = null, LongPeerId scoreKiller = null, LongPeerId scoreAssist = null)
        {
            killer.RoomInfo.Stats.Kills++;
            target.RoomInfo.Stats.Deaths++;

            Respawn(target);
            if (assist != null)
            {
                assist.RoomInfo.Stats.KillAssists++;

                Room.Broadcast(
                    new ScoreKillAssistAckMessage(new ScoreAssistDto(killer.RoomInfo.PeerId, assist.RoomInfo.PeerId,
                        target.RoomInfo.PeerId, attackAttribute)));
            }
            else
            {
                Room.Broadcast(
                    new ScoreKillAckMessage(new ScoreDto(killer.RoomInfo.PeerId, target.RoomInfo.PeerId,
                        attackAttribute)));
            }
        }

        public virtual void OnScoreTeamKill(Player killer, Player target, AttackAttribute attackAttribute,
            LongPeerId scoreKiller = null, LongPeerId scoreTarget = null)
        {
            target.RoomInfo.Stats.Deaths++;

            Respawn(target);
            Room.Broadcast(
                new ScoreTeamKillAckMessage(new Score2Dto(killer.RoomInfo.PeerId, target.RoomInfo.PeerId,
                    attackAttribute)));
        }

        public virtual void OnScoreHeal(Player plr, LongPeerId scoreTarget = null)
        {
            Room.Broadcast(new ScoreHealAssistAckMessage(plr.RoomInfo.PeerId));
        }

        public virtual void OnScoreSuicide(Player plr, LongPeerId scoreTarget = null)
        {
            Respawn(plr);
            plr.RoomInfo.Stats.Deaths++;
            Room.Broadcast(new ScoreSuicideAckMessage(plr.RoomInfo.PeerId, AttackAttribute.KillOneSelf));
        }

        #endregion
    }
}
