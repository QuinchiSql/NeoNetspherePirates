using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using BlubLib.Collections.Generic;
using Serilog.Core;
using Serilog;

namespace ProudNetSrc
{
    public class P2PGroupManager : IReadOnlyDictionary<uint, P2PGroup>
    {
        private readonly ConcurrentDictionary<uint, P2PGroup> _groups = new ConcurrentDictionary<uint, P2PGroup>();
        private readonly ProudServer _server;
        private static readonly ILogger Logger =
            Log.ForContext("SourceContext", nameof(P2PGroupManager));

        internal P2PGroupManager(ProudServer server)
        {
            _server = server;
        }

        public P2PGroup Create(bool allowDirectP2P)
        {
            var group = new P2PGroup(_server, allowDirectP2P);
            _groups.TryAdd(group.HostId, group);
            Logger.Information("Created P2PGroup {id}", group.HostId);
            return group;
        }

        public void Remove(uint groupHostId)
        {
            P2PGroup group;
            if (_groups.TryRemove(groupHostId, out group))
            {
                foreach (var member in group.Members)
                    group.Leave(member.Key);

                _server.Configuration.P2PGroupHostIdFactory.Free(groupHostId);
            }
            Logger.Information("Removed P2PGroup {id}", group.HostId);
        }

        public void Remove(P2PGroup group)
        {
            Remove(group.HostId);
        }

        #region IReadOnlyDictionary

        public int Count => _groups.Count;
        public IEnumerable<uint> Keys => _groups.Keys;
        public IEnumerable<P2PGroup> Values => _groups.Values;

        public bool ContainsKey(uint key)
        {
            return _groups.ContainsKey(key);
        }

        public bool TryGetValue(uint key, out P2PGroup value)
        {
            return _groups.TryGetValue(key, out value);
        }

        public P2PGroup this[uint key] => this.GetValueOrDefault(key);

        public IEnumerator<KeyValuePair<uint, P2PGroup>> GetEnumerator()
        {
            return _groups.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
