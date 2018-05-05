﻿using System;
using BlubLib.Serialization;
using NeoNetsphere.Network.Data.Chat;
using NeoNetsphere.Network.Data.Club;
using NeoNetsphere.Network.Serializers;
using ProudNetSrc.Serialization.Serializers;

namespace NeoNetsphere.Network.Message.Club
{
    [BlubContract]
    public class ClubCreateAckMessage : IClubMessage
    {
        public ClubCreateAckMessage()
        {
        }

        public ClubCreateAckMessage(int unk)
        {
            Unk = unk;
        }

        [BlubMember(0)] public int Unk { get; set; }
    }

    [BlubContract]
    public class ClubCreateAck2Message : IClubMessage
    {
        public ClubCreateAck2Message()
        {
        }

        public ClubCreateAck2Message(int unk)
        {
            Unk = unk;
        }

        [BlubMember(0)] public int Unk { get; set; }
    }

    [BlubContract]
    public class ClubCloseAckMessage : IClubMessage
    {
        [BlubMember(0)]
        public int Result { get; set; }
    }

    [BlubContract]
    public class ClubCloseAck2Message : IClubMessage
    {
        [BlubMember(0)]
        public int Result { get; set; }

        public ClubCloseAck2Message()
        {
        }

        public ClubCloseAck2Message(int result)
        {
            Result = result;
        }
    }


    [BlubContract]
    public class ClubJoinAckMessage : IClubMessage
    {
        [BlubMember(0)] public int Unk { get; set; }
    }

    [BlubContract]
    public class ClubUnjoinAckMessage : IClubMessage
    {
        [BlubMember(0)]
        public int Result { get; set; }

        public ClubUnjoinAckMessage()
        {
        }

        public ClubUnjoinAckMessage(int result)
        {
            Result = result;
        }
    }

    [BlubContract]
    public class ClubNameCheckAckMessage : IClubMessage
    {
        public ClubNameCheckAckMessage()
        {
            Unk = 0;
        }

        public ClubNameCheckAckMessage(int unk)
        {
            Unk = unk;
        }

        [BlubMember(0)] public int Unk { get; set; }
    }

    [BlubContract]
    public class ClubRestoreAckMessage : IClubMessage
    {
        [BlubMember(0)] public int Unk { get; set; }
    }

    [BlubContract]
    public class ClubAdminInviteAckMessage : IClubMessage
    {
        [BlubMember(0)] public int Unk { get; set; }
    }

    [BlubContract]
    public class ClubAdminJoinCommandAckMessage : IClubMessage
    {
        [BlubMember(0)] public int Unk1 { get; set; }

        [BlubMember(1, typeof(ArrayWithIntPrefixSerializer))]
        public ulong[] Unk2 { get; set; }
    }

    [BlubContract]
    public class ClubAdminGradeChangeAckMessage : IClubMessage
    {
        [BlubMember(0)] public int Unk1 { get; set; }

        [BlubMember(1, typeof(ArrayWithIntPrefixSerializer))]
        public ulong[] Unk2 { get; set; }
    }

    [BlubContract]
    public class ClubAdminNoticeChangeAckMessage : IClubMessage
    {
        [BlubMember(0)] public int Unk { get; set; }
    }

    [BlubContract]
    public class ClubAdminInfoModifyAckMessage : IClubMessage
    {
        [BlubMember(0)] public int Unk { get; set; }
    }

    [BlubContract]
    public class ClubAdminSubMasterAckMessage : IClubMessage
    {
        [BlubMember(0)] public int Unk { get; set; }
    }

    [BlubContract]
    public class ClubAdminSubMasterCancelAckMessage : IClubMessage
    {
        [BlubMember(0)] public int Unk { get; set; }
    }

    [BlubContract]
    public class ClubAdminMasterChangeAckMessage : IClubMessage
    {
        [BlubMember(0)] public int Unk { get; set; }
    }

    [BlubContract]
    public class ClubAdminJoinConditionModifyAckMessage : IClubMessage
    {
        [BlubMember(0)] public int Unk { get; set; }
    }

    [BlubContract]
    public class ClubAdminBoardModifyAckMessage : IClubMessage
    {
        [BlubMember(0)] public int Unk { get; set; }
    }

    [BlubContract]
    public class ClubSearchAckMessage : IClubMessage
    {
        public ClubSearchAckMessage()
        {
            Clubs = Array.Empty<ClubInfoDto>();
        }

        public ClubSearchAckMessage(int unk1, ClubInfoDto[] clubs)
        {
            Unk1 = unk1;
            Clubs = clubs;
        }

        [BlubMember(0)] public int Unk1 { get; set; }

        [BlubMember(1, typeof(ArrayWithIntPrefixSerializer))]
        public ClubInfoDto[] Clubs { get; set; }
    }

    [BlubContract]
    public class ClubClubInfoAckMessage : IClubMessage
    {
        [BlubMember(0)] public ClubInfoDto Info { get; set; }

        public ClubClubInfoAckMessage()
        {
            Info = new ClubInfoDto();
        }

        public ClubClubInfoAckMessage(ClubInfoDto info)
        {
            Info = info;
        }
    }


    [BlubContract]
    public class ClubClubInfoAck2Message : IClubMessage
    {
        [BlubMember(0)]
        public ClubInfoDto2 Info { get; set; }

        public ClubClubInfoAck2Message()
        {
            Info = new ClubInfoDto2();
        }

        public ClubClubInfoAck2Message(ClubInfoDto2 info)
        {
            Info = info;
        }
    }

