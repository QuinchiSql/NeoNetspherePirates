using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using BlubLib.Collections.Generic;
using Dapper.FastCrud;
using MySqlX.XDevAPI.CRUD;
using NeoNetsphere;
using NeoNetsphere.Database.Game;
using NeoNetsphere.Network;

// ReSharper disable once CheckNamespace
namespace NeoNetsphere
{
    internal class ClubManager : IReadOnlyCollection<Club>
    {
        private readonly ConcurrentDictionary<uint, Club> _clubs = new ConcurrentDictionary<uint, Club>();
        private readonly object _sync = new object();

        public Club this[uint id] => GetClub(id);

        public Club GetClub(uint id)
        {
            lock (_sync)
            {
                Club Club;
                _clubs.TryGetValue(id, out Club);
                return Club;
            }
        }

        public Club GetClubByAccount(ulong id)
        {
            lock (_sync)
            {
                return _clubs.Values.FirstOrDefault(c => c.Players.Any(p => p.Value.AccountId == id));
            }
        }

        public ClubManager(IEnumerable<DBClubInfoDto> clubInfos)
        {
            _clubs.Clear();
            foreach (var infoDto in clubInfos)
            {
                var club = new Club(infoDto.ClubDto, infoDto.PlayerDto);
                _clubs.TryAdd(infoDto.ClubDto.Id, club);
            }
        }

        public void Remove(Club club)
        {
            lock (_sync)
            {
                if (club == null)
                    return;
                _clubs.TryRemove(club.Id, out var _);
            }
        }

        public void Add(Club club)
        {
            lock (_sync)
            {
                club.NeedsToSave = true;
                _clubs.TryAdd(club.Id, club);
            }
        }

        #region IReadOnlyCollection

        public int Count => _clubs.Count;

        public IEnumerator<Club> GetEnumerator()
        {
            lock (_sync)
            {
                return _clubs.Values.GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (_sync)
            {
                return GetEnumerator();
            }
        }

        #endregion
    }
}
