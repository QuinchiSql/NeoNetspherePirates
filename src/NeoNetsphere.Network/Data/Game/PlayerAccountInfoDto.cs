using System;
using BlubLib.Serialization;
using NeoNetsphere.Network.Serializers;
using ProudNetSrc.Serialization.Serializers;

namespace NeoNetsphere.Network.Data.Game
{
    [BlubContract]
    public class PlayerAccountInfoDto
    {
        public PlayerAccountInfoDto()
        {
            Nickname = "";
            DMStats = new DMStatsDto();
            TDStats = new TDStatsDto();
            ChaserStats = new ChaserStatsDto();
            BRStats = new BRStatsDto();
            CPTStats = new CPTStatsDto();
            SiegeStats = new SiegeStatsDto();
        }

        [BlubMember(0)]
        public uint TotalMatches { get; set; }

        [BlubMember(1)]
        public uint Unk1 { get; set; } //uh?

        [BlubMember(2)]
        public uint MatchesWon { get; set; }

        [BlubMember(3)]
        public uint MatchesLost { get; set; }

        [BlubMember(4)]
        public uint MatchesLost2 { get; set; }

        [BlubMember(5)]
        public uint Unk2 { get; set; } 

        [BlubMember(6, typeof(TimeSpanSecondsSerializer))]
        public TimeSpan GameTime { get; set; }

        [BlubMember(7)]
        public bool IsGM { get; set; }

        [BlubMember(8)]
        public uint Unk3 { get; set; } //uh..?

        [BlubMember(9)]
        public byte Level { get; set; }

        [BlubMember(10)]
        public byte Unk4 { get; set; } //uh..? 

        [BlubMember(11)]
        public int TotalExp { get; set; }

        [BlubMember(12)]
        public uint AP { get; set; }

        [BlubMember(13)]
        public uint PEN { get; set; }

        [BlubMember(14)]
        public uint TutorialState { get; set; }

        [BlubMember(15, typeof(StringSerializer))]
        public string Nickname { get; set; }

        [BlubMember(16)]
        public uint Unk5 { get; set; } //gender?

        [BlubMember(17)]
        public DMStatsDto DMStats { get; set; }

        [BlubMember(18)]
        public TDStatsDto TDStats { get; set; }

        [BlubMember(19)]
        public ChaserStatsDto ChaserStats { get; set; }

        [BlubMember(20)]
        public BRStatsDto BRStats { get; set; }

        [BlubMember(21)]
        public CPTStatsDto CPTStats { get; set; }

        [BlubMember(22)]
        public SiegeStatsDto SiegeStats { get; set; }

        [BlubMember(23)]
        public uint Unk6 { get; set; }

        [BlubMember(24)]
        public uint Unk7 { get; set; }

        [BlubMember(25)]
        public uint Unk8 { get; set; }

        [BlubMember(26)]
        public uint Unk9 { get; set; }

        [BlubMember(27)]
        public uint Unk10 { get; set; }
    }
}
