﻿using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using BlubLib.Collections.Generic;

namespace ProudNetSrc
{
    public class P2PGroupManager : IReadOnlyDictionary<uint, P2PGroup>
    {
        private readonly ConcurrentDictionary<uint, P2PGroup> _groups = new ConcurrentDictionary<uint, P2PGroup>();
        private readonly object _sync = new object();
        private readonly ProudServer _server;

        internal P2PGroupManager(ProudServer server)
        {
            _server = server;
        }

        public P2PGroup Create(bool allowDirectP2P)
        {
            lock (_sync)
            {
                var group = new P2PGroup(_server, allowDirectP2P);
                _groups.TryAdd(group.HostId, group);
                _server.Configuration.Logger?.Debug("Created P2PGroup({HostId}) directP2P={AllowDirectP2P}",
                    group.HostId, allowDirectP2P);
                return group;
            }
        }

        public void Remove(uint groupHostId)
        {
            lock (_sync)
            {
                if (_groups.TryRemove(groupHostId, out var group))
                {
                    foreach (var member in group.Members)
                        group.Leave(member.Key);

                    _server.Configuration.HostIdFactory.Free(groupHostId);
                }

                _server.Configuration.Logger?.Debug("Removed P2PGroup({HostId})", group.HostId);
            }
        }

        public void Remove(P2PGroup group)
        {
            Remove(group.HostId);
        }

        #region IReadOnlyDictionary

        public int Count => _groups.Count;
        public IEnumerable<uint> Keys => _groups.Keys;
        public IEnumerable<P2PGroup> Values => _groups.Values;

        public bool ContainsKey(uint key) => _groups.ContainsKey(key);
        public bool TryGetValue(uint key, out P2PGroup value) => _groups.TryGetValue(key, out value);
        public P2PGroup this[uint key] => this.GetValueOrDefault(key);
        public IEnumerator<KeyValuePair<uint, P2PGroup>> GetEnumerator() => _groups.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}
