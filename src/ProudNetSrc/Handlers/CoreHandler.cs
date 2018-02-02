using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BlubLib.Collections.Concurrent;
using BlubLib.Collections.Generic;
using BlubLib.DotNetty;
using BlubLib.DotNetty.Handlers.MessageHandling;
using BlubLib.Security.Cryptography;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using ProudNetSrc.Codecs;
using ProudNetSrc.Serialization;
using ProudNetSrc.Serialization.Messages;
using ProudNetSrc.Serialization.Messages.Core;

namespace ProudNetSrc.Handlers
{
    internal class CoreHandler : ProudMessageHandler
    {
        private readonly ProudServer _server;
        private readonly Lazy<DateTime> _startTime = new Lazy<DateTime>(() => Process.GetCurrentProcess().StartTime);

        public CoreHandler(ProudServer server)
        {
            _server = server;
        }

        [MessageHandler(typeof(ServerPingTestMessage))]
        public async Task ServerPingTestMessage(IChannelHandlerContext context, ServerPingTestMessage message)
        {
            if (_server.Configuration.EnablePingTest)
            {
                var session = context.Channel.GetAttribute(ChannelAttributes.Session).Get();
                session.PingTestResponseDelay = DateTimeOffset.Now - session.PingTestLastUpdate;
                session.PingTestLastUpdate = DateTimeOffset.Now;
                //Console.WriteLine($"Ping: {session.HostId} => {session.PingTestResponseDelay.Milliseconds} ms");
            }
        }

        [MessageHandler(typeof(S2CRoutedMulticast2Message))]
        public async Task S2CRoutedMulticast2(IChannelHandlerContext context, S2CRoutedMulticast2Message message)
        {
            var buffer = Unpooled.WrappedBuffer(message.Data);
            context.FireChannelRead(buffer);
        }

        [MessageHandler(typeof(RmiMessage))]
        public async Task RmiMessage(IChannelHandlerContext context, RmiMessage message)
        {
            var buffer = Unpooled.WrappedBuffer(message.Data);
            context.FireChannelRead(buffer);
        }

        [MessageHandler(typeof(CompressedMessage))]
        public async Task CompressedMessage(IChannelHandlerContext context, CompressedMessage message)
        {
            var decompressed = message.Data.DecompressZLib();
            var buffer = Unpooled.WrappedBuffer(decompressed);
            context.Channel.Pipeline.Context<ProudFrameDecoder>().FireChannelRead(buffer);
        }

        [MessageHandler(typeof(EncryptedReliableMessage))]
        public async Task EncryptedReliableMessage(IChannelHandlerContext context, ProudSession session,
            EncryptedReliableMessage message)
        {
            var crypt = session.Crypt;
            if (crypt == null)
                return;

            var buffer = context.Allocator.Buffer(message.Data.Length);
            using (var src = new MemoryStream(message.Data))
            using (var dst = new WriteOnlyByteBufferStream(buffer, false))
            {
                crypt.Decrypt(context.Allocator, message.EncryptMode, src, dst, true);
            }
            context.Channel.Pipeline.Context<ProudFrameDecoder>().FireChannelRead(buffer);
        }

        [MessageHandler(typeof(NotifyCSEncryptedSessionKeyMessage))]
        public async Task NotifyCSEncryptedSessionKeyMessage(ProudServer server, ProudSession session,
            NotifyCSEncryptedSessionKeyMessage message)
        {
            var secureKey = server.Rsa.Decrypt(message.SecureKey, true);
            session.Crypt = new Crypt(secureKey);

            var fastKey = session.Crypt.AES.Decrypt(message.FastKey);
            session.Crypt.InitializeFastEncryption(fastKey);
            session.SendAsync(new NotifyCSSessionKeySuccessMessage());
        }

