using BlubLib.Serialization;
using ProudNetSrc.Serialization.Serializers;

namespace NeoNetsphere.Network.Data.Chat
{
    [BlubContract]
    public class ClubMemberDto
    {
        public ClubMemberDto(ulong accountId, string nickname, int serverid, int channelid, int roomid)
        {
            AccountId = accountId;
            Nickname = nickname;
            ServerId = serverid;
            ChannelId = channelid;
            RoomId = roomid;
        }

        public ClubMemberDto()
        {
            Nickname = "";
            JoinDate = "";
            LastLogin = "";
            ServerId = -1;
            ChannelId = -1;
            RoomId = -1;
            
            /* ClanRank
             1 - Master
             2 - Temp Master
             3 - Staff
             4 - Regular
             5 - Normal
             6 - Bad-Manner
             7 - A Class
             8 - B Class
             9 - C Class
            */


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
        public int ClanRank { get; set; }

        [BlubMember(5)]
        public int Unk4 { get; set; }

        [BlubMember(6)]
        public int Unk5 { get; set; }

        [BlubMember(7, typeof(StringSerializer))]
        public string JoinDate { get; set; }

        [BlubMember(8)]
        public int Unk7 { get; set; }

        [BlubMember(9, typeof(StringSerializer))]
        public string LastLogin { get; set; }

        [BlubMember(10)]
        public int ServerId { get; set; }

        [BlubMember(11)]
        public int ChannelId { get; set; }

        [BlubMember(12)]
        public int RoomId { get; set; }

        [BlubMember(13)]
        public int Unk11 { get; set; }

        [BlubMember(14)]
        public int Unk12 { get; set; }
    }
}
