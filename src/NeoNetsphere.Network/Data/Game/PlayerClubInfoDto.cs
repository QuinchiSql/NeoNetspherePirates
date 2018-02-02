using BlubLib.Serialization;
using ProudNetSrc.Serialization.Serializers;

namespace NeoNetsphere.Network.Data.Game
{
    [BlubContract]
    public class PlayerClubInfoDto
    {
        public PlayerClubInfoDto()
        {
            Id = 0;
            Type = "";
            Name = "";
        }

        [BlubMember(0)]
        public uint Id { get; set; }

        [BlubMember(1, typeof(StringSerializer))]
        public string Name { get; set; }

        [BlubMember(2, typeof(StringSerializer))]
        public string Type { get; set; }
    }
}
