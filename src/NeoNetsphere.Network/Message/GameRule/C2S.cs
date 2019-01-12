﻿using System;
using BlubLib.Serialization;
using NeoNetsphere.Network.Data.GameRule;
using NeoNetsphere.Network.Serializers;

namespace NeoNetsphere.Network.Message.GameRule
{
    [BlubContract]
    public class RoomEnterPlayerReqMessage : IGameRuleMessage
    {
    }

    [BlubContract]
    public class RoomLeaveReguestReqMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public ulong AccountId { get; set; }

        [BlubMember(1)]
        public RoomLeaveReason Reason { get; set; }
    }

    [BlubContract]
    public class RoomTeamChangeReqMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public Team Team { get; set; }

        [BlubMember(1)]
        public PlayerGameMode Mode { get; set; }
    }

    [BlubContract]
    public class RoomAutoAssingTeamReqMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public byte Unk { get; set; }
    }

    [BlubContract]
    public class RoomAutoMixingTeamReqMessage : IGameRuleMessage
    {
    }

    [BlubContract]
    public class RoomChoiceTeamChangeReqMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public ulong PlayerToMove { get; set; }

        [BlubMember(1)]
        public ulong PlayerToReplace { get; set; }

        [BlubMember(2)]
        public Team FromTeam { get; set; }

        [BlubMember(3)]
        public Team ToTeam { get; set; }
    }

    [BlubContract]
    public class GameEventMessageReqMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public GameEventMessage Event { get; set; }

        [BlubMember(1)]
        public ulong AccountId { get; set; }

        [BlubMember(2)]
        public uint Unk1 { get; set; } // server/game time or something like that

        [BlubMember(3)]
        public ushort Value { get; set; }

        [BlubMember(4)]
        public uint Unk2 { get; set; }
    }

    [BlubContract]
    public class RoomReadyRoundReq2Message : IGameRuleMessage
    {
        //[BlubMember(0)]
        //public bool IsReady { get; set; }
        //UNK, character??
    }

    [BlubContract]
    public class RoomReadyRoundReqMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public bool IsReady { get; set; }
    }

    [BlubContract]
    public class RoomIntrudeRoundReq2Message : IGameRuleMessage
    {
        //UNK, character??
    }

    [BlubContract]
    public class RoomBeginRoundReq2Message : IGameRuleMessage
    {
        //UNK, character??
    }

    [BlubContract]
    public class RoomBeginRoundReqMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public bool IsReady { get; set; }
    }

    [BlubContract]
    public class GameAvatarDurabilityDecreaseReqMessage : IGameRuleMessage
    {
    }

    [BlubContract]
    public class GameAvatarChangeReqMessage : IGameRuleMessage
    {
        public GameAvatarChangeReqMessage()
        {
            Unk1 = new ChangeAvatarUnk1Dto();
            Unk2 = Array.Empty<ChangeAvatarUnk2Dto>();
        }

        [BlubMember(0)]
        public ChangeAvatarUnk1Dto Unk1 { get; set; }

        [BlubMember(1, typeof(ArrayWithIntPrefixSerializer))]
        public ChangeAvatarUnk2Dto[] Unk2 { get; set; }
    }

    [BlubContract]
    public class RoomChangeRuleNotifyReqMessage : IGameRuleMessage
    {
        public RoomChangeRuleNotifyReqMessage()
        {
            Settings = new ChangeRuleDto();
        }

        [BlubMember(0)]
        public ChangeRuleDto Settings { get; set; }
    }

    [BlubContract]
    public class RoomChangeRuleNotifyReq2Message : IGameRuleMessage
    {
        public RoomChangeRuleNotifyReq2Message()
        {
            Settings = new ChangeRuleDto2();
        }

        [BlubMember(0)]
        public ChangeRuleDto2 Settings { get; set; }
    }

    [BlubContract]
    public class ScoreMissionScoreReqMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public int Score { get; set; }
    }

    [BlubContract]
    public class ScoreKillReqMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public ScoreDto Score { get; set; }
    }

    [BlubContract]
    public class ScoreKillAssistReqMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public ScoreAssist2Dto Score { get; set; }
    }

    [BlubContract]
    public class ScoreOffenseReqMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public Score2Dto Score { get; set; }
    }

    [BlubContract]
    public class ScoreOffenseAssistReqMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public ScoreAssist2Dto Score { get; set; }
    }

    [BlubContract]
    public class ScoreDefenseReqMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public Score2Dto Score { get; set; }
    }

    [BlubContract]
    public class ScoreDefenseAssistReqMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public ScoreAssist2Dto Score { get; set; }
    }

    [BlubContract]
    public class ScoreHealAssistReqMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public LongPeerId Id { get; set; }
    }

    [BlubContract]
    public class ScoreGoalReqMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public LongPeerId PeerId { get; set; }
    }

    [BlubContract]
    public class ScoreReboundReqMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public LongPeerId NewId { get; set; }

        [BlubMember(1)]
        public LongPeerId OldId { get; set; }
    }

    [BlubContract]
    public class ScoreSuicideReqMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public LongPeerId Id { get; set; }

        [BlubMember(1)]
        public uint Icon { get; set; }
    }

    [BlubContract]
    public class ScoreTeamKillReqMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public Score2Dto Score { get; set; }
    }

    [BlubContract]
    public class RoomItemChangeReqMessage : IGameRuleMessage
    {
        public RoomItemChangeReqMessage()
        {
            Unk1 = new ChangeItemsUnkDto();
            Unk2 = Array.Empty<ChangeAvatarUnk2Dto>();
        }

        [BlubMember(0)]
        public ChangeItemsUnkDto Unk1 { get; set; }

        [BlubMember(1, typeof(ArrayWithIntPrefixSerializer))]
        public ChangeAvatarUnk2Dto[] Unk2 { get; set; }
    }

    [BlubContract]
    public class RoomPlayModeChangeReqMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public PlayerGameMode Mode { get; set; }
    }

    [BlubContract]
    public class ArcadeScoreSyncReqMessage : IGameRuleMessage
    {
        public ArcadeScoreSyncReqMessage()
        {
            Scores = Array.Empty<ArcadeScoreSyncReqDto>();
        }

        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public ArcadeScoreSyncReqDto[] Scores { get; set; }
    }

    [BlubContract]
    public class ArcadeBeginRoundReqMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public byte Unk1 { get; set; }

        [BlubMember(1)]
        public byte Unk2 { get; set; }
    }

    [BlubContract]
    public class ArcadeStageClearReqMessage : IGameRuleMessage
    {
        public ArcadeStageClearReqMessage()
        {
            Scores = Array.Empty<ArcadeScoreSyncReqDto>();
        }

        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public ArcadeScoreSyncReqDto[] Scores { get; set; }
    }

    [BlubContract]
    public class ArcadeStageFailedReqMessage : IGameRuleMessage
    {
        public ArcadeStageFailedReqMessage()
        {
            Scores = Array.Empty<ArcadeScoreSyncReqDto>();
        }

        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public ArcadeScoreSyncReqDto[] Scores { get; set; }
    }

    [BlubContract]
    public class ArcadeStageInfoReqMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public byte Unk1 { get; set; }

        [BlubMember(1)]
        public int Unk2 { get; set; }
    }

    [BlubContract]
    public class ArcadeEnablePlayTimeReqMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public byte Unk { get; set; }
    }

    [BlubContract]
    public class ArcardRespawnReqMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public byte Unk1 { get; set; }

        [BlubMember(1)]
        public int Unk2 { get; set; }
    }

    [BlubContract]
    public class ArcadeStageReadyReqMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public byte Unk1 { get; set; }

        [BlubMember(1)]
        public int Unk2 { get; set; }
    }

    [BlubContract]
    public class ArcadeStageSelectReqMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public byte Unk1 { get; set; }

        [BlubMember(1)]
        public byte Unk2 { get; set; }
    }

    [BlubContract]
    public class SlaughterAttackPointReqMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public ulong AccountId { get; set; }

        [BlubMember(1)]
        public float Unk1 { get; set; }

        [BlubMember(2)]
        public float Unk2 { get; set; }
    }

    [BlubContract]
    public class SlaughterHealPointReqMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public float Unk { get; set; }
    }

    [BlubContract]
    public class ArcadeLoagdingSuccessReqMessage : IGameRuleMessage
    {
    }

    [BlubContract]
    public class MoneyUseCoinReqMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public int Unk { get; set; }
    }

    [BlubContract]
    public class LogBeginResponeReqMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public ulong Unk { get; set; }
    }

    [BlubContract]
    public class LogWeaponFireReqMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public ulong Unk1 { get; set; }

        [BlubMember(1)]
        public float Unk2 { get; set; }

        [BlubMember(2)]
        public int Unk3 { get; set; }

        [BlubMember(3)]
        public ulong Unk4 { get; set; }

        [BlubMember(4)]
        public int Unk5 { get; set; }

        [BlubMember(5)]
        public string Unk6 { get; set; }

        [BlubMember(6)]
        public int Unk7 { get; set; }

        [BlubMember(7)]
        public byte Unk8 { get; set; }
    }

    [BlubContract]
    public class GameKickOutRequestReqMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public ulong Sender { get; set; }

        [BlubMember(1)]
        public ulong Target { get; set; }

        [BlubMember(2)]
        public VoteKickReason Reason { get; set; }
    }

    [BlubContract]
    public class GameKickOutVoteResultReqMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public bool IsYes { get; set; }
    }

    [BlubContract]
    public class RoomIntrudeRoundReqMessage : IGameRuleMessage
    {
    }

    [BlubContract]
    public class GameLoadingSuccessReqMessage : IGameRuleMessage
    {
    }

    [BlubContract]
    public class SeizePositionCaptureReqMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public uint CaptureId { get; set; }

        [BlubMember(1)]
        public bool IsCapturing { get; set; }

        [BlubMember(2)]
        public uint Unk { get; set; }
    }

    [BlubContract]
    public class SeizeBuffItemGainReqMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public ulong Item { get; set; }
    }

    [BlubContract]
    public class RoomChoiceMasterChangeReqMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public ulong AccountId { get; set; }
    }

    [BlubContract]
    public class GameEquipCheckReqMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public EquipCheckDto Equip { get; set; }
    }

    [BlubContract]
    public class PromotionCointEventGetCoinReqMessage : IGameRuleMessage
    {
    }

    [BlubContract]
    public class InGameItemDropReqMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public ItemDropDto Item { get; set; }
    }

    [BlubContract]
    public class InGameItemGetReqMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public int Unk1 { get; set; }

        [BlubMember(1)]
        public int Unk2 { get; set; }
    }

    [BlubContract]
    public class InGamePlayerResponseReqMessage : IGameRuleMessage
    {
    }

    [BlubContract]
    public class ChallengeRankingListReqMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public int Unk { get; set; }
    }

    [BlubContract]
    public class ChallengeResultReqMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public ChallengeResultDto Item { get; set; }
    }

    [BlubContract]
    public class ChallengeReStartReqMessage : IGameRuleMessage
    {
    }

    [BlubContract]
    public class PromotionCouponEventIngameGetReqMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public int Unk1 { get; set; }

        [BlubMember(1)]
        public int Unk2 { get; set; }
    }
}
