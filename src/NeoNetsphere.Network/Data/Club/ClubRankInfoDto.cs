using BlubLib.Serialization;
using ProudNetSrc.Serialization.Serializers;

namespace NeoNetsphere.Network.Data.Club
{
    [BlubContract]
    public class ClubRankInfoDto
    {
        public ClubRankInfoDto()
        {
        }

        [BlubMember(0)]
        public uint ClanId { get; set; }

        [BlubMember(1, typeof(StringSerializer))]
        public string Name { get; set; }

        [BlubMember(2, typeof(StringSerializer))]
        public string Type { get; set; }

        [BlubMember(3, typeof(StringSerializer))]
        public string Unk2 { get; set; }

        [BlubMember(4)]
        public uint Unk3 { get; set; }

        [BlubMember(5)]
        public uint Unk4 { get; set; }

        [BlubMember(6)]
        public uint Unk5 { get; set; }

        [BlubMember(7, typeof(StringSerializer))]
        public string MasterName { get; set; }

        [BlubMember(8)]
        public uint Unk6 { get; set; }

        [BlubMember(9)]
        public uint Unk7 { get; set; }

        [BlubMember(10)]
        public uint Unk8 { get; set; }

        [BlubMember(11, typeof(StringSerializer))]
        public string CreationDate { get; set; }

        [BlubMember(12, typeof(StringSerializer))]
        public string Unk9 { get; set; }

        [BlubMember(13, typeof(StringSerializer))]
        public string Unk10 { get; set; }
        
        //todo

    }
}
