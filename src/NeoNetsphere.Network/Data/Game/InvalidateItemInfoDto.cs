using BlubLib.Serialization;

namespace NeoNetsphere.Network.Data.Game
{
    [BlubContract]
    public class InvalidateItemInfoDto
    {
        [BlubMember(0)]
        public ulong ItemId { get; set; }

        [BlubMember(1)]
        public uint Unk1 { get; set; }

        [BlubMember(2)]
        public uint Unk2 { get; set; }

        [BlubMember(3)]
        public byte Unk3 { get; set; }
    }
}
