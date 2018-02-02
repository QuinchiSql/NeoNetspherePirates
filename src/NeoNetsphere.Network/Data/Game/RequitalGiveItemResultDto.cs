using BlubLib.Serialization;

namespace NeoNetsphere.Network.Data.Game
{
    [BlubContract]
    public class RequitalGiveItemResultDto
    {
        [BlubMember(0)]
        public int Unk1 { get; set; }

        [BlubMember(1)]
        public int Unk2 { get; set; }
    }
}
