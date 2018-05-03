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
            _players.TryGetValue(id, out var plr);
            return plr;
        }

        public Player Get(string nick)
        {
            return _players.Values.FirstOrDefault(plr =>
                plr.Account.Nickname != null &&
                plr.Account.Nickname.Equals(nick, StringComparison.InvariantCultureIgnoreCase));
        }

        public void Add(Player plr)
        {
            if (!CollectionExtensions.TryAdd(_players, plr.Account.Id, plr))
                throw new Exception("Player " + plr.Account.Id + " already exists");
        }

        public void Remove(Player plr)
        {
            if(plr?.Account != null)
                _players.TryRemove(plr.Account.Id, out _);
        }

        public void Remove(ulong id)
        {
        }

        public bool Contains(Player plr)
        {
            return Contains(plr.Account.Id);
        }

        public bool Contains(ulong id)
        {
            return _players.ContainsKey(id);
        }
    }
}
