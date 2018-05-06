using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BlubLib.Collections.Generic;
using BlubLib.Threading.Tasks;

namespace NeoNetsphere
{
    internal class PlayerManager : IReadOnlyCollection<Player>
    {
        private readonly IDictionary<ulong, Player> _players = new ConcurrentDictionary<ulong, Player>();
        internal readonly AsyncLock _sync = new AsyncLock();

        public Player this[ulong id] => Get(id);

        public int Count => _players.Count;

        public IEnumerator<Player> GetEnumerator()
        {
            return _players.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Player Get(ulong id)
        {
            //using (_sync.Lock())
            {
                _players.TryGetValue(id, out var plr);
                return plr;
            }
        }

        public Player Get(string nick)
        {
            //using (_sync.Lock())
            {
                return _players.Values.FirstOrDefault(plr =>
                    plr.Account.Nickname != null &&
                    plr.Account.Nickname.Equals(nick, StringComparison.InvariantCultureIgnoreCase));
            }
        }

        public void Add(Player plr)
        {
            //using (_sync.Lock())
            {
                if (!CollectionExtensions.TryAdd(_players, plr.Account.Id, plr))
                    throw new Exception("Player " + plr.Account.Id + " already exists");
            }
        }

        public void Remove(Player plr)
        {
            //using (_sync.Lock())
            {
                if (plr?.Account != null)
                    _players.TryRemove(plr.Account.Id, out _);
            }
        }

        public bool Contains(Player plr)
        {
            return Contains(plr.Account.Id);
        }

        public bool Contains(ulong id)
        {
            //using (_sync.Lock())
            {
                return _players.ContainsKey(id);
            }
        }
    }
}
