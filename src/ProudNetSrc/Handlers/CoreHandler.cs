// <copyright file="CoreHandler.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ProudNetSrc.Handlers
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using BlubLib.Collections.Concurrent;
    using BlubLib.Collections.Generic;
    using BlubLib.DotNetty;
    using BlubLib.DotNetty.Handlers.MessageHandling;
    using DotNetty.Buffers;
    using DotNetty.Transport.Channels;
    using Codecs;
    using Serialization;
    using Serialization.Messages;
    using Serialization.Messages.Core;

    internal class CoreHandler : ProudMessageHandler
    {
        private readonly ProudServer _server;
        private readonly Lazy<DateTime> _startTime = new Lazy<DateTime>(() => Process.GetCurrentProcess().StartTime);

        public CoreHandler(ProudServer server)
        {
            _server = server;
        }

        [MessageHandler(typeof(RmiMessage))]
        public void RmiMessage(IChannelHandlerContext context, RmiMessage message, RecvContext recvContext)
        {
            recvContext.Message = Unpooled.WrappedBuffer(message.Data);
            context.FireChannelRead(recvContext);
        }

        [MessageHandler(typeof(CompressedMessage))]
        public void CompressedMessage(IChannelHandlerContext context, CompressedMessage message, RecvContext recvContext)
        {
            var decompressed = message.Data.DecompressZLib();
            recvContext.Message = Unpooled.WrappedBuffer(decompressed);
            context.Channel.Pipeline.Context<ProudFrameDecoder>().FireChannelRead(recvContext);
        }

        [MessageHandler(typeof(EncryptedReliableMessage))]
        public void EncryptedReliableMessage(IChannelHandlerContext context, ProudSession session, EncryptedReliableMessage message, RecvContext recvContext)
        {
            var crypt = session.Crypt;
            if (crypt != null)
            {
                var buffer = context.Allocator.Buffer(message.Data.Length);
                using (var src = new MemoryStream(message.Data))
                using (var dst = new WriteOnlyByteBufferStream(buffer, false))
                {
                    crypt.Decrypt(context.Allocator, message.EncryptMode, src, dst, true);
                }
                recvContext.Message = buffer;
                context.Channel.Pipeline.Context<ProudFrameDecoder>().FireChannelRead(recvContext);
            }
        }

        [MessageHandler(typeof(NotifyCSEncryptedSessionKeyMessage))]
        public void NotifyCSEncryptedSessionKeyMessage(ProudServer server, ProudSession session, NotifyCSEncryptedSessionKeyMessage message)
        {
            session.Logger?.Verbose("Handshake:NotifyCSEncryptedSessionKey");
            session.Crypt = new Crypt(server.Rsa.Decrypt(message.SecureKey, true));
            session.Crypt.InitializeFastEncryption(session.Crypt.AES.Decrypt(message.FastKey));
            session.SendAsync(new NotifyCSSessionKeySuccessMessage());
        }

        [MessageHandler(typeof(NotifyServerConnectionRequestDataMessage))]
        public void NotifyServerConnectionRequestDataMessage(ProudSession session, NotifyServerConnectionRequestDataMessage message)
        {
            session.Logger?.Verbose("Handshake:NotifyServerConnectionRequestData");
            if (message.InternalNetVersion != Constants.NetVersion ||
                    message.Version != _server.Configuration.Version)
            {
                session.Logger?.Warning(
                    "Protocol version mismatch - Client={@ClientVersion} Server={@ServerVersion}",
                    new { NetVersion = message.InternalNetVersion, message.Version },
                    new { Constants.NetVersion, _server.Configuration.Version });
                session.SendAsync(new NotifyProtocolVersionMismatchMessage());
                session.CloseAsync();
                return;
            }

            _server.AddSession(session);
            session.HandhsakeEvent.Set();
            session.SendAsync(new NotifyServerConnectSuccessMessage(session.HostId, _server.Configuration.Version, session.RemoteEndPoint));
        }

        [MessageHandler(typeof(UnreliablePingMessage))]
        public void UnreliablePingHandler(ProudSession session, UnreliablePingMessage message, RecvContext recvContext)
        {
            session.UnreliablePing = TimeSpan.FromSeconds(message.Ping).TotalMilliseconds;
            if (recvContext.UdpEndPoint != null)
            {
                session.LastUdpPing = DateTimeOffset.Now;
            }

            var ts = DateTime.Now - _startTime.Value;
            session.SendUdpIfAvailableAsync(new UnreliablePongMessage(message.ClientTime, ts.TotalSeconds));
        }

        [MessageHandler(typeof(SpeedHackDetectorPingMessage))]
        public void SpeedHackDetectorPingHandler(ProudSession session)
        {
            session.LastSpeedHackDetectorPing = DateTime.Now;
        }

        [MessageHandler(typeof(ReliableRelay1Message))]
        public void ReliableRelayHandler(ProudSession session, ReliableRelay1Message message)
        {
            if (session.P2PGroup == null)
            {
                return;
            }

            foreach (var destination in message.Destination.Where(d => d.HostId != session.HostId))
            {
                if (session.P2PGroup == null)
                {
                    continue;
                }

                if (!session.P2PGroup.Members.ContainsKey(destination.HostId))
                {
                    continue;
                }

                var target = _server.Sessions.GetValueOrDefault(destination.HostId);
                target?.SendAsync(new ReliableRelay2Message(new RelayDestinationDto(session.HostId, destination.FrameNumber), message.Data));
            }
        }

        [MessageHandler(typeof(UnreliableRelay1Message))]
        public void UnreliableRelayHandler(ProudSession session, UnreliableRelay1Message message)
        {
            foreach (var destination in message.Destination.Where(id => id != session.HostId))
            {
                if (session.P2PGroup == null)
                {
                    continue;
                }

                if (!session.P2PGroup.Members.ContainsKey(destination))
                {
                    continue;
                }

                var target = _server.Sessions.GetValueOrDefault(destination);
                target?.SendUdpIfAvailableAsync(new UnreliableRelay2Message(session.HostId, message.Data));
            }
        }

        [MessageHandler(typeof(ServerHolepunchMessage))]
        public void ServerHolepunch(ProudSession session, ServerHolepunchMessage message)
        {
            session.Logger?.Debug("ServerHolepunch={@Message}", message);
            if (session.P2PGroup == null || !_server.UdpSocketManager.IsRunning ||
                session.HolepunchMagicNumber != message.MagicNumber)
            {
                return;
            }

            session.SendUdpAsync(new ServerHolepunchAckMessage(session.HolepunchMagicNumber, session.UdpEndPoint));
        }

        [MessageHandler(typeof(NotifyHolepunchSuccessMessage))]
        public void NotifyHolepunchSuccess(ProudSession session, NotifyHolepunchSuccessMessage message)
        {
            session.Logger?.Debug("NotifyHolepunchSuccess={@Message}", message);
            if (session.P2PGroup == null || !_server.UdpSocketManager.IsRunning ||
                session.HolepunchMagicNumber != message.MagicNumber)
            {
                return;
            }

            session.LastUdpPing = DateTimeOffset.Now;
            session.UdpEnabled = true;
            session.UdpLocalEndPoint = message.LocalEndPoint;
            session.SendUdpAsync(new NotifyClientServerUdpMatchedMessage(message.MagicNumber));
        }

        [MessageHandler(typeof(PeerUdp_ServerHolepunchMessage))]
        public void PeerUdp_ServerHolepunch(ProudSession session, PeerUdp_ServerHolepunchMessage message, RecvContext recvContext)
        {
            session.Logger?.Debug("PeerUdp_ServerHolepunch={@Message}", message);
            if (!session.UdpEnabled || !_server.UdpSocketManager.IsRunning)
            {
                return;
            }

            var target = session.P2PGroup?.Members.GetValueOrDefault(message.HostId)?.Session;
            if (target == null || !target.UdpEnabled)
            {
                return;
            }

            session.SendUdpAsync(new PeerUdp_ServerHolepunchAckMessage(message.MagicNumber, recvContext.UdpEndPoint, target.HostId));
        }

        [MessageHandler(typeof(PeerUdp_NotifyHolepunchSuccessMessage))]
        public void PeerUdp_NotifyHolepunchSuccess(ProudSession session, PeerUdp_NotifyHolepunchSuccessMessage message)
        {
            session.Logger?.Debug("PeerUdp_NotifyHolepunchSuccess={@Message}", message);
            if (!session.UdpEnabled || !_server.UdpSocketManager.IsRunning)
            {
                return;
            }

            var remotePeer = session.P2PGroup?.Members.GetValueOrDefault(session.HostId);
            if (remotePeer != null)
            {
                using (remotePeer._sync.Lock())
                {
                    var connectionState = remotePeer.ConnectionStates.GetValueOrDefault(message.HostId);
                    if (connectionState != null)
                    {
                        connectionState.PeerUdpHolepunchSuccess = true;
                        connectionState.LocalEndPoint = message.LocalEndPoint;
                        connectionState.EndPoint = message.EndPoint;
                        var connectionStateB = connectionState.RemotePeer?.ConnectionStates[session.HostId];
                        if (connectionStateB?.PeerUdpHolepunchSuccess ?? false)
                        {
                            remotePeer.SendAsync(new RequestP2PHolepunchMessage(message.HostId,
                                connectionStateB.LocalEndPoint,
                                connectionStateB.EndPoint));
                            connectionState.RemotePeer.SendAsync(new RequestP2PHolepunchMessage(session.HostId,
                                connectionState.LocalEndPoint, connectionState.EndPoint));
                        }
                    }
                }
            }
        }
    }
}