        [MessageHandler(typeof(NotifyServerConnectionRequestDataMessage))]
        public async Task NotifyServerConnectionRequestDataMessage(IChannelHandlerContext context, ProudSession session,
            NotifyServerConnectionRequestDataMessage message)
        {
            if (message.InternalNetVersion != Constants.NetVersion)
            {
                Console.WriteLine($"[ProudNet] Invalid Netversion! <{message.InternalNetVersion}> ({session.RemoteEndPoint})");
                await session.SendAsync(new NotifyProtocolVersionMismatchMessage());
                await session.CloseAsync();
                return;
            }
            if(message.Version != _server.Configuration.Version)
            {
                Console.WriteLine($"[ProudNet] Invalid Protocolversion! <{message.Version}> ({session.RemoteEndPoint})");
                //await session.SendAsync(new NotifyProtocolVersionMismatchMessage());
                //await session.CloseAsync();
                //return;
            }
            _server.AddSession(session);
            session.HandhsakeEvent.Set();
            session.SendAsync(new NotifyServerConnectSuccessMessage(session.HostId, _server.Configuration.Version,
                session.RemoteEndPoint));
        }

        [MessageHandler(typeof(UnreliablePingMessage))]
        public async Task UnreliablePingHandler(IChannelHandlerContext context, ProudSession session,
            UnreliablePingMessage message)
        {
            session.UnreliablePing = TimeSpan.FromSeconds(message.Ping).TotalMilliseconds;
            var ts = DateTime.Now - _startTime.Value;
            session.SendUdpIfAvailableAsync(new UnreliablePongMessage(message.ClientTime, ts.TotalSeconds));
        }

        [MessageHandler(typeof(SpeedHackDetectorPingMessage))]
        public async Task SpeedHackDetectorPingHandler(ProudSession session)
        {
            session.LastSpeedHackDetectorPing = DateTime.Now;
        }

        [MessageHandler(typeof(ReliableRelay1Message))]
        public async Task ReliableRelayHandler(IChannel channel, ProudSession session, ReliableRelay1Message message)
        {
            if (session.P2PGroup == null)
                return;

            foreach (var destination in message.Destination.Where(d => d.HostId != session.HostId))
            {
                if (destination.HostId == 0)
                {
                    var buffer = Unpooled.WrappedBuffer(message.Data);
                    channel?.Pipeline?.Context<CoreMessageDecoder>().FireChannelRead(buffer);
                    continue;
                }

                if (!session.P2PGroup.Members.ContainsKey(destination.HostId))
                    continue;

                var target = _server.Sessions.GetValueOrDefault(destination.HostId);
                if (target != null)
                    await target.SendAsync(
                    new ReliableRelay2Message(new RelayDestinationDto(session.HostId, destination.FrameNumber),
                        message.Data));
            }
        }


        [MessageHandler(typeof(UnreliableRelay1Message))]
        public async Task UnreliableRelayHandler(IChannel channel, ProudSession session,
            UnreliableRelay1Message message)
        {
            if (session?.P2PGroup == null)
                return;

            foreach (var destination in message.Destination.Where(id => id != session.HostId))
            {
                if (destination == 0)
                {
                    var buffer = Unpooled.WrappedBuffer(message.Data);
                    channel?.Pipeline?.Context<CoreMessageDecoder>().FireChannelRead(buffer);
                    continue;
                }

                if (!session?.P2PGroup?.Members.ContainsKey(destination) ?? false)
                    continue;

                var target = _server.Sessions.GetValueOrDefault(destination);
                if(target != null)
                    await target.SendUdpIfAvailableAsync(new UnreliableRelay2Message(session.HostId, message.Data));
            }
        }
        
        [MessageHandler(typeof(ServerHolepunchMessage))]
        public async Task NotifyHolepunchSuccess(ProudServer server, ProudSession session,
            ServerHolepunchMessage message)
        {
            if (session?.P2PGroup == null || !_server.UdpSocketManager.IsRunning ||
                session?.HolepunchMagicNumber != message.MagicNumber)
                return;

            //Logger<>.Debug($"Client:{session.HostId} - Server holepunch success(EndPoint:{message.EndPoint} LocalEndPoint:{message.LocalEndPoint})");

            session?.SendUdpAsync(new ServerHolepunchAckMessage(session.HolepunchMagicNumber, session.UdpEndPoint));
        }

