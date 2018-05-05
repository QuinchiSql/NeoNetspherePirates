using BlubLib.Serialization;
using ProudNetSrc.Serialization.Serializers;

namespace NeoNetsphere.Network.Data.Club
{
    [BlubContract]
    public class MyInfoDto
    {
        [BlubMember(0)]
        public uint Id { get; set; } //unique club id

        [BlubMember(1, typeof(StringSerializer))]
        public string Type { get; set; } // 0-0-0 Background - Stroke - Inner-Pattern

        [BlubMember(2, typeof(StringSerializer))]
        public string Name { get; set; } //name

        [BlubMember(3)]
        public ClubState State { get; set; } //0 not joined, 1 awaiting acception, 2 joined

        [BlubMember(4)]
        public int Unk1 { get; set; }

        [BlubMember(5)]
        public int Level { get; set; } //?

        [BlubMember(6)]
        public int Unk2 { get; set; }

        [BlubMember(7)]
        public int Unk3 { get; set; }

        [BlubMember(8)]
        public int Unk4 { get; set; }

        [BlubMember(9)]
        public long Unk5 { get; set; }

        [BlubMember(10)]
        public int Unk6 { get; set; } //rank?

        [BlubMember(11)]
        public byte Unk7 { get; set; }

        public MyInfoDto()
        {
            Type = "";
            Id = 0;
            State = 0;
            Level = 4;
            Unk4 = 1;
            Unk5 = -1;
            Unk6 = 449678389;
        }
    }
}
