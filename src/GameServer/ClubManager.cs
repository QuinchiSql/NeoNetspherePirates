using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BlubLib.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace NeoNetsphere
{
    internal class ClubManager : IReadOnlyCollection<Club>
    {
        private readonly ConcurrentDictionary<uint, Club> _clubs = new ConcurrentDictionary<uint, Club>();
        internal readonly AsyncLock _sync = new AsyncLock();

        public ClubManager(IEnumerable<DBClubInfoDto> clubInfos)
        {
            _clubs.Clear();
            foreach (var infoDto in clubInfos)
            {
                var club = new Club(infoDto.ClubDto, infoDto.PlayerDto);
                _clubs.TryAdd(infoDto.ClubDto.Id, club);
            }
        }

        public Club this[uint id] => GetClub(id);

        public Club GetClub(uint id)
        {
            //using (_sync.Lock())
            {
                Club Club;
                _clubs.TryGetValue(id, out Club);
                return Club;
            }
        }

        public Club GetClubByAccount(ulong id)
        {
            //using (_sync.Lock())
            {
                return _clubs.Values.FirstOrDefault(c => c.Players.Any(p => p.Value.AccountId == id));
            }
        }

        public void Remove(Club club)
        {
            //using (_sync.Lock())
            {
                if (club == null)
                    return;
                _clubs.TryRemove(club.Id, out var _);
            }
        }

        public void Add(Club club)
        {
            //using (_sync.Lock())
            {
                _clubs.TryAdd(club.Id, club);
            }
        }

        #region IReadOnlyCollection

        public int Count => _clubs.Count;

        public IEnumerator<Club> GetEnumerator()
        {
            //using (_sync.Lock())
            {
                return _clubs.Values.GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            //using (_sync.Lock())
            {
                return GetEnumerator();
            }
        }

        #endregion
    }
}
