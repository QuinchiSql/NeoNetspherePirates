using BlubLib.Serialization;
using ProudNetSrc.Serialization.Serializers;

namespace NeoNetsphere.Network.Data.Game
{
    [BlubContract]
    public class MakeRoomDto
    {
        public MakeRoomDto()
        {
            rName = "";
            rPassword = "";
        }

        [BlubMember(0)]
        public int GameRule { get; set; }

        [BlubMember(1)]
        public byte Map_ID { get; set; }

        [BlubMember(2)]
        public byte Player_Limit { get; set; }

        [BlubMember(3)]
        public short Points { get; set; }

        [BlubMember(4)]
        public byte Time { get; set; }

        [BlubMember(5)]
        public int Weapon_Limit { get; set; }

        [BlubMember(6, typeof(StringSerializer))]
        public string rName { get; set; }

        [BlubMember(7, typeof(StringSerializer))]
        public string rPassword { get; set; }

        [BlubMember(8)]
        public byte Spectator { get; set; }

        [BlubMember(9)]
        public byte SpectatorCount { get; set; }

        [BlubMember(10)]
        public long mUnknow01 { get; set; }

        [BlubMember(11)]
        public short mUnknow02 { get; set; }

        [BlubMember(12)]
        public int mUnknow03 { get; set; }

        [BlubMember(13)]
        public int FMBURNMode { get; set; }

#if LATESTS4
        [BlubMember(14)]
        public int ServerKey { get; set; }
#endif
    }
}
