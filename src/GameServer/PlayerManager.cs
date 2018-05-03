using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BlubLib.Collections.Generic;

namespace NeoNetsphere
{
    internal class PlayerManager : IReadOnlyCollection<Player>
    {
        private readonly IDictionary<ulong, Player> _players = new ConcurrentDictionary<ulong, Player>();
        private readonly object _sync = new object();

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
            lock (_sync)
            {
                _players.TryGetValue(id, out var plr);
                return plr;
            }
        }

        public Player Get(string nick)
        {
            lock (_sync)
            {
                return _players.Values.FirstOrDefault(plr =>
                    plr.Account.Nickname != null &&
                    plr.Account.Nickname.Equals(nick, StringComparison.InvariantCultureIgnoreCase));
            }
        }

        public void Add(Player plr)
        {
            lock (_sync)
            {
                if (!CollectionExtensions.TryAdd(_players, plr.Account.Id, plr))
                    throw new Exception("Player " + plr.Account.Id + " already exists");
            }
        }

        public void Remove(Player plr)
        {
            lock (_sync)
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

            lock (_sync)
            {
                return _players.ContainsKey(id);
            }
        }
    }
}
