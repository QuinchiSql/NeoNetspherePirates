using BlubLib.Serialization;
using ProudNetSrc.Serialization.Serializers;

namespace NeoNetsphere.Network.Data.Game
{
    [BlubContract]
    public class RoomPlayerDto
    {
        public RoomPlayerDto()
        {
            Nickname = "";
        }

        [BlubMember(0)]
        public uint ClanId { get; set; } //maybe

        [BlubMember(1)]
        public ulong AccountId { get; set; }

        [BlubMember(2)]
        public byte Unk1 { get; set; }

        [BlubMember(3, typeof(StringSerializer))]
        public string Nickname { get; set; }

        [BlubMember(4)]
        public byte Unk2 { get; set; }

        [BlubMember(5)]
        public byte IsGM { get; set; }
    }
}
