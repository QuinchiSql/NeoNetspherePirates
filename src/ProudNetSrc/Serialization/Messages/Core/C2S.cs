using System;
using System.Net;
using BlubLib.Serialization;
using ProudNetSrc.Serialization.Serializers;

namespace ProudNetSrc.Serialization.Messages.Core
{
    [BlubContract]
    internal class NotifyCSEncryptedSessionKeyMessage : ICoreMessage
    {
        [BlubMember(0, typeof(ArrayWithScalarSerializer))]
        public byte[] SecureKey { get; set; }

        [BlubMember(1, typeof(ArrayWithScalarSerializer))]
        public byte[] FastKey { get; set; }
    }

    [BlubContract]
    internal class NotifyServerConnectionRequestDataMessage : ICoreMessage
    {
        public NotifyServerConnectionRequestDataMessage()
        {
            Version = Guid.Empty;
            InternalNetVersion = Constants.NetVersion;
        }

        [BlubMember(0, typeof(ArrayWithScalarSerializer))]
        public byte[] UserData { get; set; }

        [BlubMember(1)]
        public Guid Version { get; set; }

        [BlubMember(2)]
        public uint InternalNetVersion { get; set; }
    }

    [BlubContract]
    internal class ServerHolepunchMessage : ICoreMessage
    {
        public ServerHolepunchMessage()
        {
            MagicNumber = Guid.Empty;
        }

        [BlubMember(0)]
        public Guid MagicNumber { get; set; }
    }

    [BlubContract]
    internal class NotifyHolepunchSuccessMessage : ICoreMessage
    {
        public NotifyHolepunchSuccessMessage()
        {
            MagicNumber = Guid.Empty;
            LocalEndPoint = new IPEndPoint(0, 0);
            EndPoint = LocalEndPoint;
        }

        [BlubMember(0)]
        public Guid MagicNumber { get; set; }

        [BlubMember(1, typeof(IPEndPointSerializer))]
        public IPEndPoint LocalEndPoint { get; set; }

        [BlubMember(2, typeof(IPEndPointSerializer))]
        public IPEndPoint EndPoint { get; set; }
    }

    [BlubContract]
    internal class PeerUdp_ServerHolepunchMessage : ICoreMessage
    {
        public PeerUdp_ServerHolepunchMessage()
        {
            MagicNumber = Guid.Empty;
        }

        [BlubMember(0)]
        public Guid MagicNumber { get; set; }

        [BlubMember(1)]
        public uint HostId { get; set; }
    }

    [BlubContract]
    internal class PeerUdp_NotifyHolepunchSuccessMessage : ICoreMessage
    {
        public PeerUdp_NotifyHolepunchSuccessMessage()
        {
            LocalEndPoint = new IPEndPoint(0, 0);
            EndPoint = new IPEndPoint(0, 0);
        }

        [BlubMember(0, typeof(IPEndPointSerializer))]
        public IPEndPoint LocalEndPoint { get; set; }

        [BlubMember(1, typeof(IPEndPointSerializer))]
        public IPEndPoint EndPoint { get; set; }

        [BlubMember(2)]
        public uint HostId { get; set; }
    }

    [BlubContract]
    internal class UnreliablePingMessage : ICoreMessage
    {
        [BlubMember(0)]
        public double ClientTime { get; set; }

        [BlubMember(1)]
        public double Ping { get; set; }
    }

    [BlubContract]
    internal class SpeedHackDetectorPingMessage : ICoreMessage
    {
    }

    [BlubContract]
    internal class ReliableRelay1Message : ICoreMessage
    {
        public ReliableRelay1Message()
        {
            Destination = Array.Empty<RelayDestinationDto>();
            Data = Array.Empty<byte>();
        }

        [BlubMember(0, typeof(ArrayWithScalarSerializer))]
        public RelayDestinationDto[] Destination { get; set; }

        [BlubMember(1, typeof(ArrayWithScalarSerializer))]
        public byte[] Data { get; set; }
    }

    [BlubContract]
    internal class UnreliableRelay1Message : ICoreMessage
    {
        [BlubMember(0)]
        public MessagePriority Priority { get; set; }
        
        [BlubMember(1, typeof(ScalarSerializer))]
        public int UniqueId { get; set; }
        
        [BlubMember(2, typeof(ArrayWithScalarSerializer))]
        public uint[] Destination { get; set; }
        
        [BlubMember(3, typeof(ArrayWithScalarSerializer))]
        public byte[] Data { get; set; }
        
        public UnreliableRelay1Message()
        {
            Destination = Array.Empty<uint>();
            Data = Array.Empty<byte>();
        }
    }

    [BlubContract]
    internal class urdestlist
    {
        [BlubMember(0)]
        public uint hostid { get; set; }

        [BlubMember(2, typeof(ArrayWithScalarSerializer))]
        public uint[] excludeHostIdArray { get; set; }
    }

    [BlubContract]
    internal class UnreliableRelay1_RelayDestListCompressedMessage : ICoreMessage
    {
        public UnreliableRelay1_RelayDestListCompressedMessage()
        {
            Destination = Array.Empty<uint>();
            Data = Array.Empty<byte>();
        }

        [BlubMember(0)]
        public MessagePriority Priority { get; set; }

        [BlubMember(1, typeof(ScalarSerializer))]
        public int UniqueId { get; set; }

        [BlubMember(2, typeof(ArrayWithScalarSerializer))]
        public uint[] Destination { get; set; }

        [BlubMember(3, typeof(ArrayWithScalarSerializer))]
        public urdestlist[] Destination2 { get; set; }

        [BlubMember(3, typeof(ArrayWithScalarSerializer))]
        public byte[] Data { get; set; }
    }
}
