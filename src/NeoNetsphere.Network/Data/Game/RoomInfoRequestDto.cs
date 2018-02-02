using System;
using BlubLib.Serialization;
using NeoNetsphere.Network.Serializers;
using ProudNetSrc.Serialization.Serializers;

namespace NeoNetsphere.Network.Data.Game
{
    [BlubContract]
    public class RoomInfoRequestDto
    {
        public RoomInfoRequestDto()
        {
            MasterName = "";
            Unk3 = "";
        }

        [BlubMember(0, typeof(StringSerializer))]
        public string MasterName { get; set; }

        [BlubMember(1)]
        public uint MasterLevel { get; set; }

        [BlubMember(2, typeof(StringSerializer))]
        public string Unk3 { get; set; }

        [BlubMember(3)]
        public bool IsMasterInClan { get; set; }

        [BlubMember(4)]
        public uint ScoreLimit { get; set; }

        [BlubMember(5, typeof(TimeSpanSecondsSerializer))]
        public TimeSpan TimeLimit { get; set; }

        [BlubMember(6)]
        public GameState State { get; set; }

        [BlubMember(7)]
        public int Unk8 { get; set; } //roomid?

        [BlubMember(8)]
        public int Unk9 { get; set; } //weaponlimit?

        [BlubMember(9)]
        public int Unk10 { get; set; }

        [BlubMember(10)]
        public int Unk11 { get; set; }

        [BlubMember(11)]
        public int Unk12 { get; set; }

        [BlubMember(12)]
        public int Unk13 { get; set; }

        [BlubMember(13)]
        public int Unk14 { get; set; }
    }
}
