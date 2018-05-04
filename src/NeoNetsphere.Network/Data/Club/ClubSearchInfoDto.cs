using BlubLib.Serialization;
using ProudNetSrc.Serialization.Serializers;

namespace NeoNetsphere.Network.Data.Club
{
    [BlubContract]
    public class ClubSearchInfoDto
    {
        public ClubSearchInfoDto()
        {
            Type = "";
            Name = "";
            MasterName = "";
            CreationDate = "";
            Unk7 = 1;
            Unk8 = " ";
            Unk9 = "";
        }

        [BlubMember(0)]
        public uint Id { get; set; }

        [BlubMember(1, typeof(StringSerializer))]
        public string Type { get; set; }

        [BlubMember(2, typeof(StringSerializer))]
        public string Name { get; set; }

        [BlubMember(3)]
        public int MemberCount { get; set; }

        [BlubMember(4, typeof(StringSerializer))]
        public string MasterName { get; set; }

        [BlubMember(5, typeof(StringSerializer))]
        public string CreationDate { get; set; } //20180502211111

        [BlubMember(6)]
        public int Unk1 { get; set; }

        [BlubMember(7)]
        public int Unk2 { get; set; }

        [BlubMember(8)]
        public int Unk3 { get; set; }

        [BlubMember(9)]
        public int Unk4 { get; set; }

        [BlubMember(10)]
        public int Unk5 { get; set; }

        [BlubMember(11)]
        public int Unk6 { get; set; }

        [BlubMember(12)]
        public byte Unk7 { get; set; }

        [BlubMember(13, typeof(StringSerializer))]
        public string Unk8 { get; set; } //motto?

        [BlubMember(14, typeof(StringSerializer))]
        public string Unk9 { get; set; }

        [BlubMember(15)]
        public int Unk10 { get; set; }

        [BlubMember(16)]
        public int Unk11 { get; set; }

        [BlubMember(17)]
        public int Unk12 { get; set; }

        [BlubMember(18)]
        public int Unk13 { get; set; }
    }
}
