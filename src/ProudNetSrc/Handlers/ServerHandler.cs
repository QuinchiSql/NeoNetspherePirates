using System;
using System.Net;
using System.Threading.Tasks;
using BlubLib.Collections.Concurrent;
using BlubLib.Collections.Generic;
using BlubLib.DotNetty.Handlers.MessageHandling;
using ProudNetSrc.Serialization.Messages;
using ProudNetSrc.Serialization.Messages.Core;

namespace ProudNetSrc.Handlers
{
    internal class ServerHandler : ProudMessageHandler
    {
        [MessageHandler(typeof(ReliablePingMessage))]
        public async Task ReliablePing(ProudSession session)
        {
            session?.SendAsync(new ReliablePongMessage());
        }

        [MessageHandler(typeof(P2PGroup_MemberJoin_AckMessage))]
        public async Task P2PGroupMemberJoinAck(ProudSession session, P2PGroup_MemberJoin_AckMessage message)
        {
            if (session.P2PGroup == null || session.HostId == message.AddedMemberHostId)
                return;

            var remotePeer = session.P2PGroup?.Members[session.HostId];
            var connectionState = remotePeer?.ConnectionStates?.GetValueOrDefault(message.AddedMemberHostId);

            if (connectionState == null || connectionState?.EventId != message.EventId)
                return;

            connectionState.IsJoined = true;
            var connectionStateB = connectionState.RemotePeer.ConnectionStates[session.HostId];
            if (connectionStateB?.IsJoined ?? false)
            {
                remotePeer?.SendAsync(new P2PRecycleCompleteMessage(connectionState.RemotePeer.HostId));
                connectionState?.RemotePeer.SendAsync(new P2PRecycleCompleteMessage(session.HostId));
            }
        }

        [MessageHandler(typeof(NotifyP2PHolepunchSuccessMessage))]
        public async Task NotifyP2PHolepunchSuccess(ProudSession session, NotifyP2PHolepunchSuccessMessage message)
        {
            var group = session.P2PGroup;
            if (group == null || session.HostId != message.A && session.HostId != message.B)
                return;

            var remotePeerA = group.Members.GetValueOrDefault(message.A);
            var remotePeerB = group.Members.GetValueOrDefault(message.B);

            if (remotePeerA == null || remotePeerB == null)
                return;

            var stateA = remotePeerA.ConnectionStates.GetValueOrDefault(remotePeerB.HostId);
            var stateB = remotePeerB.ConnectionStates.GetValueOrDefault(remotePeerA.HostId);

            if (stateA == null || stateB == null)
                return;

            if (session.HostId == remotePeerA.HostId)
                stateA.HolepunchSuccess = true;
            else if (session.HostId == remotePeerB.HostId)
                stateB.HolepunchSuccess = true;

            var notify = new NotifyDirectP2PEstablishMessage(message.A, message.B, message.ABSendAddr,
                message.ABRecvAddr,
                message.BASendAddr, message.BARecvAddr);

            if (stateA.HolepunchSuccess || stateB.HolepunchSuccess)
            {
                remotePeerA?.SendAsync(notify);
                remotePeerB?.SendAsync(notify);
            }
        }

        [MessageHandler(typeof(ShutdownTcpMessage))]
        public async Task ShutdownTcp(ProudSession session)
        {
            session?.CloseAsync();
        }

        [MessageHandler(typeof(NotifyLogMessage))]
        public async Task NotifyLog(NotifyLogMessage message)
        {
            //Logger<>.Debug($"{message.TraceId} - {message.Message}");
        }

        [MessageHandler(typeof(NotifyJitDirectP2PTriggeredMessage))]
        public async Task NotifyJitDirectP2PTriggered(ProudSession session, NotifyJitDirectP2PTriggeredMessage message)
        {
            var group = session.P2PGroup;

            if (group == null)
                return;

            var remotePeerA = group.Members.GetValueOrDefault(session.HostId);
            var remotePeerB = group.Members.GetValueOrDefault(message.HostId);

            if (remotePeerA == null || remotePeerB == null)
                return;

            var stateA = remotePeerA.ConnectionStates.GetValueOrDefault(remotePeerB.HostId);
            var stateB = remotePeerB.ConnectionStates.GetValueOrDefault(remotePeerA.HostId);

            if (stateA == null || stateB == null)
                return;

            if (session.HostId == remotePeerA.HostId)
                stateA.JitTriggered = true;
            else if (session.HostId == remotePeerB.HostId)
                stateB.JitTriggered = true;

            if (stateA.JitTriggered || stateB.JitTriggered)
            {
                remotePeerA?.SendAsync(new NewDirectP2PConnectionMessage(remotePeerB.HostId));
                remotePeerB?.SendAsync(new NewDirectP2PConnectionMessage(remotePeerA.HostId));
            }
        }

        [MessageHandler(typeof(NotifyNatDeviceNameDetectedMessage))]
        public async Task NotifyNatDeviceNameDetected()
        {
        }

        [MessageHandler(typeof(C2S_RequestCreateUdpSocketMessage))]
        public async Task C2S_RequestCreateUdpSocket(ProudServer server, ProudSession session)
        {
            if (session.P2PGroup == null || !server.UdpSocketManager.IsRunning)
                return;

            //Logger<>.Debug($"Client:{session.HostId} - Requesting UdpSocket");
            var socket = server.UdpSocketManager.NextSocket();
            session.UdpSocket = socket;
            session.HolepunchMagicNumber = Guid.NewGuid();
            session?.SendAsync(new S2C_RequestCreateUdpSocketMessage(new IPEndPoint(server.UdpSocketManager.Address,
                ((IPEndPoint) socket.Channel.LocalAddress).Port)));
        }

        [MessageHandler(typeof(C2S_CreateUdpSocketAckMessage))]
        public async Task C2S_CreateUdpSocketAck(ProudServer server, ProudSession session,
            C2S_CreateUdpSocketAckMessage message)
        {
            if (session.P2PGroup == null || !server.UdpSocketManager.IsRunning)
                return;

            //Logger<>.Debug($"Client:{session.HostId} - Starting server holepunch");
            session?.SendAsync(new RequestStartServerHolepunchMessage(session.HolepunchMagicNumber));
        }

        [MessageHandler(typeof(ReportC2CUdpMessageCountMessage))]
        public async Task ReportC2CUdpMessageCount()
        {
            //Todo
        }

        [MessageHandler(typeof(ReportC2SUdpMessageTrialCountMessage))]
        public async Task ReportC2SUdpMessageTrialCount()
        {
            //Todo
        }
    }
}
