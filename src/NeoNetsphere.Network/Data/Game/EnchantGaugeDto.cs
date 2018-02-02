using BlubLib.Serialization;

namespace NeoNetsphere.Network.Data.Game
{
    [BlubContract]
    public class EnchantGaugeDto
    {
        [BlubMember(0)]
        public ulong Unk1 { get; set; }

        [BlubMember(1)]
        public int Unk2 { get; set; }

        [BlubMember(2)]
        public byte Unk3 { get; set; }
    }
}
