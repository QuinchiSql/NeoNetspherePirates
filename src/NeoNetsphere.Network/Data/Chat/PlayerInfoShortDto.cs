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
<<<<<<< HEAD

        public PlayerInfoShortDto()
        {
            AccountId = 0;
=======
        
        public PlayerInfoShortDto()
        {
>>>>>>> 5d1b562013a5b87d78e51d151835d8a28d2f5fc6
            Nickname = "";
            IsGM = false;
        }
    }
}
