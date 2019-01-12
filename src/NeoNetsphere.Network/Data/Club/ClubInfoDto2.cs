using BlubLib.Serialization;
using ProudNetSrc.Serialization.Serializers;

namespace NeoNetsphere.Network.Data.Club
{
    [BlubContract]
    public class ClubInfoDto2
    {
        public ClubInfoDto2()
        {
            Type = "";
            Name = "";

            Unk1 = 1;
            Unk2 = 1;
            Unk3 = 1;

            MasterName = "";
            CreationDate = "";
        }

        [BlubMember(0)]
        public uint Id { get; set; }

        [BlubMember(1)]
        public uint Id2 { get; set; }

        [BlubMember(2, typeof(StringSerializer))]
        public string Name { get; set; }

        [BlubMember(3, typeof(StringSerializer))]
        public string Type { get; set; }

        [BlubMember(4)]
        public byte Unk1 { get; set; }

        [BlubMember(5)]
        public byte Unk2 { get; set; }

        [BlubMember(6)]
        public byte Unk3 { get; set; }

        [BlubMember(7)]
        public uint Unk4 { get; set; } //rank?

        [BlubMember(8)]
        public uint Unk5 { get; set; }

        [BlubMember(9)]
        public byte Unk6 { get; set; }

        [BlubMember(10)]
        public uint Unk7 { get; set; } //6

        [BlubMember(11, typeof(StringSerializer))]
        public string MasterName { get; set; }

        [BlubMember(12)]
        public uint Unk8 { get; set; } //1

        [BlubMember(13)]
        public uint MemberCount { get; set; } //count?

        [BlubMember(14, typeof(StringSerializer))]
        public string Motto { get; set; }

        [BlubMember(15, typeof(StringSerializer))]
        public string CreationDate { get; set; }

        [BlubMember(16, typeof(StringSerializer))]
        public string Unk10 { get; set; }

        [BlubMember(17, typeof(StringSerializer))]
        public string Unk11 { get; set; }

        [BlubMember(18)]
        public uint Unk12 { get; set; } //0xFFFFFF

        [BlubMember(19)]
        public uint Unk13 { get; set; }

        [BlubMember(20)]
        public uint Unk14 { get; set; }

        [BlubMember(21)]
        public uint Unk15 { get; set; }

        [BlubMember(22)]
        public uint Unk16 { get; set; }

        [BlubMember(23)]
        public uint Unk17 { get; set; }

        [BlubMember(24)]
        public uint Unk18 { get; set; }

        [BlubMember(25)]
        public uint Unk19 { get; set; }

        [BlubMember(26)]
        public uint Unk20 { get; set; }

        [BlubMember(27)]
        public uint Unk21 { get; set; }

        [BlubMember(28)]
        public ushort Unk22 { get; set; }
    }
}

