using BlubLib.Serialization;

namespace NeoNetsphere.Network.Data.Chat
{
    [BlubContract]
    public class ClubMemberDto
    {
        public ClubMemberDto(ulong accountId, string nickname)
        {
            AccountId = accountId;
            Nickname = nickname;
        }

        public ClubMemberDto()
        {
            Nickname = "";
            Unk7 = "";
            Unk8 = "";
        }

        [BlubMember(0)]
        public ulong AccountId { get; set; }

        [BlubMember(1)]
        public string Nickname { get; set; }

        [BlubMember(2)]
        public int Unk3 { get; set; }

        [BlubMember(3)]
        public int Unk4 { get; set; }

        [BlubMember(4)]
        public int Unk5 { get; set; }

        [BlubMember(5)]
        public int Unk6 { get; set; }

        [BlubMember(6)]
        public string Unk7 { get; set; }

        [BlubMember(7)]
        public string Unk8 { get; set; }

        [BlubMember(8)]
        public int Unk9 { get; set; }
    }
}
