using BlubLib.Serialization;
using ProudNetSrc.Serialization.Serializers;

namespace NeoNetsphere.Network.Data.GameRule
{
    [BlubContract]
    public class ChallengeRankerDto
    {
        public ChallengeRankerDto()
        {
            Unk2 = "";
        }

        [BlubMember(0)]
        public int Unk1 { get; set; }

        [BlubMember(1, typeof(StringSerializer))]
        public string Unk2 { get; set; }

        [BlubMember(2)]
        public int Unk3 { get; set; }

        [BlubMember(3)]
        public int Unk4 { get; set; }

        [BlubMember(4)]
        public int Unk5 { get; set; }
    }
}
