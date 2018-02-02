using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BlubLib.Collections.Concurrent;
using BlubLib.Collections.Generic;
using ProudNetSrc.Serialization.Messages;

namespace ProudNetSrc
{
    public class P2PGroup
    {
        private readonly ConcurrentDictionary<uint, RemotePeer> _members = new ConcurrentDictionary<uint, RemotePeer>();
        private readonly ProudServer _server;

        internal P2PGroup(ProudServer server, bool allowDirectP2P)
        {
            _server = server;
            HostId = _server.Configuration.P2PGroupHostIdFactory.New();
            AllowDirectP2P = allowDirectP2P;
        }

        public uint HostId { get; }
        public bool AllowDirectP2P { get; }
        public IReadOnlyDictionary<uint, RemotePeer> Members => _members;

        public void Join(uint hostId)
        {
            var encrypted = _server.Configuration.EnableP2PEncryptedMessaging;
            Crypt crypt = null;
            if (encrypted)
                crypt = new Crypt(_server.Configuration.EncryptedMessageKeyLength,
                    _server.Configuration.FastEncryptedMessageKeyLength);

            var session = _server.Sessions[hostId];
            var remotePeer = new RemotePeer(this, session, crypt);
            if (!_members.TryAdd(hostId, remotePeer))
                throw new ProudException($"Member {hostId} is already in P2PGroup {HostId}");

            session.P2PGroup = this;

            var state0 = new P2PConnectionState(remotePeer);
            remotePeer.ConnectionStates[remotePeer.HostId] = state0;
            if (encrypted)
                session.SendAsync(new P2PGroup_MemberJoinMessage(HostId, hostId, state0.EventId, crypt.AES.Key,
                    crypt.RC4.Key, AllowDirectP2P));
            else
                session.SendAsync(
                    new P2PGroup_MemberJoin_UnencryptedMessage(HostId, hostId, state0.EventId, AllowDirectP2P));

            foreach (var member in _members.Values.Where(member => member.HostId != hostId).Cast<RemotePeer>())
            {
                var memberSession = _server.Sessions[member.HostId];

                var stateA = new P2PConnectionState(member);
                var stateB = new P2PConnectionState(remotePeer);

                remotePeer.ConnectionStates[member.HostId] = stateA;
                member.ConnectionStates[remotePeer.HostId] = stateB;
                if (encrypted)
                {
                    memberSession.SendAsync(new P2PGroup_MemberJoinMessage(HostId, hostId, stateB.EventId,
                        crypt.AES.Key, crypt.RC4.Key, AllowDirectP2P));
                    session.SendAsync(new P2PGroup_MemberJoinMessage(HostId, member.HostId, stateA.EventId,
                        crypt.AES.Key, member.Crypt.RC4.Key, AllowDirectP2P));
                }
                else
                {
                    memberSession.SendAsync(
                        new P2PGroup_MemberJoin_UnencryptedMessage(HostId, hostId, stateB.EventId, AllowDirectP2P));
                    session.SendAsync(new P2PGroup_MemberJoin_UnencryptedMessage(HostId, member.HostId, stateA.EventId,
                        AllowDirectP2P));
                }
            }
        }

        public void Leave(uint hostId)
        {
            RemotePeer memberToLeave;
            if (!_members.TryRemove(hostId, out memberToLeave))
                return;

            var session = memberToLeave.Session;
            session.P2PGroup = null;
            session.SendAsync(new P2PGroup_MemberLeaveMessage(hostId, HostId));

            foreach (var member in _members.Values.Where(entry => entry.HostId != hostId))
            {
                var memberSession = _server.Sessions.GetValueOrDefault(member.HostId);
                memberSession?.SendAsync(new P2PGroup_MemberLeaveMessage(hostId, HostId));
                session.SendAsync(new P2PGroup_MemberLeaveMessage(member.HostId, HostId));
                member.ConnectionStates.Remove(hostId);
            }
            if(_members.Count < 1)
                _server.P2PGroupManager.Remove(this);
        }
    }
}
