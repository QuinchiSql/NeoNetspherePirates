using System.Drawing;
using BlubLib.Serialization;
using NeoNetsphere.Network.Serializers;
using ProudNetSrc.Serialization.Serializers;

namespace NeoNetsphere.Network.Data.Game
{
    [BlubContract]
    public class ChannelInfoDto
    {
        public ChannelInfoDto()
        {
            Name = "";
            Rank = "FREE";
            Description = "";
            Color = Color.Black;
            minKD = 0.0f;
            maxKD = -1.0f;
        }

        [BlubMember(0)]
        public ushort Id { get; set; }

        [BlubMember(1)]
        public ushort PlayersOnline { get; set; }

        [BlubMember(2)]
        public ushort PlayerLimit { get; set; }

        [BlubMember(3, typeof(IntBooleanSerializer))]
        public bool IsClanChannel { get; set; }

        [BlubMember(4, typeof(StringSerializer))]
        public string Name { get; set; }

        [BlubMember(5, typeof(StringSerializer))]
        public string Rank { get; set; }

        [BlubMember(6, typeof(StringSerializer))]
        public string Description { get; set; }

        [BlubMember(7, typeof(ColorSerializer))]
        public Color Color { get; set; }

        [BlubMember(8)]
        public uint MinLevel { get; set; }

        [BlubMember(9)]
        public uint MaxLevel { get; set; }

        [BlubMember(10)]
        public float minKD { get; set; }

        [BlubMember(11)]
        public float maxKD { get; set; }
    }
}
