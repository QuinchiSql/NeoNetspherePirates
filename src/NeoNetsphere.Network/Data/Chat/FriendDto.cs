using BlubLib.Serialization;
using ProudNetSrc.Serialization.Serializers;

namespace NeoNetsphere.Network.Data.Chat
{
    [BlubContract]
    public class FriendDto
    {
        public FriendDto()
        {
            Nickname = "";
        }

        [BlubMember(0)]
        public ulong AccountId { get; set; }

        [BlubMember(1, typeof(StringSerializer))]
        public string Nickname { get; set; }

        [BlubMember(2)]
        public uint State { get; set; } // request pending, accepted etc.

        [BlubMember(3)]
        public uint Unk { get; set; }
    }
}
