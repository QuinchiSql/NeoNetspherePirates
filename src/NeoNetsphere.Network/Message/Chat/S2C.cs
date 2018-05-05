using System;
using BlubLib.Serialization;
using NeoNetsphere.Network.Data.Chat;
using NeoNetsphere.Network.Serializers;
using ProudNetSrc.Serialization.Serializers;

namespace NeoNetsphere.Network.Message.Chat
{
    [BlubContract]
    public class LoginAckMessage : IChatMessage
    {
        public LoginAckMessage()
        {
        }

        public LoginAckMessage(uint result)
        {
            Result = result;
        }

        [BlubMember(0)]
        public uint Result { get; set; }
    }

    [BlubContract]
    public class FriendActionAckMessage : IChatMessage
    {
        public FriendActionAckMessage()
        {
            Friend = new FriendDto();
        }

        public FriendActionAckMessage(int result)
            : this()
        {
            Result = result;
        }

        public FriendActionAckMessage(int result, int unk, FriendDto friend)
        {
            Result = result;
            Unk = unk;
            Friend = friend;
        }

        [BlubMember(0)]
        public int Result { get; set; }

        [BlubMember(1)]
        public int Unk { get; set; }

        [BlubMember(2)]
        public FriendDto Friend { get; set; }
    }

    [BlubContract]
    public class FriendListAckMessage : IChatMessage
    {
        public FriendListAckMessage()
        {
            Friends = Array.Empty<FriendDto>();
        }

        public FriendListAckMessage(FriendDto[] friends)
        {
            Friends = friends;
        }

        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public FriendDto[] Friends { get; set; }
    }

    [BlubContract]
    public class CombiActionAckMessage : IChatMessage
    {
        public CombiActionAckMessage()
        {
            Combi = new CombiDto();
        }

        public CombiActionAckMessage(int result, int unk, CombiDto combi)
        {
            Result = result;
            Unk = unk;
            Combi = combi;
        }

        [BlubMember(0)]
        public int Result { get; set; }

        [BlubMember(1)]
        public int Unk { get; set; }

        [BlubMember(2)]
        public CombiDto Combi { get; set; }
    }

    [BlubContract]
    public class CombiListAckMessage : IChatMessage
    {
        public CombiListAckMessage()
        {
            Combies = Array.Empty<CombiDto>();
        }

        public CombiListAckMessage(CombiDto[] combies)
        {
            Combies = combies;
        }

        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public CombiDto[] Combies { get; set; }
    }

    [BlubContract]
    public class CombiCheckNameAckMessage : IChatMessage
    {
        public CombiCheckNameAckMessage()
        {
            Unk2 = "";
        }

        public CombiCheckNameAckMessage(uint unk1, string unk2)
        {
            Unk1 = unk1;
            Unk2 = unk2;
        }

        [BlubMember(0)]
        public uint Unk1 { get; set; }

        [BlubMember(1, typeof(StringSerializer))]
        public string Unk2 { get; set; }
    }

    [BlubContract]
    public class DenyActionAckMessage : IChatMessage
    {
        public DenyActionAckMessage()
        {
            Deny = new DenyDto();
        }

        public DenyActionAckMessage(int result, DenyAction action, DenyDto deny)
        {
            Result = result;
            Action = action;
            Deny = deny;
        }

        [BlubMember(0)]
        public int Result { get; set; }

        [BlubMember(1)]
        public DenyAction Action { get; set; }

        [BlubMember(2)]
        public DenyDto Deny { get; set; }
    }

    [BlubContract]
    public class DenyListAckMessage : IChatMessage
    {
        public DenyListAckMessage()
        {
            Denies = Array.Empty<DenyDto>();
        }

        public DenyListAckMessage(DenyDto[] denies)
        {
            Denies = denies;
        }

        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public DenyDto[] Denies { get; set; }
    }

    [BlubContract]
    public class ChannelPlayerListAckMessage : IChatMessage
    {
        public ChannelPlayerListAckMessage()
        {
            UserData = Array.Empty<PlayerInfoShortDto>();
        }

        public ChannelPlayerListAckMessage(PlayerInfoShortDto[] userData)
        {
            UserData = userData;
        }

        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public PlayerInfoShortDto[] UserData { get; set; }
    }

