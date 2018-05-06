using BlubLib.Serialization;
using NeoNetsphere.Network.Serializers;
using ProudNetSrc.Serialization;

namespace NeoNetsphere.Network.Message.Chat
{
    public interface IChatMessage
    {
    }

    public class ChatMessageFactory : MessageFactory<ChatOpCode, IChatMessage>
    {
        static ChatMessageFactory()
        {
            Serializer.AddCompiler(new ItemNumberSerializer());
        }

        public ChatMessageFactory()
        {
            // S2C
            Register<LoginAckMessage>(ChatOpCode.LoginAck);
            Register<FriendActionAckMessage>(ChatOpCode.FriendActionAck);
            Register<FriendListAckMessage>(ChatOpCode.FriendListAck);
            Register<CombiActionAckMessage>(ChatOpCode.CombiActionAck);
            Register<CombiListAckMessage>(ChatOpCode.CombiListAck);
            Register<CombiCheckNameAckMessage>(ChatOpCode.CombiCheckNameAck);
            Register<DenyActionAckMessage>(ChatOpCode.DenyActionAck);
            Register<DenyListAckMessage>(ChatOpCode.DenyListAck);
            Register<ChannelPlayerListAckMessage>(ChatOpCode.ChannelPlayerListAck);
            Register<ChannelEnterPlayerAckMessage>(ChatOpCode.ChannelEnterPlayerAck);
            Register<ChannelLeavePlayerAckMessage>(ChatOpCode.ChannelLeavePlayerAck);
            Register<MessageChatAckMessage>(ChatOpCode.MessageChatAck);
            Register<MessageWhisperChatAckMessage>(ChatOpCode.MessageWhisperChatAck);
            Register<RoomInvitationPlayerAckMessage>(ChatOpCode.RoomInvitationPlayerAck);
            Register<ClanMemberListAckMessage>(ChatOpCode.ClanMemberListAck);
            Register<NoteListAckMessage>(ChatOpCode.NoteListAck);
            Register<NoteSendAckMessage>(ChatOpCode.NoteSendAck);
            Register<NoteReadAckMessage>(ChatOpCode.NoteReadAck);
            Register<NoteDeleteAckMessage>(ChatOpCode.NoteDeleteAck);
            Register<NoteErrorAckMessage>(ChatOpCode.NoteErrorAck);
            Register<NoteCountAckMessage>(ChatOpCode.NoteCountAck);
            Register<ChatPlayerInfoAckMessage>(ChatOpCode.PlayerInfoAck);
            Register<PlayerPositionAckMessage>(ChatOpCode.PlayerPositionAck);
            Register<PlayerPlayerInfoListAckMessage>(ChatOpCode.PlayerPlayerInfoListAck);
            Register<UserDataTwoReqMessage>(ChatOpCode.UserDataTwoReq);
            Register<UserDataFourAckMessage>(ChatOpCode.UserDataFourAck);
            Register<ClanChangeNoticeAckMessage>(ChatOpCode.ClanChangeNoticeAck);
            Register<NoteRejectImportuneAckMessage>(ChatOpCode.NoteRejectImportuneAck);
            Register<ClubSystemMessageMessage>(ChatOpCode.ClubSystemMessage);
            Register<ClubNewsRemindMessage>(ChatOpCode.ClubNewsRemind);
            Register<ClubNoteSendAckMessage>(ChatOpCode.ClubNoteSendAck);
            Register<ClubMemberListAckMessage>(ChatOpCode.ClubMemberListAck);
            Register<ClubMemberLoginStateAckMessage>(ChatOpCode.ClubMemberLoginStateAck);
            Register<Chennel_PlayerNameTagList_AckMessage>(ChatOpCode.Chennel_PlayerNameTagList_Ack);

            // C2S
            Register<LoginReqMessage>(ChatOpCode.LoginReq);
            Register<DenyActionReqMessage>(ChatOpCode.DenyActionReq);
            Register<FriendActionReqMessage>(ChatOpCode.FriendActionReq);
            Register<CombiCheckNameReqMessage>(ChatOpCode.CombiCheckNameReq);
            Register<CombiActionReqMessage>(ChatOpCode.CombiActionReq);
            Register<UserDataOneReqMessage>(ChatOpCode.UserDataOneReq);
            Register<UserDataThreeAckMessage>(ChatOpCode.UserDataThreeAck);
            Register<MessageChatReqMessage>(ChatOpCode.MessageChatReq);
            Register<MessageWhisperChatReqMessage>(ChatOpCode.MessageWhisperChatReq);
            Register<RoomInvitationPlayerReqMessage>(ChatOpCode.RoomInvitationPlayerReq);
            Register<NoteListReqMessage>(ChatOpCode.NoteListReq);
            Register<NoteSendReqMessage>(ChatOpCode.NoteSendReq);
            Register<NoteReadReqMessage>(ChatOpCode.NoteReadReq);
            Register<NoteDeleteReqMessage>(ChatOpCode.NoteDeleteReq);
            Register<NoteCountReqMessage>(ChatOpCode.NoteCountReq);
            Register<OptionSaveCommunityReqMessage>(ChatOpCode.OptionSaveCommunityReq);
            Register<OptionSaveBinaryReqMessage>(ChatOpCode.OptionSaveBinaryReq);
            Register<NoteRejectImportuneReqMessage>(ChatOpCode.NoteRejectImportuneReq);
            Register<ClubNoteSendReqMessage>(ChatOpCode.ClubNoteSendReq);
            Register<ClubMemberListReqMessage>(ChatOpCode.ClubMemberListReq);
#if NEWIDS
            //S2C
            Register<ClubClubMemberInfoAck2Message>(ChatOpCode.Club_ClubMemberInfo_Ack_2);
            Register<ClubMemberListAck2Message>(ChatOpCode.Club_Member_List_Ack_2);


            //C2S
            Register<ClubClubMemberInfoReq2Message>(ChatOpCode.Club_ClubMemberInfo_Req_2);
            Register<ClubMemberListReq2Message>(ChatOpCode.Club_Member_List_Req_2);
            Register<ClubNoteSendReq2Message>(ChatOpCode.Club_Note_Send_Req_2);
            Register<ChannellistReqMessage>(ChatOpCode.Channel_List_Req);
#endif
        }
    }
}
