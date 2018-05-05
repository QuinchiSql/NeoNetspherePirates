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
            Unk7 = 1;
            Motto = " ";
            Announce = "";
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
        
        [BlubMember(12, typeof(StringSerializer))]
        public string Motto { get; set; } //motto?

        [BlubMember(13, typeof(StringSerializer))]
        public string Announce { get; set; }

        [BlubMember(14)]
        public int Unk7 { get; set; }

        [BlubMember(15)]
        public int Unk8 { get; set; }

        [BlubMember(16)]
        public int Unk9 { get; set; }

        [BlubMember(17)]
        public int Unk10 { get; set; }
    }
}
