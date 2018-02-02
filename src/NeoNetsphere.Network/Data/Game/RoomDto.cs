using BlubLib.Serialization;
using ProudNetSrc.Serialization.Serializers;

namespace NeoNetsphere.Network.Data.Game
{
    [BlubContract]
    public class RoomDto
    {
        public RoomDto()
        {
            Name = "";
            Password = "";
        }

        [BlubMember(0)]
        public byte RoomId { get; set; }

        [BlubMember(1)]
        public byte State { get; set; }

        [BlubMember(2)]
        public int GameRule { get; set; }

        [BlubMember(3)]
        public byte Map { get; set; }

        [BlubMember(4)]
        public byte PlayerCount { get; set; }

        [BlubMember(5)]
        public byte PlayerLimit { get; set; }

        [BlubMember(6)]
        public int WeaponLimit { get; set; }

        [BlubMember(7, typeof(StringSerializer))]
        public string Password { get; set; }

        [BlubMember(8, typeof(StringSerializer))]
        public string Name { get; set; }

        [BlubMember(9)]
        public bool SpectatorEnabled { get; set; }

        [BlubMember(10)]
        public byte Unk1 { get; set; }

        [BlubMember(11)]
        public int IsRandom { get; set; }

        [BlubMember(12)]
        public short FMBURNMode { get; set; }

        [BlubMember(13)]
        public long Unk4 { get; set; }

        [BlubMember(14)]
        public short Unk5 { get; set; }
    }
}