        [MessageHandler(typeof(NotifyHolepunchSuccessMessage))]
        public async Task NotifyHolepunchSuccess(ProudServer server, ProudSession session,
            NotifyHolepunchSuccessMessage message)
        {
            if (session?.P2PGroup == null || !_server.UdpSocketManager.IsRunning ||
                session?.HolepunchMagicNumber != message.MagicNumber)
                return;

            //Logger<>.Debug($"Client:{session.HostId} - Server holepunch success(EndPoint:{message.EndPoint} LocalEndPoint:{message.LocalEndPoint})");

            session.UdpEnabled = true;
            session.UdpLocalEndPoint = message.LocalEndPoint;
            session?.SendUdpAsync(new NotifyClientServerUdpMatchedMessage(message.MagicNumber));
        }

        [MessageHandler(typeof(PeerUdp_ServerHolepunchMessage))]
        public async Task PeerUdp_ServerHolepunch(IChannel channel, ProudSession session,
            PeerUdp_ServerHolepunchMessage message)
        {
            if (!session.UdpEnabled || !_server.UdpSocketManager.IsRunning)
                return;

            var target = session?.P2PGroup?.Members.GetValueOrDefault(message.HostId)?.Session;
            if (target == null || !target.UdpEnabled)
                return;

            session?.SendUdpAsync(
                new PeerUdp_ServerHolepunchAckMessage(message.MagicNumber, target.UdpEndPoint, target.HostId));
        }

        [MessageHandler(typeof(PeerUdp_NotifyHolepunchSuccessMessage))]
        public async Task PeerUdp_NotifyHolepunchSuccess(IChannel channel, ProudSession session,
            PeerUdp_NotifyHolepunchSuccessMessage message)
        {
            if (!session.UdpEnabled || !_server.UdpSocketManager.IsRunning)
                return;

            var remotePeer = session.P2PGroup?.Members[session.HostId];
            var connectionState = remotePeer?.ConnectionStates?.GetValueOrDefault(message.HostId);

            if(connectionState == null)
                return;

            connectionState.PeerUdpHolepunchSuccess = true;
            connectionState.LocalEndPoint = message.LocalEndPoint;
            connectionState.EndPoint = message.EndPoint;
            var connectionStateB = connectionState?.RemotePeer?.ConnectionStates[session.HostId];
            if (connectionStateB?.PeerUdpHolepunchSuccess ?? false)
            {
                remotePeer?.SendAsync(new RequestP2PHolepunchMessage(message.HostId, connectionStateB.LocalEndPoint,
                    connectionState.EndPoint));
                connectionState?.RemotePeer?.SendAsync(new RequestP2PHolepunchMessage(session?.HostId ?? 0,
                    connectionState.LocalEndPoint, connectionStateB.EndPoint));

                //remotePeer.SendAsync(new RequestP2PHolepunchMessage(message.HostId, message.LocalEndPoint, new IPEndPoint(message.EndPoint.Address, message.LocalEndPoint.Port)));
                //connectionState.RemotePeer.SendAsync(new RequestP2PHolepunchMessage(session.HostId, session.UdpLocalEndPoint, new IPEndPoint(session.UdpEndPoint.Address, session.UdpLocalEndPoint.Port)));
                //remotePeer.SendAsync(new RequestP2PHolepunchMessage(message.HostId, message.LocalEndPoint, message.EndPoint));
                //connectionState.RemotePeer.SendAsync(new RequestP2PHolepunchMessage(session.HostId, session.UdpLocalEndPoint, session.UdpEndPoint));
            }
        }
    }
}
