using BlubLib.Serialization;
using ProudNetSrc.Serialization.Serializers;

namespace NeoNetsphere.Network.Data.Game
{
    [BlubContract]
    public class PromotionPunkinRankerDto
    {
        [BlubMember(0, typeof(StringSerializer))]
        public string Unk1 { get; set; }

        [BlubMember(1)]
        public int Unk2 { get; set; }
    }
}
