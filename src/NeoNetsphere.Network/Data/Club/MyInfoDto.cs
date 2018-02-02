using BlubLib.Serialization;
using ProudNetSrc.Serialization.Serializers;

namespace NeoNetsphere.Network.Data.Club
{
    [BlubContract]
    public class MyInfoDto
    {
        [BlubMember(0)]
        public uint Id { get; set; } //unique club id

        [BlubMember(1, typeof(StringSerializer))]
        public string Type { get; set; } // 0-0-0 Background - Stroke - Inner-Pattern

        [BlubMember(2, typeof(StringSerializer))]
        public string Name { get; set; } //name

        [BlubMember(3)]
        public ClubState State { get; set; } //0 not joined, 1 awaiting acception, 2 joined

        [BlubMember(4)]
        public byte Unk2 { get; set; }

        [BlubMember(5)]
        public byte Unk3 { get; set; }

        [BlubMember(6)]
        public byte Unk4 { get; set; }

        [BlubMember(7)]
        public int MemberCount { get; set; } //membercount

        [BlubMember(8)]
        public int Unk5 { get; set; }

        [BlubMember(9)]
        public int Unk6 { get; set; }

        [BlubMember(10)]
        public int Unk7 { get; set; }

        [BlubMember(11)]
        public long Unk8 { get; set; } //always maxval? 0xFFFFFFFFFFFFFFF

        [BlubMember(12)]
        public int Unk9 { get; set; }

        public MyInfoDto()
        {
            Type = "";
            Id = 0;
            State = 0;
            MemberCount = 0;
        }
    }
}
