using BlubLib.Serialization;
using ProudNetSrc.Serialization.Serializers;

namespace NeoNetsphere.Network.Data.Club
{
    [BlubContract]
    public class ClubInfoDto
    {
        public ClubInfoDto()
        {
            Type = "";
            Name = "";
            MasterName = "";
            CreationDate = "";
        }

        [BlubMember(0)]
        public int Id { get; set; }

        [BlubMember(1, typeof(StringSerializer))]
        public string Type { get; set; }

        [BlubMember(2, typeof(StringSerializer))]
        public string Name { get; set; }

        [BlubMember(3)]
        public int MemberCount { get; set; }

        [BlubMember(4, typeof(StringSerializer))]
        public string MasterName { get; set; }

        [BlubMember(5, typeof(StringSerializer))]
        public string CreationDate { get; set; }

        [BlubMember(6)]
        public int Unk6 { get; set; }

        [BlubMember(7)]
        public int Unk7 { get; set; }

        [BlubMember(8)]
        public int Unk8 { get; set; }

        [BlubMember(9)]
        public int Unk9 { get; set; }

        [BlubMember(10)]
        public int Unk10 { get; set; }

        [BlubMember(11)]
        public int Unk11 { get; set; }

        [BlubMember(12, typeof(StringSerializer))]
        public string Unk12 { get; set; }

        [BlubMember(13, typeof(StringSerializer))]
        public string Unk13 { get; set; }

        [BlubMember(14)]
        public int Unk14 { get; set; }

        [BlubMember(15)]
        public int Unk15 { get; set; }

        [BlubMember(16)]
        public int Unk16 { get; set; }

        [BlubMember(17)]
        public int Unk17 { get; set; }
    }
}