    [BlubContract]
    public class ChannelEnterPlayerAckMessage : IChatMessage
    {
        public ChannelEnterPlayerAckMessage()
        {
            UserData = new PlayerInfoShortDto();
        }

        public ChannelEnterPlayerAckMessage(PlayerInfoShortDto userData)
        {
            UserData = userData;
        }

        [BlubMember(0)]
        public PlayerInfoShortDto UserData { get; set; }
    }

    [BlubContract]
    public class ChannelLeavePlayerAckMessage : IChatMessage
    {
        public ChannelLeavePlayerAckMessage()
        {
        }

        public ChannelLeavePlayerAckMessage(ulong accountId)
        {
            AccountId = accountId;
        }

        [BlubMember(0)]
        public ulong AccountId { get; set; }
    }

    [BlubContract]
    public class MessageChatAckMessage : IChatMessage
    {
        public MessageChatAckMessage()
        {
            Nickname = "";
            Message = "";
        }

        public MessageChatAckMessage(ChatType chatType, ulong accountId, string nick, string message)
        {
            ChatType = chatType;
            AccountId = accountId;
            Nickname = nick;
            Message = message;
        }

        [BlubMember(0)]
        public ChatType ChatType { get; set; }

        [BlubMember(1)]
        public ulong AccountId { get; set; }

        [BlubMember(2, typeof(StringSerializer))]
        public string Nickname { get; set; }

        [BlubMember(3, typeof(StringSerializer))]
        public string Message { get; set; }
    }

    [BlubContract]
    public class MessageWhisperChatAckMessage : IChatMessage
    {
        public MessageWhisperChatAckMessage()
        {
            ToNickname = "";
            Nickname = "";
            Message = "";
        }

        public MessageWhisperChatAckMessage(uint unk, string toNickname, ulong accountId, string nick, string message)
        {
            Unk = unk;
            ToNickname = toNickname;
            AccountId = accountId;
            Nickname = nick;
            Message = message;
        }

        [BlubMember(0)]
        public uint Unk { get; set; }

        [BlubMember(1, typeof(StringSerializer))]
        public string ToNickname { get; set; }

        [BlubMember(2)]
        public ulong AccountId { get; set; }

        [BlubMember(3, typeof(StringSerializer))]
        public string Nickname { get; set; }

        [BlubMember(4, typeof(StringSerializer))]
        public string Message { get; set; }
    }

    [BlubContract]
    public class RoomInvitationPlayerAckMessage : IChatMessage
    {
        public RoomInvitationPlayerAckMessage()
        {
            Unk2 = "";
            Location = new PlayerLocationDto();
        }

        public RoomInvitationPlayerAckMessage(ulong unk1, string unk2, PlayerLocationDto location)
        {
            Unk1 = unk1;
            Unk2 = unk2;
            Location = location;
        }

        [BlubMember(0)]
        public ulong Unk1 { get; set; }

        [BlubMember(1, typeof(StringSerializer))]
        public string Unk2 { get; set; }

        [BlubMember(2)]
        public PlayerLocationDto Location { get; set; }
    }

    [BlubContract]
    public class ClanMemberListAckMessage : IChatMessage
    {
        public ClanMemberListAckMessage()
        {
            Players = Array.Empty<PlayerInfoDto>();
        }

        public ClanMemberListAckMessage(PlayerInfoDto[] players)
        {
            Players = players;
        }

        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public PlayerInfoDto[] Players { get; set; }
    }

    [BlubContract]
    public class NoteListAckMessage : IChatMessage
    {
        public NoteListAckMessage()
        {
            Notes = Array.Empty<NoteDto>();
        }

        public NoteListAckMessage(int pageCount, byte currentPage, NoteDto[] notes)
        {
            PageCount = pageCount;
            CurrentPage = currentPage;
            Unk3 = 7;
            Notes = notes;
        }

        [BlubMember(0)]
        public int PageCount { get; set; }

        [BlubMember(1)]
        public byte CurrentPage { get; set; }

        [BlubMember(2)]
        public int Unk3 { get; set; } // MessageType? - MessageType UI does not exist in this version

        [BlubMember(3, typeof(ArrayWithIntPrefixSerializer))]
        public NoteDto[] Notes { get; set; }
    }