    [BlubContract]
    public class ClubJoinWaiterInfoAckMessage : IClubMessage
    {
        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public JoinWaiterInfoDto[] Member { get; set; }

        public ClubJoinWaiterInfoAckMessage()
        {
            Member = Array.Empty<JoinWaiterInfoDto>();
        }

        public ClubJoinWaiterInfoAckMessage(JoinWaiterInfoDto[] member)
        {
            Member = member;
        }
    }

    [BlubContract]
    public class ClubNewJoinMemberInfoAckMessage : IClubMessage
    {
        public ClubNewJoinMemberInfoAckMessage()
        {
            Unk = Array.Empty<ClubMemberInfoDto>();
        }

        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public ClubMemberInfoDto[] Unk { get; set; }
    }

    [BlubContract]
    public class ClubJoinConditionInfoAckMessage : IClubMessage
    {
        [BlubMember(0)] public int Unk1 { get; set; }

        [BlubMember(1)] public int Unk2 { get; set; }

        [BlubMember(2, typeof(StringSerializer))]
        public string Unk3 { get; set; }

        [BlubMember(3, typeof(StringSerializer))]
        public string Unk4 { get; set; }

        [BlubMember(4, typeof(StringSerializer))]
        public string Unk5 { get; set; }

        [BlubMember(5, typeof(StringSerializer))]
        public string Unk6 { get; set; }

        [BlubMember(6, typeof(StringSerializer))]
        public string Unk7 { get; set; }
    }

    [BlubContract]
    public class ClubUnjoinerListAckMessage : IClubMessage
    {
        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public UnjoinerDto[] Unk { get; set; }
    }

    [BlubContract]
    public class ClubUnjoinSettingMemberListAckMessage : IClubMessage
    {
        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public UnjoinSettingMemberDto[] Unk { get; set; }
    }

    [BlubContract]
    public class ClubGradeCountAckMessage : IClubMessage
    {
        [BlubMember(0)] public int Unk1 { get; set; }

        [BlubMember(1)] public int Unk2 { get; set; }

        [BlubMember(2)] public int Unk3 { get; set; }

        [BlubMember(3)] public int Unk4 { get; set; }
    }

    [BlubContract]
    public class ClubStuffListAckMessage : IClubMessage
    {
        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public ClubMemberDto[] Members { get; set; }

        public ClubStuffListAckMessage()
        {
            Members = Array.Empty<ClubMemberDto>();
        }

        public ClubStuffListAckMessage(ClubMemberDto[] members)
        {
            Members = members;
        }
    }
    [BlubContract]
    public class ClubStuffListAck2Message : IClubMessage
    {
        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public ClubMemberDto[] Members { get; set; }

        public ClubStuffListAck2Message()
        {
            Members = Array.Empty<ClubMemberDto>();
        }

        public ClubStuffListAck2Message(ClubMemberDto[] members)
        {
            Members = members;
        }
    }

    [BlubContract]
    public class ClubNewsInfoAckMessage : IClubMessage
    {
        [BlubMember(0)] public int Unk1 { get; set; }

        [BlubMember(1, typeof(StringSerializer))]
        public string Unk2 { get; set; }

        [BlubMember(2, typeof(StringSerializer))]
        public string Unk3 { get; set; }
    }

    [BlubContract]
    public class ClubMyInfoAckMessage : IClubMessage
    {
        [BlubMember(0)] public MyInfoDto Unk { get; set; }

        public ClubMyInfoAckMessage()
        {

        }

        public ClubMyInfoAckMessage(MyInfoDto unk)
        {
            Unk = unk;
        }
    }

    [BlubContract]
    public class ClubBoardWriteAckMessage : IClubMessage
    {
        [BlubMember(0)] public int Unk { get; set; }
    }

    [BlubContract]
    public class ClubBoardReadAckMessage : IClubMessage
    {
        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public BoardMessageDto[] Unk { get; set; }
    }

    [BlubContract]
    public class ClubBoardModifyAckMessage : IClubMessage
    {
        [BlubMember(0)] public int Unk { get; set; }
    }

    [BlubContract]
    public class ClubBoardDeleteAckMessage : IClubMessage
    {
        [BlubMember(0)] public int Unk { get; set; }
    }

    [BlubContract]
    public class ClubBoardDeleteAllAckMessage : IClubMessage
    {
        [BlubMember(0)] public int Unk { get; set; }
    }

    [BlubContract]
    public class ClubBoardReadFailedAckMessage : IClubMessage
    {
        [BlubMember(0)] public int Unk { get; set; }
    }

    [BlubContract]
    public class ClubRankListAckMessage : IClubMessage
    {
        [BlubMember(0)]
        public int TotalClans { get; set; }

        [BlubMember(1, typeof(ArrayWithIntPrefixSerializer))]
        public ClubRankInfoDto[] Clans { get; set; }

        public ClubRankListAckMessage()
        {
            Clans = Array.Empty<ClubRankInfoDto>();
        }

        public ClubRankListAckMessage(int totalClans, ClubRankInfoDto[] clans)
        {
            TotalClans = totalClans;
            Clans = clans;
        }

        //
    }

    [BlubContract]
    public class ClubUnjoinAck2Message : IClubMessage
    {
        [BlubMember(0)]
        public int Result { get; set; }

        public ClubUnjoinAck2Message()
        {  
        }

        public ClubUnjoinAck2Message(int result)
        {
            Result = result;
        }
    }

}
