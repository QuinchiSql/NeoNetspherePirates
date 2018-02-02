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
namespace Netsphere
{
    internal class ClubManager : IReadOnlyCollection<Club>
    {
        private readonly ConcurrentDictionary<uint, Club> _clubs = new ConcurrentDictionary<uint, Club>();

        public Club this[uint id] => GetClub(id);

        public Club GetClub(uint id)
        {
            Club Club;
            _clubs.TryGetValue(id, out Club);
            return Club;
        }

        public Club GetClubByAccount(ulong id)
        {
            Club Club = _clubs.Values.Where(c => c.Players.Any(p => p.Value.AccountId == id)).FirstOrDefault();
            return Club;
        }

        public ClubManager(IEnumerable<DBClubInfoDto> ClubInfos)
        {
            foreach (var InfoDto in ClubInfos)
            {
                var Club = new Club(InfoDto.ClubDto, InfoDto.PlayerDto);
                _clubs.TryAdd(InfoDto.ClubDto.Id, Club);
            }
        }

        
        #region IReadOnlyCollection

        public int Count => _clubs.Count;

        public IEnumerator<Club> GetEnumerator()
        {
            return _clubs.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
