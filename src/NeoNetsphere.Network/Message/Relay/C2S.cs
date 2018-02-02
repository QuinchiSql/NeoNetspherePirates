using BlubLib.Serialization;
using NeoNetsphere.Network.Data.Relay;
using NeoNetsphere.Network.Serializers;
using ProudNetSrc.Serialization.Serializers;

namespace NeoNetsphere.Network.Message.Relay
{
    [BlubContract]
    public class CRequestLoginMessage : IRelayMessage
    {
        [BlubMember(0)]
        public ulong AccountId { get; set; }

        [BlubMember(1, typeof(StringSerializer))]
        public string Nickname { get; set; }

        [BlubMember(2, typeof(RoomLocationSerializer))]
        public RoomLocation RoomLocation { get; set; }

        [BlubMember(3)]
        public bool CreatedRoom { get; set; }
    }

    [BlubContract]
    public class CNotifyP2PLogMessage : IRelayMessage
    {
        [BlubMember(0)]
        public int Unk1 { get; set; }

        [BlubMember(1)]
        public short Unk2 { get; set; }

        [BlubMember(2)]
        public byte Unk3 { get; set; }
    }
}
