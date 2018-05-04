using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using NeoNetsphere;
using NeoNetsphere.Game.GameRules;
using Netsphere.Game.GameRules;

// ReSharper disable once CheckNamespace
namespace Netsphere.Game
{
    internal class GameRuleFactory
    {
        private readonly IDictionary<GameRule, Func<Room, GameRuleBase>> _gameRules =
            new ConcurrentDictionary<GameRule, Func<Room, GameRuleBase>>();

        public GameRuleFactory()
        {
            Add(GameRule.Touchdown, room => new TouchdownGameRule(room));
            Add(GameRule.Deathmatch, room => new DeathmatchGameRule(room));
            Add(GameRule.Chaser, room => new ChaserGameRule(room));
            Add(GameRule.BattleRoyal, room => new BattleRoyalGameRule(room));

            //Event
            Add(GameRule.SnowballFight, room => new SnowballFightGameRule(room));
            Add(GameRule.PassTouchdown, room => new PassTouchdownGameRule(room));
            //Nearly finished
            Add(GameRule.Practice, room => new PracticeGameRule(room));

            //Not completely supported yet
            Add(GameRule.Horde, room => new ConquestGameRule(room));
            Add(GameRule.CombatTrainingTD, room => new TouchdownTrainingGameRule(room));
            Add(GameRule.CombatTrainingDM, room => new DeathmatchTrainingGameRule(room)); 
            //Add(GameRule.Siege, room => new SiegeGameRule(room));
            //Add(GameRule.Captain, room => new CaptainGameRule(room));
            //Add(GameRule.Warfare, room => new WarfareGameRule(room));
            //Add(GameRule.Arena, room => new ArenaGameRule(room));
        }

        public void Add(GameRule gameRule, Func<Room, GameRuleBase> gameRuleFactory)
        {
            if (!_gameRules.TryAdd(gameRule, gameRuleFactory))
                throw new Exception($"GameRule {gameRule} already registered");
        }

        public void Remove(GameRuleBase gameRule)
        {
            _gameRules.Remove(gameRule.GameRule);
        }

        public GameRuleBase Get(GameRule gameRule, Room room)
        {
            if (!_gameRules.TryGetValue(gameRule, out var gameRuleFactory))
                throw new Exception($"GameRule {gameRule} not registered");

            return gameRuleFactory(room);
        }

        public bool Contains(GameRule gameRule)
        {
            return _gameRules.ContainsKey(gameRule);
        }
    }
}
