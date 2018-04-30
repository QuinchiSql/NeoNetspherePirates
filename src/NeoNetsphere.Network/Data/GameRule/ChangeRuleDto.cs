using BlubLib.Serialization;
using ProudNetSrc.Serialization.Serializers;

namespace NeoNetsphere.Network.Data.GameRule
{
    [BlubContract]
    public class ChangeRuleDto
    {
        public ChangeRuleDto()
        {
            Name = "";
            Password = "";
        }

        [BlubMember(0)]
        public int GameRule { get; set; }

        [BlubMember(1)]
        public byte Map_ID { get; set; }

        [BlubMember(2)]
        public byte Player_Limit { get; set; }

        [BlubMember(3)]
        public ushort Points { get; set; }

        [BlubMember(4)]
        public int Unk1 { get; set; } //Time

        [BlubMember(5)]
        public byte Time { get; set; }

        [BlubMember(6)]
        public int Weapon_Limit { get; set; }

        [BlubMember(7, typeof(StringSerializer))]
        public string Password { get; set; }

        [BlubMember(8, typeof(StringSerializer))]
        public string Name { get; set; }

        [BlubMember(9)]
        public bool HasSpectator { get; set; }

        [BlubMember(10)]
        public byte SpectatorLimit { get; set; }

        [BlubMember(11)]
        public byte Unk3 { get; set; }

        [BlubMember(12)]
        public int Unk4 { get; set; } //Time

        [BlubMember(13)]
        public int Unk5 { get; set; }

        [BlubMember(14)]
        public byte Unk6 { get; set; }// = 0x10;
    }

    [BlubContract]
    public class ChangeRuleDto2
    {
        public ChangeRuleDto2()
        {
            Name = "";
            Password = "";
        }

        [BlubMember(0)]
        public int GameRule { get; set; }

        [BlubMember(1)]
        public byte Map_ID { get; set; }

        [BlubMember(2)]
        public byte Player_Limit { get; set; }

        [BlubMember(3)]
        public ushort Points { get; set; }

        [BlubMember(4)]
        public int Unk1 { get; set; } //Time

        [BlubMember(5)]
        public byte Time { get; set; }

        [BlubMember(6)]
        public int Weapon_Limit { get; set; }

        [BlubMember(7, typeof(StringSerializer))]
        public string Password { get; set; }

        [BlubMember(8, typeof(StringSerializer))]
        public string Name { get; set; }

        [BlubMember(9)]
        public bool HasSpectator { get; set; }

        [BlubMember(10)]
        public byte SpectatorLimit { get; set; }

        [BlubMember(11)]
        public byte Unk3 { get; set; }

        [BlubMember(12)]
        public int Unk4 { get; set; } //Time

        [BlubMember(13)]
        public int Unk5 { get; set; }

        [BlubMember(14)]
        public byte Unk6 { get; set; }// = 0x10;

        [BlubMember(15)]
        public int Unk7 { get; set; }

        [BlubMember(16)]
        public int FMBurnMode { get; set; }

        [BlubMember(17)]
        public int Unk8 { get; set; }

        [BlubMember(18)]
        public byte Unk9 { get; set; }// = 0xFF;
    }
}