    [BlubContract]
    public class NoteSendAckMessage : IChatMessage
    {
        public NoteSendAckMessage()
        {
        }

        public NoteSendAckMessage(int result)
        {
            Result = result;
        }

        [BlubMember(0)]
        public int Result { get; set; }
    }

    [BlubContract]
    public class NoteReadAckMessage : IChatMessage
    {
        public NoteReadAckMessage()
        {
            Note = new NoteContentDto();
        }

        public NoteReadAckMessage(ulong id, NoteContentDto note, int unk)
        {
            Id = id;
            Note = note;
            Unk = unk;
        }

        [BlubMember(0)]
        public ulong Id { get; set; }

        [BlubMember(1)]
        public NoteContentDto Note { get; set; }

        [BlubMember(2)]
        public int Unk { get; set; }
    }

    [BlubContract]
    public class NoteDeleteAckMessage : IChatMessage
    {
        public NoteDeleteAckMessage()
        {
            Notes = Array.Empty<DeleteNoteDto>();
        }

        public NoteDeleteAckMessage(DeleteNoteDto[] notes)
        {
            Notes = notes;
        }

        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public DeleteNoteDto[] Notes { get; set; }
    }

    [BlubContract]
    public class NoteErrorAckMessage : IChatMessage
    {
        public NoteErrorAckMessage()
        {
        }

        public NoteErrorAckMessage(int unk)
        {
            Unk = unk;
        }

        [BlubMember(0)]
        public int Unk { get; set; }
    }

    [BlubContract]
    public class NoteCountAckMessage : IChatMessage
    {
        public NoteCountAckMessage()
        {
        }

        public NoteCountAckMessage(byte noteCount, byte unk2, byte unk3)
        {
            NoteCount = noteCount;
            Unk2 = unk2;
            Unk3 = unk3;
        }

        [BlubMember(0)]
        public byte NoteCount { get; set; }

        [BlubMember(1)]
        public byte Unk2 { get; set; }

        [BlubMember(2)]
        public byte Unk3 { get; set; }
    }

    [BlubContract]
    public class PlayerInfoAckMessage : IChatMessage
    {
        public PlayerInfoAckMessage()
        {
        }

        public PlayerInfoAckMessage(PlayerInfoDto player)
        {
            Player = player;
        }

        [BlubMember(0)]
        public PlayerInfoDto Player { get; set; }
    }

    [BlubContract]
    public class PlayerPositionAckMessage : IChatMessage
    {
        public PlayerPositionAckMessage()
        {
        }

        public PlayerPositionAckMessage(ulong accountId, PlayerInfoDto player)
        {
            AccountId = accountId;
            Player = player;
        }

        [BlubMember(0)]
        public ulong AccountId { get; set; }

        [BlubMember(1)]
        public PlayerInfoDto Player { get; set; }
    }

    [BlubContract]
    public class PlayerPlayerInfoListAckMessage : IChatMessage
    {
        public PlayerPlayerInfoListAckMessage()
        {
            Players = Array.Empty<PlayerInfoDto>();
        }

        public PlayerPlayerInfoListAckMessage(PlayerInfoDto[] players)
        {
            Players = players;
        }

        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public PlayerInfoDto[] Players { get; set; }
    }

    [BlubContract]
    public class UserDataTwoReqMessage : IChatMessage
    {
        [BlubMember(0)]
        public ulong AccountId { get; set; }

        [BlubMember(1)]
        public int Unk { get; set; }
    }

    [BlubContract]
    public class UserDataFourAckMessage : IChatMessage
    {
        public UserDataFourAckMessage()
        {
        }

        public UserDataFourAckMessage(int unk, UserDataDto userData)
        {
            Unk = unk;
            UserData = userData;
        }

        [BlubMember(0)]
        public int Unk { get; set; }

        [BlubMember(1)]
        public UserDataDto UserData { get; set; }
    }

    [BlubContract]
    public class ClanChangeNoticeAckMessage : IChatMessage
    {
        [BlubMember(0)]
        public string Unk { get; set; }
    }

    [BlubContract]
    public class NoteRejectImportuneAckMessage : IChatMessage
    {
        [BlubMember(0)]
        public int Unk { get; set; }
    }

