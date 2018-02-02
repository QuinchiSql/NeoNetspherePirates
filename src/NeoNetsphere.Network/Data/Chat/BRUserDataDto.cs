using BlubLib.Serialization;

namespace NeoNetsphere.Network.Data.Chat
{
    [BlubContract]
    public class BRUserDataDto
    {
        [BlubMember(0)]
        public uint CountFirstPlaceKilled { get; set; }

        [BlubMember(1)]
        public uint CountFirstPlace { get; set; }
    }
}
