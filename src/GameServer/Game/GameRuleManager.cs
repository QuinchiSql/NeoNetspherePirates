using System;
using NeoNetsphere;
using NeoNetsphere.Resource;
using Netsphere.Game.GameRules;

// ReSharper disable once CheckNamespace
namespace Netsphere.Game.Systems
{
    internal class GameRuleManager
    {
        private GameRuleBase _gameRule;
        private MapInfo _mapInfo;

        public GameRuleManager(Room room)
        {
            Room = room;
        }

        public Room Room { get; }

        public GameRuleBase GameRule
        {
            get => _gameRule;
            set
            {
                if (value != _gameRule)
                {
                    _gameRule?.Cleanup();
                    _gameRule = value;
                    _gameRule?.Initialize();
                    OnGameRuleChanged();
                }
            }
        }

        public MapInfo MapInfo
        {
            get => _mapInfo;
            set
            {
                if (value != _mapInfo)
                {
                    _mapInfo = value;
                    OnMapInfoChanged();
                }
            }
        }

        public event EventHandler GameRuleChanged;
        public event EventHandler MapInfoChanged;

        protected virtual void OnGameRuleChanged()
        {
            GameRuleChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnMapInfoChanged()
        {
            MapInfoChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Update(TimeSpan delta)
        {
            GameRule.Update(delta);
        }
    }
}
