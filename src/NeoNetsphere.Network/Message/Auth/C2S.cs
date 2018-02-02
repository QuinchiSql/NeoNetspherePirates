using BlubLib.Serialization;
using ProudNetSrc.Serialization.Serializers;

namespace NeoNetsphere.Network.Message.Auth
{
    [BlubContract]
    public class LoginKRReqMessage : IAuthMessage
    {
        [BlubMember(0, typeof(StringSerializer))]
        public string AccountIdOne { get; set; }

        [BlubMember(1, typeof(StringSerializer))]
        public string Unk { get; set; } // -> "_"

        [BlubMember(2, typeof(StringSerializer))]
        public string Unk1 { get; set; }

        [BlubMember(3, typeof(StringSerializer))]
        public string AccountHashCode { get; set; }

        [BlubMember(4)]
        public int AccountIdOne_2 { get; set; }

        [BlubMember(5)]
        public int AccountTwo { get; set; }

        [BlubMember(6)]
        public int Unk5 { get; set; }

        [BlubMember(7, typeof(StringSerializer))]
        public string Unk6 { get; set; }

        [BlubMember(8)]
        public int Unk7 { get; set; }

        [BlubMember(9, typeof(StringSerializer))]
        public string Unk8 { get; set; }

        [BlubMember(10, typeof(StringSerializer))]
        public string Unk9 { get; set; }

        [BlubMember(11, typeof(StringSerializer))]
        public string token { get; set; }

        [BlubMember(12, typeof(StringSerializer))]
        public string AuthToken { get; set; }

        [BlubMember(13, typeof(StringSerializer))]
        public string NewToken { get; set; }

        [BlubMember(14, typeof(StringSerializer))]
        public string DataTime { get; set; }

        //[BlubMember(15, typeof(StringSerializer))]
        //public string unk13 { get; set; }
        //
        //[BlubMember(16, typeof(StringSerializer))]
        //public string unk14 { get; set; }
    }

    [BlubContract]
    public class LoginEUReqMessage : IAuthMessage
    {
        [BlubMember(0, typeof(StringSerializer))]
        public string Username { get; set; }

        [BlubMember(1, typeof(StringSerializer))]
        public string Password { get; set; }

        [BlubMember(2, typeof(StringSerializer))]
        public string Unk1 { get; set; }

        [BlubMember(3, typeof(StringSerializer))]
        public string Unk2 { get; set; }

        [BlubMember(4)]
        public int Unk3 { get; set; }

        [BlubMember(5)]
        public int Unk4 { get; set; }

        [BlubMember(6)]
        public int Unk5 { get; set; }

        [BlubMember(7, typeof(StringSerializer))]
        public string Unk6 { get; set; }

        [BlubMember(8)]
        public int Unk7 { get; set; }

        [BlubMember(9, typeof(StringSerializer))]
        public string Unk8 { get; set; }

        [BlubMember(10, typeof(StringSerializer))]
        public string Unk9 { get; set; }

        [BlubMember(11, typeof(StringSerializer))]
        public string token { get; set; }

        [BlubMember(12, typeof(StringSerializer))]
        public string AuthToken { get; set; }

        [BlubMember(13, typeof(StringSerializer))]
        public string NewToken { get; set; }

        [BlubMember(14, typeof(StringSerializer))]
        public string DataTime { get; set; }

        //[BlubMember(15, typeof(StringSerializer))]
        //public string unk13 { get; set; }
        //
        //[BlubMember(16, typeof(StringSerializer))]
        //public string unk14 { get; set; }
    }

    [BlubContract]
    public class ServerListReqMessage : IAuthMessage
    {
    }

    [BlubContract]
    public class GameDataReqMessage : IAuthMessage
    {
    }

    [BlubContract]
    public class OptionVersionCheckReqMessage : IAuthMessage
    {
        [BlubMember(0)]
        public ulong AccountId { get; set; }

        [BlubMember(1)]
        public uint Checksum { get; set; }
    }
}
