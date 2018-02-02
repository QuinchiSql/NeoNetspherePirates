using BlubLib.Serialization;

namespace NeoNetsphere.Network.Data.Game
{
    [BlubContract]
    public class ClearEsperChipDto
    {
        [BlubMember(0)]
        public ulong Unk1 { get; set; }

        [BlubMember(1)]
        public int Unk2 { get; set; }
    }
}
