using BlubLib.Serialization;
using ProudNetSrc.Serialization.Serializers;

namespace NeoNetsphere.Network.Data.Chat
{
    [BlubContract]
    public class PlayerInfoShortDto
    {
        [BlubMember(0)]
        public ulong AccountId { get; set; }

        [BlubMember(1, typeof(StringSerializer))]
        public string Nickname { get; set; }

        [BlubMember(2)]
        public int Unk { get; set; }

        [BlubMember(3)]
        public int TotalExp { get; set; }

        [BlubMember(4)]
        public bool IsGM { get; set; }

        public PlayerInfoShortDto()
        {
            AccountId = 0;
            Nickname = "";
            IsGM = false;
        }
    }
}