    [BlubContract]
    public class ClubSystemMessageMessage : IChatMessage
    {
        [BlubMember(0)]
        public ulong AccountId { get; set; }

        [BlubMember(1)]
        public string Message { get; set; }

        public ClubSystemMessageMessage()
        {
            AccountId = 0;
            Message = "";
        }

        public ClubSystemMessageMessage(ulong accountId, string message)
        {
            AccountId = accountId;
            Message = message;
        }
    }

    [BlubContract]
    public class ClubNewsRemindMessage : IChatMessage
    {
        [BlubMember(0)]
        public ulong Unk { get; set; }
    }

    [BlubContract]
    public class ClubNoteSendAckMessage : IChatMessage
    {
        [BlubMember(0)]
        public int Unk { get; set; }
    }

    [BlubContract]
    public class ClubMemberListAckMessage : IChatMessage
    {
        [BlubMember(0)]
        public uint ClanId { get; set; }

        [BlubMember(1, typeof(ArrayWithIntPrefixSerializer))]
        public ClubMemberDto[] Members { get; set; }

        public ClubMemberListAckMessage()
        {
            Members = Array.Empty<ClubMemberDto>();
        }

        public ClubMemberListAckMessage(uint clanId, ClubMemberDto[] members)
        {
            ClanId = clanId;
            Members = members;
        }
    }

    [BlubContract]
    public class ClubMemberListAck2Message : IChatMessage
    {
        [BlubMember(0)]
        public uint ClanId { get; set; }

        [BlubMember(1, typeof(ArrayWithIntPrefixSerializer))]
        public ClubMemberDto[] Members { get; set; }

        public ClubMemberListAck2Message()
        {
            Members = Array.Empty<ClubMemberDto>();
        }

        public ClubMemberListAck2Message(uint clanId, ClubMemberDto[] members)
        {
            ClanId = clanId;
            Members = members;
        }
    }

    [BlubContract]
    public class ClubMemberLoginStateAckMessage : IChatMessage
    {
        [BlubMember(0)]
        public int State { get; set; }

        [BlubMember(1)]
        public ulong AccountId { get; set; }

        public ClubMemberLoginStateAckMessage()
        {
            State = 0;
            AccountId = 0;
        }

        public ClubMemberLoginStateAckMessage(int state, ulong accountId)
        {
            State = state;
            AccountId = accountId;
        }
    }

    [BlubContract]
    public class Chennel_PlayerNameTagList_AckMessage : IChatMessage
    {
        public Chennel_PlayerNameTagList_AckMessage()
        {
            UserData = Array.Empty<PlayerNameTagInfoDto>();
        }

        public Chennel_PlayerNameTagList_AckMessage(PlayerNameTagInfoDto[] userData)
        {
            UserData = userData;
        }

        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public PlayerNameTagInfoDto[] UserData { get; set; }
    }

    [BlubContract]
    public class ClubClubMemberInfoAck2Message : IChatMessage
    {
        [BlubMember(0)]
        public uint ClanId { get; set; }

        [BlubMember(1)]
        public ulong AccountId { get; set; }

        [BlubMember(2, typeof(StringSerializer))]
        public string Nickname { get; set; }

        [BlubMember(3)]
        public int Unk1 { get; set; }

        [BlubMember(4)]
        public int Unk2 { get; set; }

        [BlubMember(5)]
        public int IsModerator { get; set; } 

        [BlubMember(6)]
        public int Unk4 { get; set; }

        [BlubMember(7)]
        public int Unk5 { get; set; }

        [BlubMember(8, typeof(StringSerializer))]
        public string JoinDate { get; set; }

        [BlubMember(9)]
        public int Unk6 { get; set; } //MemberCount? MemberIndex?

        [BlubMember(10, typeof(StringSerializer))]
        public string Unk7 { get; set; }

        [BlubMember(11)]
        public long Unk8 { get; set; }

        [BlubMember(12)]
        public int Unk9 { get; set; }

        [BlubMember(13)]
        public int Unk10 { get; set; }

        public ClubClubMemberInfoAck2Message()
        {
            Unk1 = 0;
            IsModerator = 0;
            Unk6 = 0;
            Nickname = "";
            JoinDate = "";
            Unk7 = "";
            Unk8 = -1;
        }
    }
}
