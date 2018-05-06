using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NeoNetsphere.Database.Auth;
using NeoNetsphere.Database.Game;
using NeoNetsphere.Network;
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

        public AccountDto Account { get; set; }
    }

    internal class Club
    {
        // ReSharper disable once InconsistentNaming
        private static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(Club));

        public Club(ClubDto dto, IEnumerable<ClubPlayerInfo> player)
        {
            Players = new ConcurrentDictionary<ulong, ClubPlayerInfo>(player.ToDictionary(playerinfo =>
                playerinfo.AccountId));
            Id = dto.Id;
            ClanName = dto.Name;
            ClanIcon = dto.Icon;
            Logger.Information("New Club: {name} {type} {playercount}", ClanName, ClanIcon, Count);
        }

        public ConcurrentDictionary<ulong, ClubPlayerInfo> Players { get; }

        public ClubPlayerInfo this[ulong id] => GetPlayer(id);

        public int Count => Players.Count;

        public uint Id { get; }

        public string ClanIcon { get; } = "1-1-1";

        public string ClanName { get; } = "CC_LOVERS_C2";

        public ClubPlayerInfo GetPlayer(ulong id)
        {
            Players.TryGetValue(id, out var returnval);
            return returnval;
        }

        public static void LogOn(Player plr)
        {
            plr.Club?.Broadcast(new ClubMemberLoginStateAckMessage(1, plr.Account.Id));
            //plr.Club?.Broadcast(new ClubSystemMessageMessage(plr.Account.Id, $"<Chat Key =\"1\" Cnt =\"2\" Param1=\"{plr.Account.Nickname}\" Param2=\"1\"  />"));
        }

        public static void LogOff(Player plr)
        {
            plr.Club?.Broadcast(new ClubMemberLoginStateAckMessage(0, plr.Account.Id));
            //plr.Club?.Broadcast(new ClubSystemMessageMessage(plr.Account.Id, $"<Chat Key =\"1\" Cnt =\"2\" Param1=\"{plr.Account.Nickname}\" Param2=\"2\"  />"));
        }

        public void Broadcast(IClubMessage message)
        {
            foreach (var member in GameServer.Instance.PlayerManager.Where(x => x.Club?.Id == Id))
                member.Session?.SendAsync(message);
        }

        public void Broadcast(IGameMessage message)
        {
            foreach (var member in GameServer.Instance.PlayerManager.Where(x => x.Club?.Id == Id))
                member.Session?.SendAsync(message);
        }

        public void Broadcast(IChatMessage message)
        {
            foreach (var member in GameServer.Instance.PlayerManager.Where(x => x.Club?.Id == Id))
                member.ChatSession?.SendAsync(message);
        }
    }
}
