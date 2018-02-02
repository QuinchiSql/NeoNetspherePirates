using BlubLib.Serialization;

namespace NeoNetsphere.Network.Data.Chat
{
    [BlubContract]
    public class PlayerNameTagInfoDto
    {
        [BlubMember(0)]
        public ulong AccountId { get; set; }

        [BlubMember(1)]
        public byte Unk1 { get; set; } 

        [BlubMember(2)]
        public byte Unk2 { get; set; }

        [BlubMember(3)]
        public byte Unk3 { get; set; }

        [BlubMember(4)]
        public byte Unk4 { get; set; }
    }
}
