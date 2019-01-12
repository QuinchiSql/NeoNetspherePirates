﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BlubLib.Collections.Concurrent;
using BlubLib.Collections.Generic;
using BlubLib.Threading.Tasks;
using ProudNetSrc.Serialization.Messages;

namespace ProudNetSrc
{
    public class P2PGroup
    {
        private readonly ConcurrentDictionary<uint, RemotePeer> _members = new ConcurrentDictionary<uint, RemotePeer>();
        internal readonly AsyncLock _sync = new AsyncLock();
        private readonly ProudServer _server;

        public uint HostId { get; }
        public bool AllowDirectP2P { get; }
        public IReadOnlyDictionary<uint, RemotePeer> Members => _members;

        internal P2PGroup(ProudServer server, bool allowDirectP2P)
        {
            _server = server;
            HostId = _server.Configuration.HostIdFactory.New();
            AllowDirectP2P = allowDirectP2P;
        }

        public void Join(uint hostId)
        {
            // using (_sync.Lock())
            {
                var encrypted = _server.Configuration.EnableP2PEncryptedMessaging;
                Crypt crypt = null;
                if (encrypted)
                {
                    crypt = new Crypt(_server.Configuration.EncryptedMessageKeyLength,
                        _server.Configuration.FastEncryptedMessageKeyLength);
                }

                var session = _server.Sessions[hostId];
                var remotePeer = new RemotePeer(this, session, crypt);
                if (!_members.TryAdd(hostId, remotePeer))
                {
                    throw new ProudException($"Member {hostId} is already in P2PGroup {HostId}");
                }

                _server.Configuration.Logger?.Debug("Client({HostId}) joined P2PGroup({GroupHostId})", hostId, HostId);
                session.P2PGroup = this;

                if (encrypted)
                {
                    session.SendAsync(new P2PGroup_MemberJoinMessage(HostId, hostId, 0, crypt.AES.Key, AllowDirectP2P));
                }
                else
                {
                    session.SendAsync(new P2PGroup_MemberJoin_UnencryptedMessage(HostId, hostId, 0, AllowDirectP2P));
                }

                foreach (var member in _members.Values.Where(member => member.HostId != hostId))
                {
                    var memberSession = _server.Sessions[member.HostId];
                    var stateA = new P2PConnectionState(member);
                    var stateB = new P2PConnectionState(remotePeer);

                    remotePeer.ConnectionStates[member.HostId] = stateA;
                    member.ConnectionStates[remotePeer.HostId] = stateB;
                    if (encrypted)
                    {
                        memberSession.SendAsync(new P2PGroup_MemberJoinMessage(HostId, hostId, stateB.EventId,
                            crypt.AES.Key, AllowDirectP2P));
                        session.SendAsync(new P2PGroup_MemberJoinMessage(HostId, member.HostId, stateA.EventId,
                            crypt.AES.Key, AllowDirectP2P));
                    }
                    else
                    {
                        memberSession.SendAsync(
                            new P2PGroup_MemberJoin_UnencryptedMessage(HostId, hostId, stateB.EventId, AllowDirectP2P));
                        session.SendAsync(new P2PGroup_MemberJoin_UnencryptedMessage(HostId, member.HostId,
                            stateA.EventId, AllowDirectP2P));
                    }
                }
            }
        }

        public void Leave(uint hostId)
        {
            // using (_sync.Lock())
            {
                if (!_members.TryRemove(hostId, out var memberToLeave))
                {
                    return;
                }

                _server.Configuration.Logger?.Debug("Client({HostId}) left P2PGroup({GroupHostId})", hostId, HostId);
                var session = memberToLeave.Session;
                session.P2PGroup = null;
                session.SendAsync(new P2PGroup_MemberLeaveMessage(hostId, HostId));
                memberToLeave.ConnectionStates.Clear();
                foreach (var member in _members.Values.Where(entry => entry.HostId != hostId))
                {
                    var memberSession = _server.Sessions.GetValueOrDefault(member.HostId);
                    memberSession?.SendAsync(new P2PGroup_MemberLeaveMessage(hostId, HostId));
                    session.SendAsync(new P2PGroup_MemberLeaveMessage(member.HostId, HostId));
                    member.ConnectionStates.Remove(hostId);
                }
            }
        }
    }
}
