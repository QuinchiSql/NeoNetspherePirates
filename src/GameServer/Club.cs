using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Dapper.FastCrud;
using ExpressMapper.Extensions;
using NeoNetsphere.Database.Auth;
using NeoNetsphere.Database.Game;
using NeoNetsphere.Network;
using NeoNetsphere.Network.Data.Game;
using NeoNetsphere.Network.Message.Chat;
using NeoNetsphere.Network.Message.Club;
using NeoNetsphere.Network.Message.Game;
using Serilog;
using Serilog.Core;

namespace NeoNetsphere
{
    internal class DBClubInfoDto
    {
        public ClubDto ClubDto { get; set; }
        public ClubPlayerInfo[] PlayerDto { get; set; }
    }

    internal class ClubPlayerInfo
    {
        public ulong AccountId { get; set; }
        public ClubState State { get; set; }
        public bool IsMod { get; set; }

        public AccountDto account { get; set; }
    }
    
    internal class Club
    {
        // ReSharper disable once InconsistentNaming
        private static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(Club));
        private readonly Dictionary<ulong, ClubPlayerInfo> _players = new Dictionary<ulong, ClubPlayerInfo>();
        internal bool NeedsToSave { get; set; }

        public Dictionary<ulong, ClubPlayerInfo> Players => _players;

        public ClubPlayerInfo this[ulong id] => _players[id];
        public int Count => _players.Count;

        public static void LogOn(Player plr)
        {
            plr.Club?.Broadcast(new ClubMemberLoginStateAckMessage(1, plr.Account.Id));
            plr.Club?.Broadcast(new ClubSystemMessageMessage(plr.Account.Id, $"<Chat Key =\"1\" Cnt =\"2\" Param1=\"{plr.Account.Nickname}\" Param2=\"1\"  />"));
        }

        public static void LogOff(Player plr)
        {
            plr.Club?.Broadcast(new ClubMemberLoginStateAckMessage(0, plr.Account.Id));
            plr.Club?.Broadcast(new ClubSystemMessageMessage(plr.Account.Id, $"<Chat Key =\"1\" Cnt =\"2\" Param1=\"{plr.Account.Nickname}\" Param2=\"2\"  />"));
        }

        public void Broadcast(IClubMessage message)
        {
            foreach (var member in GameServer.Instance.PlayerManager.Where(x => x.Club?.Id == Id))
            {
                member.Session?.SendAsync(message);
            }
        }

        public void Broadcast(IGameMessage message)
        {
            foreach (var member in GameServer.Instance.PlayerManager.Where(x => x.Club?.Id == Id))
            {
                member.Session?.SendAsync(message);
            }
        }

        public void Broadcast(IChatMessage message)
        {
            foreach (var member in GameServer.Instance.PlayerManager.Where(x => x.Club?.Id == Id))
            {
                member.ChatSession?.SendAsync(message);
            }
        }

        public Club()
        {
        }

        public Club(ClubDto dto, ClubPlayerInfo[] Player)
        {
            _players = Player.ToDictionary(playerinfo => playerinfo.AccountId);
            Id = dto.Id;
            Clan_Name = dto.Name;
            Clan_Icon = dto.Icon;

            Logger.Information("New Club: {name} {type} {playercount}", Clan_Name, Clan_Icon, Count);
        }

        public uint Id { get; set; }

        public string Clan_Icon { get; set; } = "1-1-1";

        public string Clan_Name { get; set; } = "CC_LOVERS_C2";
    }
}
