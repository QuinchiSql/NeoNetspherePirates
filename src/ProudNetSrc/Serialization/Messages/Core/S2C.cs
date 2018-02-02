using System;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using BlubLib.Serialization;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Security;
using ProudNetSrc.Serialization.Serializers;

namespace ProudNetSrc.Serialization.Messages.Core
{
    [BlubContract]
    internal class ConnectServerTimedoutMessage : ICoreMessage
    {
    }

    [BlubContract(typeof(Serializer))]
    internal class NotifyServerConnectionHintMessage : ICoreMessage
    {
        public NotifyServerConnectionHintMessage()
        {
            Config = new NetConfigDto();
            PublicKey = new RSAParameters();
        }

        public NotifyServerConnectionHintMessage(NetConfigDto config, RSAParameters publicKey)
        {
            Config = config;
            PublicKey = publicKey;
        }

        public NetConfigDto Config { get; set; }
        public RSAParameters PublicKey { get; set; }

        internal class Serializer : ISerializer<NotifyServerConnectionHintMessage>
        {
            public bool CanHandle(Type type)
            {
                return type == typeof(NotifyServerConnectionHintMessage);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Serialize(BinaryWriter writer, NotifyServerConnectionHintMessage value)
            {
                var pubKey = DotNetUtilities.GetRsaPublicKey(value.PublicKey);
                var pubKeyStruct = new RsaPublicKeyStructure(pubKey.Modulus, pubKey.Exponent);
                var encodedKey = pubKeyStruct.GetDerEncoded();
                BlubLib.Serialization.Serializer.Serialize(writer, value.Config);
                writer.WriteStruct(encodedKey);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NotifyServerConnectionHintMessage Deserialize(BinaryReader reader)
            {
                var config = BlubLib.Serialization.Serializer.Deserialize<NetConfigDto>(reader);
                var encodedKey = reader.ReadStruct();
                var sequence = (DerSequence) Asn1Object.FromByteArray(encodedKey);
                var modulus = ((DerInteger) sequence[0]).Value.ToByteArrayUnsigned();
                var exponent = ((DerInteger) sequence[1]).Value.ToByteArrayUnsigned();
                var publicKey = new RSAParameters {Exponent = exponent, Modulus = modulus};
                return new NotifyServerConnectionHintMessage(config, publicKey);
            }
        }
    }

    [BlubContract]
    internal class NotifyCSSessionKeySuccessMessage : ICoreMessage
    {
    }

    [BlubContract]
    internal class NotifyProtocolVersionMismatchMessage : ICoreMessage
    {
    }

    [BlubContract]
    internal class NotifyServerDeniedConnectionMessage : ICoreMessage
    {
        [BlubMember(0)]
        public ushort Unk { get; set; }
    }

    [BlubContract]
    internal class NotifyServerConnectSuccessMessage : ICoreMessage
    {
        public NotifyServerConnectSuccessMessage()
        {
            Version = Guid.Empty;
            UserData = Array.Empty<byte>();
            EndPoint = new IPEndPoint(0, 0);
        }

        public NotifyServerConnectSuccessMessage(uint hostId, Guid version, IPEndPoint endPoint)
            : this()
        {
            HostId = hostId;
            Version = version;
            EndPoint = endPoint;
        }

        [BlubMember(0)]
        public uint HostId { get; set; }

        [BlubMember(1)]
        public Guid Version { get; set; }

        [BlubMember(2, typeof(ArrayWithScalarSerializer))]
        public byte[] UserData { get; set; }

        [BlubMember(3, typeof(IPEndPointSerializer))]
        public IPEndPoint EndPoint { get; set; }
    }

    [BlubContract]
    internal class RequestStartServerHolepunchMessage : ICoreMessage
    {
        public RequestStartServerHolepunchMessage()
        {
            MagicNumber = Guid.Empty;
        }

        public RequestStartServerHolepunchMessage(Guid magicNumber)
        {
            MagicNumber = magicNumber;
        }

        [BlubMember(0)]
        public Guid MagicNumber { get; set; }
    }

    [BlubContract]
    internal class ServerHolepunchAckMessage : ICoreMessage
    {
        public ServerHolepunchAckMessage()
        {
            MagicNumber = Guid.Empty;
            EndPoint = new IPEndPoint(0, 0);
        }

        public ServerHolepunchAckMessage(Guid magicNumber, IPEndPoint endPoint)
        {
            MagicNumber = magicNumber;
            EndPoint = endPoint;
        }

        [BlubMember(0)]
        public Guid MagicNumber { get; set; }

        [BlubMember(1, typeof(IPEndPointSerializer))]
        public IPEndPoint EndPoint { get; set; }
    }

    [BlubContract]
    internal class NotifyClientServerUdpMatchedMessage : ICoreMessage
    {
        public NotifyClientServerUdpMatchedMessage()
        {
            MagicNumber = Guid.Empty;
        }

        public NotifyClientServerUdpMatchedMessage(Guid magicNumber)
        {
            MagicNumber = magicNumber;
        }

        [BlubMember(0)]
        public Guid MagicNumber { get; set; }
    }

    [BlubContract]
    internal class PeerUdp_ServerHolepunchAckMessage : ICoreMessage
    {
        public PeerUdp_ServerHolepunchAckMessage()
        {
            MagicNumber = Guid.Empty;
            EndPoint = new IPEndPoint(0, 0);
        }

        public PeerUdp_ServerHolepunchAckMessage(Guid magicNumber, IPEndPoint endPoint, uint hostId)
        {
            MagicNumber = magicNumber;
            EndPoint = endPoint;
            HostId = hostId;
        }

        [BlubMember(0)]
        public Guid MagicNumber { get; set; }

        [BlubMember(1, typeof(IPEndPointSerializer))]
        public IPEndPoint EndPoint { get; set; }

        [BlubMember(2)]
        public uint HostId { get; set; }
    }

    [BlubContract]
    internal class UnreliablePongMessage : ICoreMessage
    {
        public UnreliablePongMessage()
        {
        }

        public UnreliablePongMessage(double clientTime, double serverTime)
        {
            ClientTime = clientTime;
            ServerTime = serverTime;
        }

        [BlubMember(0)]
        public double ClientTime { get; set; }

        [BlubMember(1)]
        public double ServerTime { get; set; }
    }

    [BlubContract]
    internal class ReliableRelay2Message : ICoreMessage
    {
        public ReliableRelay2Message()
        {
            Destination = new RelayDestinationDto();
        }

        public ReliableRelay2Message(RelayDestinationDto destination, byte[] data)
        {
            Destination = destination;
            Data = data;
        }

        [BlubMember(0)]
        public RelayDestinationDto Destination { get; set; }

        [BlubMember(1, typeof(ArrayWithScalarSerializer))]
        public byte[] Data { get; set; }
    }

    [BlubContract]
    internal class UnreliableRelay2Message : ICoreMessage
    {
        public UnreliableRelay2Message()
        {
        }

        public UnreliableRelay2Message(uint hostId, byte[] data)
        {
            HostId = hostId;
            Data = data;
        }

        [BlubMember(0)]
        public uint HostId { get; set; }

        [BlubMember(1, typeof(ArrayWithScalarSerializer))]
        public byte[] Data { get; set; }
    }
}
