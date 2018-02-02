using System.Collections.Generic;
using BlubLib.Configuration;

namespace NeoNetsphere.Resource
{
    public class MapInfo
    {
        public MapInfo()
        {
            GameRules = new List<GameRule>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public byte MinLevel { get; set; }
        public uint ServerId { get; set; }
        public uint ChannelId { get; set; }
        public byte RespawnType { get; set; }
        public IniFile Config { get; set; }

        public IList<GameRule> GameRules { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
