using BlubLib.Serialization;
using ProudNetSrc.Serialization.Serializers;

namespace NeoNetsphere.Network.Data.Chat
{
    [BlubContract]
    public class ClubMemberDto
    {
        public ClubMemberDto(ulong accountId, string nickname, int serverid)
        {
            AccountId = accountId;
            Nickname = nickname;
            Unk4 = serverid;
        }

        public ClubMemberDto()
        {
            Nickname = "";
            Unk5 = -1;
            Unk6 = -1;
        }

        [BlubMember(0)]
        public ulong AccountId { get; set; }

        [BlubMember(1, typeof(StringSerializer))]
        public string Nickname { get; set; }

        [BlubMember(2)]
        public int Unk1 { get; set; }

        [BlubMember(3)]
        public int Unk2 { get; set; }

        [BlubMember(4)]
        public byte Unk3 { get; set; }

        [BlubMember(5)]
        public int Unk4 { get; set; } //serverid?

        [BlubMember(6)]
        public int Unk5 { get; set; }

        [BlubMember(7)]
        public int Unk6 { get; set; }

        [BlubMember(8)]
        public int Unk7 { get; set; }

        [BlubMember(9)]
        public int Unk8 { get; set; }

        [BlubMember(10)]
        public int Unk9 { get; set; }
    }
}
