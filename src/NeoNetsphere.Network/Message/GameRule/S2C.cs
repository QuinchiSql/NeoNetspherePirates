using System;
using BlubLib.Serialization;
using BlubLib.Serialization.Serializers;
using NeoNetsphere.Network.Data.GameRule;
using NeoNetsphere.Network.Serializers;
using ProudNetSrc.Serialization.Serializers;

namespace NeoNetsphere.Network.Message.GameRule
{
    [BlubContract]
    public class RoomEnterPlayerAckMessage : IGameRuleMessage
    {
        public RoomEnterPlayerAckMessage()
        {
            Nickname = "";
        }

        public RoomEnterPlayerAckMessage(ulong accountId, string nickname, byte unk1, PlayerGameMode mode, int unk3)
        {
            AccountId = accountId;
            Unk1 = unk1;
            PlayerGameMode = mode;
            ClanId = unk3;
            Nickname = nickname;
        }

        [BlubMember(0)]
        public ulong AccountId { get; set; }

        [BlubMember(1)]
        public byte Unk1 { get; set; } // 0 = char does not spawn

        [BlubMember(2)]
        public PlayerGameMode PlayerGameMode { get; set; }

        [BlubMember(3)]
        public int ClanId { get; set; }

        [BlubMember(4, typeof(StringSerializer))]
        public string Nickname { get; set; }

        [BlubMember(5)]
        public Team Team { get; set; }
    }

    [BlubContract]
    public class RoomLeavePlayerAckMessage : IGameRuleMessage
    {
        public RoomLeavePlayerAckMessage()
        {
            Nickname = "";
        }

        public RoomLeavePlayerAckMessage(ulong accountId, string nickname, RoomLeaveReason reason)
        {
            AccountId = accountId;
            Nickname = nickname;
            Reason = reason;
        }

        [BlubMember(0)]
        public ulong AccountId { get; set; }

        [BlubMember(1, typeof(StringSerializer))]
        public string Nickname { get; set; }

        [BlubMember(2)]
        public RoomLeaveReason Reason { get; set; }
    }

    [BlubContract]
    public class RoomLeaveReqeustAckMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public byte Unk { get; set; } // result?
    }

    [BlubContract]
    public class RoomChangeTeamAckMessage : IGameRuleMessage
    {
        public RoomChangeTeamAckMessage()
        {
        }

        public RoomChangeTeamAckMessage(ulong accountId, Team team, PlayerGameMode mode)
        {
            AccountId = accountId;
            Team = team;
            Mode = mode;
        }

        [BlubMember(0)]
        public ulong AccountId { get; set; }

        [BlubMember(1)]
        public Team Team { get; set; }

        [BlubMember(2)]
        public PlayerGameMode Mode { get; set; }
    }

    [BlubContract]
    public class RoomChangeTeamFailAckMessage : IGameRuleMessage
    {
        public RoomChangeTeamFailAckMessage()
        {
        }

        public RoomChangeTeamFailAckMessage(ChangeTeamResult result)
        {
            Result = result;
        }

        [BlubMember(0)]
        public ChangeTeamResult Result { get; set; }
    }

    [BlubContract]
    public class RoomChoiceTeamChangeAckMessage : IGameRuleMessage
    {
        public RoomChoiceTeamChangeAckMessage()
        {
        }

        public RoomChoiceTeamChangeAckMessage(ulong playerToMove, ulong playerToReplace, Team fromTeam, Team toTeam)
        {
            PlayerToMove = playerToMove;
            PlayerToReplace = playerToReplace;
            FromTeam = fromTeam;
            ToTeam = toTeam;
        }

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
    public class RoomChoiceTeamChangeFailAckMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public byte Result { get; set; }
    }

    [BlubContract]
    public class GameEventMessageAckMessage : IGameRuleMessage
    {
        public GameEventMessageAckMessage()
        {
            String = "";
        }

        public GameEventMessageAckMessage(GameEventMessage @event, ulong accountId, uint unk, ushort value,
            string @string)
        {
            Event = @event;
            AccountId = accountId;
            Unk = unk;
            Value = value;
            String = @string;
        }

        [BlubMember(0)]
        public GameEventMessage Event { get; set; }

        [BlubMember(1)]
        public ulong AccountId { get; set; }

        [BlubMember(2)]
        public uint Unk { get; set; } // server/game time or something like that

        [BlubMember(3)]
        public ushort Value { get; set; }

        [BlubMember(4, typeof(StringSerializer))]
        public string String { get; set; }
    }

    [BlubContract]
    public class GameBriefingInfoAckMessage : IGameRuleMessage
    {
        public GameBriefingInfoAckMessage()
        {
            Data = Array.Empty<byte>();
        }

        public GameBriefingInfoAckMessage(bool isResult, bool isEvent, byte[] data)
        {
            IsResult = isResult;
            IsEvent = isEvent;
            Data = data;
        }

        [BlubMember(0)]
        public bool IsResult { get; set; }

        [BlubMember(1)]
        public bool IsEvent { get; set; }

        [BlubMember(2, typeof(ArrayWithScalarSerializer))]
        public byte[] Data { get; set; }
    }

    [BlubContract]
    public class GameChangeStateAckMessage : IGameRuleMessage
    {
        public GameChangeStateAckMessage()
        {
        }

        public GameChangeStateAckMessage(GameState state)
        {
            State = state;
        }

        [BlubMember(0)]
        public GameState State { get; set; }
    }

    [BlubContract]
    public class GameChangeSubStateAckMessage : IGameRuleMessage
    {
        public GameChangeSubStateAckMessage()
        {
        }

        public GameChangeSubStateAckMessage(GameTimeState state)
        {
            State = state;
        }

        [BlubMember(0)]
        public GameTimeState State { get; set; }
    }

    [BlubContract]
    public class GameDestroyGameRuleAckMessage : IGameRuleMessage
    {
    }

    [BlubContract]
    public class RoomChangeMasterAckMessage : IGameRuleMessage
    {
        public RoomChangeMasterAckMessage()
        {
        }

        public RoomChangeMasterAckMessage(ulong accountId)
        {
            AccountId = accountId;
        }

        [BlubMember(0)]
        public ulong AccountId { get; set; }
    }

    [BlubContract]
    public class RoomChangeRefereeAckMessage : IGameRuleMessage
    {
        public RoomChangeRefereeAckMessage()
        {
        }

        public RoomChangeRefereeAckMessage(ulong accountId)
        {
            AccountId = accountId;
        }

        [BlubMember(0)]
        public ulong AccountId { get; set; }
    }

    [BlubContract]
    public class SlaughterChangeSlaughterAckMessage : IGameRuleMessage
    {
        public SlaughterChangeSlaughterAckMessage()
        {
            Unk = Array.Empty<ulong>();
        }

        public SlaughterChangeSlaughterAckMessage(ulong accountId)
        {
            AccountId = accountId;
            Unk = Array.Empty<ulong>();
        }

        public SlaughterChangeSlaughterAckMessage(ulong accountId, ulong[] unk)
        {
            AccountId = accountId;
            Unk = unk;
        }

        [BlubMember(0)]
        public ulong AccountId { get; set; }

        [BlubMember(1, typeof(ArrayWithIntPrefixSerializer))]
        public ulong[] Unk { get; set; }
    }

    [BlubContract]
    public class RoomReadyRoundAckMessage : IGameRuleMessage
    {
        public RoomReadyRoundAckMessage()
        {
        }

        public RoomReadyRoundAckMessage(ulong accountId, bool isReady)
        {
            AccountId = accountId;
            IsReady = isReady;
        }

        [BlubMember(0)]
        public ulong AccountId { get; set; }

        [BlubMember(1)]
        public bool IsReady { get; set; }

        [BlubMember(2)]
        public byte Result { get; set; }
    }

    [BlubContract]
    public class RoomBeginRoundAckMessage : IGameRuleMessage
    {
    }

    [BlubContract]
    public class GameAvatarChangeAckMessage : IGameRuleMessage
    {
        public GameAvatarChangeAckMessage()
        {
            Unk1 = new ChangeAvatarUnk1Dto();
            Unk2 = Array.Empty<ChangeAvatarUnk2Dto>();
        }

        public GameAvatarChangeAckMessage(ChangeAvatarUnk1Dto unk1, ChangeAvatarUnk2Dto[] unk2)
        {
            Unk1 = unk1;
            Unk2 = unk2;
        }

        [BlubMember(0)]
        public ChangeAvatarUnk1Dto Unk1 { get; set; }

        [BlubMember(1, typeof(ArrayWithIntPrefixSerializer))]
        public ChangeAvatarUnk2Dto[] Unk2 { get; set; }
    }

    [BlubContract]
    public class RoomChangeRuleNotifyAckMessage : IGameRuleMessage
    {
        public RoomChangeRuleNotifyAckMessage()
        {
            Settings = new ChangeRuleDto();
        }

        public RoomChangeRuleNotifyAckMessage(ChangeRuleDto settings)
        {
            Settings = settings;
        }

        [BlubMember(0)]
        public ChangeRuleDto Settings { get; set; }
    }

    [BlubContract]
    public class RoomChangeRuleNotifyAck2Message : IGameRuleMessage
    {
        public RoomChangeRuleNotifyAck2Message()
        {
            Settings = new ChangeRuleDto2();
        }

        public RoomChangeRuleNotifyAck2Message(ChangeRuleDto2 settings)
        {
            Settings = settings;
        }

        [BlubMember(0)]
        public ChangeRuleDto2 Settings { get; set; }
    }

    [BlubContract]
    public class RoomChangeRuleAckMessage : IGameRuleMessage
    {
        public RoomChangeRuleAckMessage()
        {
            Settings = new ChangeRuleDto();
        }

        public RoomChangeRuleAckMessage(ChangeRuleDto settings)
        {
            Settings = settings;
        }

        [BlubMember(0)]
        public ChangeRuleDto Settings { get; set; }
    }

    [BlubContract]
    public class RoomChangeRuleFailAckMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public byte Result { get; set; }
    }

    [BlubContract]
    public class ScoreMissionScoreAckMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public ulong AccountId { get; set; }

        [BlubMember(1)]
        public int Score { get; set; }
    }

    [BlubContract]
    public class ScoreKillAckMessage : IGameRuleMessage
    {
        public ScoreKillAckMessage()
        {
            Score = new ScoreDto();
        }

        public ScoreKillAckMessage(ScoreDto score)
        {
            Score = score;
        }

        [BlubMember(0)]
        public ScoreDto Score { get; set; }
    }

    [BlubContract]
    public class ScoreKillAssistAckMessage : IGameRuleMessage
    {
        public ScoreKillAssistAckMessage()
        {
            Score = new ScoreAssistDto();
        }

        public ScoreKillAssistAckMessage(ScoreAssistDto score)
        {
            Score = score;
        }

        [BlubMember(0)]
        public ScoreAssistDto Score { get; set; }
    }

    [BlubContract]
    public class ScoreOffenseAckMessage : IGameRuleMessage
    {
        public ScoreOffenseAckMessage()
        {
            Score = new ScoreDto();
        }

        public ScoreOffenseAckMessage(ScoreDto score)
        {
            Score = score;
        }

        [BlubMember(0)]
        public ScoreDto Score { get; set; }
    }

    [BlubContract]
    public class ScoreOffenseAssistAckMessage : IGameRuleMessage
    {
        public ScoreOffenseAssistAckMessage()
        {
            Score = new ScoreAssistDto();
        }

        public ScoreOffenseAssistAckMessage(ScoreAssistDto score)
        {
            Score = score;
        }

        [BlubMember(0)]
        public ScoreAssistDto Score { get; set; }
    }

    [BlubContract]
    public class ScoreDefenseAckMessage : IGameRuleMessage
    {
        public ScoreDefenseAckMessage()
        {
            Score = new ScoreDto();
        }

        public ScoreDefenseAckMessage(ScoreDto score)
        {
            Score = score;
        }

        [BlubMember(0)]
        public ScoreDto Score { get; set; }
    }

    [BlubContract]
    public class ScoreDefenseAssistAckMessage : IGameRuleMessage
    {
        public ScoreDefenseAssistAckMessage()
        {
            Score = new ScoreAssistDto();
        }

        public ScoreDefenseAssistAckMessage(ScoreAssistDto score)
        {
            Score = score;
        }

        [BlubMember(0)]
        public ScoreAssistDto Score { get; set; }
    }

    [BlubContract]
    public class ScoreHealAssistAckMessage : IGameRuleMessage
    {
        public ScoreHealAssistAckMessage()
        {
            Id = 0;
        }

        public ScoreHealAssistAckMessage(LongPeerId id)
        {
            Id = id;
        }

        [BlubMember(0)]
        public LongPeerId Id { get; set; }
    }

    [BlubContract]
    public class ScoreGoalAckMessage : IGameRuleMessage
    {
        public ScoreGoalAckMessage()
        {
            Id = 0;
        }

        public ScoreGoalAckMessage(LongPeerId id)
        {
            Id = id;
        }

        [BlubMember(0)]
        public LongPeerId Id { get; set; }
    }

    [BlubContract]
    public class ScoreGoalAssistAckMessage : IGameRuleMessage
    {
        public ScoreGoalAssistAckMessage()
        {
            Id = 0;
            Assist = 0;
        }

        public ScoreGoalAssistAckMessage(LongPeerId id, LongPeerId assist)
        {
            Id = id;
            Assist = assist;
        }

        [BlubMember(0)]
        public LongPeerId Id { get; set; }

        [BlubMember(1)]
        public LongPeerId Assist { get; set; }
    }

    [BlubContract]
    public class ScoreReboundAckMessage : IGameRuleMessage
    {
        public ScoreReboundAckMessage()
        {
            NewId = 0;
            OldId = 0;
        }

        public ScoreReboundAckMessage(LongPeerId newId, LongPeerId oldId)
        {
            NewId = newId;
            OldId = oldId;
        }

        [BlubMember(0)]
        public LongPeerId NewId { get; set; }

        [BlubMember(1)]
        public LongPeerId OldId { get; set; }
    }

    [BlubContract]
    public class ScoreSuicideAckMessage : IGameRuleMessage
    {
        public ScoreSuicideAckMessage()
        {
            Id = 0;
        }

        public ScoreSuicideAckMessage(LongPeerId id, AttackAttribute icon)
        {
            Id = id;
            Icon = icon;
        }

        [BlubMember(0)]
        public LongPeerId Id { get; set; }

        [BlubMember(1, typeof(EnumSerializer), typeof(uint))]
        public AttackAttribute Icon { get; set; }
    }

    [BlubContract]
    public class ScoreTeamKillAckMessage : IGameRuleMessage
    {
        public ScoreTeamKillAckMessage()
        {
            Score = new Score2Dto();
        }

        public ScoreTeamKillAckMessage(Score2Dto score)
        {
            Score = score;
        }

        [BlubMember(0)]
        public Score2Dto Score { get; set; }
    }

    [BlubContract]
    public class SlaughterRoundWinAckMessage : IGameRuleMessage
    {
        public SlaughterRoundWinAckMessage()
        {
        }

        public SlaughterRoundWinAckMessage(byte unk)
        {
            Unk = unk;
        }

        [BlubMember(0)]
        public byte Unk { get; set; }
    }

    [BlubContract]
    public class SlaughterSLRoundWinAckMessage : IGameRuleMessage
    {
    }

    [BlubContract]
    public class RoomChangeItemAckMessage : IGameRuleMessage
    {
        public RoomChangeItemAckMessage()
        {
            Unk1 = new ChangeItemsUnkDto();
            Unk2 = Array.Empty<ChangeAvatarUnk2Dto>();
        }

        public RoomChangeItemAckMessage(ChangeItemsUnkDto unk1, ChangeAvatarUnk2Dto[] unk2)
        {
            Unk1 = unk1;
            Unk2 = unk2;
        }

        [BlubMember(0)]
        public ChangeItemsUnkDto Unk1 { get; set; }

        [BlubMember(1, typeof(ArrayWithIntPrefixSerializer))]
        public ChangeAvatarUnk2Dto[] Unk2 { get; set; }
    }

    [BlubContract]
    public class RoomPlayModeChangeAckMessage : IGameRuleMessage
    {
        public RoomPlayModeChangeAckMessage()
        {
        }

        public RoomPlayModeChangeAckMessage(ulong accountId, PlayerGameMode mode)
        {
            AccountId = accountId;
            Mode = mode;
        }

        [BlubMember(0)]
        public ulong AccountId { get; set; }

        [BlubMember(1)]
        public PlayerGameMode Mode { get; set; }
    }

    [BlubContract]
    public class GameRefreshGameRuleInfoAckMessage : IGameRuleMessage
    {
        public GameRefreshGameRuleInfoAckMessage()
        {
        }

        public GameRefreshGameRuleInfoAckMessage(GameState _GameState, GameTimeState _GameTimeState, int _ElapsedTime)
        {
            GameState = _GameState;
            GameTimeState = _GameTimeState;
            ElapsedTime = _ElapsedTime;
        }

        [BlubMember(0)]
        public GameState GameState { get; set; }

        [BlubMember(1)]
        public GameTimeState GameTimeState { get; set; }

        [BlubMember(2)]
        public int ElapsedTime { get; set; }
    }

    [BlubContract]
    public class ArcadeScoreSyncAckMessage : IGameRuleMessage
    {
        public ArcadeScoreSyncAckMessage()
        {
            Scores = Array.Empty<ArcadeScoreSyncDto>();
        }

        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public ArcadeScoreSyncDto[] Scores { get; set; }
    }

    [BlubContract]
    public class ArcadeBeginRoundAckMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public byte Unk1 { get; set; }

        [BlubMember(1)]
        public byte Unk2 { get; set; }

        [BlubMember(2)]
        public byte Unk3 { get; set; }
    }

    [BlubContract]
    public class ArcadeStageBriefingAckMessage : IGameRuleMessage
    {
        public ArcadeStageBriefingAckMessage()
        {
            Data = Array.Empty<byte>();
        }

        [BlubMember(0)]
        public byte Unk1 { get; set; }

        [BlubMember(1)]
        public byte Unk2 { get; set; }

        [BlubMember(2, typeof(ArrayWithScalarSerializer))]
        public byte[] Data { get; set; } // ToDo
    }

    [BlubContract]
    public class ArcadeEnablePlayTimeAckMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public byte Unk { get; set; }
    }

    [BlubContract]
    public class ArcadeStageInfoAckMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public byte Unk1 { get; set; }

        [BlubMember(1)]
        public int Unk2 { get; set; }
    }

    [BlubContract]
    public class ArcadeRespawnAckMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public int Unk { get; set; }
    }

    [BlubContract]
    public class ArcadeDeathPlayerInfoAckMessage : IGameRuleMessage
    {
        public ArcadeDeathPlayerInfoAckMessage()
        {
            Players = Array.Empty<ulong>();
        }

        [BlubMember(0)]
        public byte Unk { get; set; }

        [BlubMember(1, typeof(ArrayWithIntPrefixSerializer))]
        public ulong[] Players { get; set; }
    }

    [BlubContract]
    public class ArcadeStageReadyAckMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public ulong AccountId { get; set; }
    }

    [BlubContract]
    public class ArcadeRespawnFailAckMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public uint Result { get; set; }
    }

    [BlubContract]
    public class AdminChangeHPAckMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public float Value { get; set; }
    }

    [BlubContract]
    public class AdminChangeMPAckMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public float Value { get; set; }
    }

    [BlubContract]
    public class ArcadeChangeStageAckMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public byte Stage { get; set; }
    }

    [BlubContract]
    public class ArcadeStageSelectAckMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public byte Unk1 { get; set; }

        [BlubMember(1)]
        public byte Unk2 { get; set; }
    }

    [BlubContract]
    public class ArcadeSaveDateInfoAckMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public byte Unk { get; set; }
    }

    [BlubContract]
    public class SlaughterAttackPointAckMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public ulong AccountId { get; set; }

        [BlubMember(1)]
        public float Unk1 { get; set; }

        [BlubMember(2)]
        public float Unk2 { get; set; }
    }

    [BlubContract]
    public class SlaughterHealPointAckMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public ulong AccountId { get; set; }

        [BlubMember(1)]
        public float Unk { get; set; }
    }

    [BlubContract]
    public class SlaughterChangeBonusTargetAckMessage : IGameRuleMessage
    {
        public SlaughterChangeBonusTargetAckMessage()
        {
        }

        public SlaughterChangeBonusTargetAckMessage(ulong accountId)
        {
            AccountId = accountId;
        }

        [BlubMember(0)]
        public ulong AccountId { get; set; }
    }

    [BlubContract]
    public class ArcadeSucceedLoadingAckMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public ulong AccountId { get; set; }
    }

    [BlubContract]
    public class MoneyUseCoinAckMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public int Unk1 { get; set; }

        [BlubMember(1)]
        public int Unk2 { get; set; }

        [BlubMember(2)]
        public int Unk3 { get; set; }

        [BlubMember(3)]
        public int Unk4 { get; set; }

        [BlubMember(4)]
        public byte Unk5 { get; set; }
    }

    [BlubContract]
    public class GameLuckyShotAckMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public int Unk1 { get; set; }

        [BlubMember(1)]
        public int Unk2 { get; set; }

        [BlubMember(2)]
        public int Unk3 { get; set; }
    }

    [BlubContract]
    public class FreeAllForChangeTheFirstAckMessage : IGameRuleMessage
    {
        public FreeAllForChangeTheFirstAckMessage()
        {
        }

        public FreeAllForChangeTheFirstAckMessage(ulong accountId)
        {
            AccountId = accountId;
        }

        [BlubMember(0)]
        public ulong AccountId { get; set; }
    }

    [BlubContract]
    public class LogDevLogStartAckMessage : IGameRuleMessage
    {
    }

    [BlubContract]
    public class GameKickOutRequestAckMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public int Unk { get; set; }
    }

    [BlubContract]
    public class GameKickOutVoteResultAckMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public int Unk { get; set; }
    }

    [BlubContract]
    public class GameKickOutStateAckMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public uint NoCount { get; set; }

        [BlubMember(1)]
        public uint YesCount { get; set; }

        [BlubMember(2)]
        public uint NeededCount { get; set; }

        [BlubMember(3)]
        public VoteKickReason Reason { get; set; }

        [BlubMember(4)]
        public ulong Sender { get; set; }

        [BlubMember(5)]
        public ulong Target { get; set; }
    }

    [BlubContract]
    public class CaptainRoundCaptainLifeInfoAckMessage : IGameRuleMessage
    {
        public CaptainRoundCaptainLifeInfoAckMessage()
        {
            Players = Array.Empty<CaptainLifeDto>();
        }

        public CaptainRoundCaptainLifeInfoAckMessage(CaptainLifeDto[] players)
        {
            Players = players;
        }

        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public CaptainLifeDto[] Players { get; set; }
    }

    [BlubContract]
    public class CaptainSubRoundWinAckMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public int Unk1 { get; set; } //team?

        [BlubMember(1)]
        public byte Unk2 { get; set; } 

        public CaptainSubRoundWinAckMessage()
        {
        }

        public CaptainSubRoundWinAckMessage(int Team, byte hasWon)
        {
            Unk1 = Team;
            Unk2 = hasWon;
        }
    }

    [BlubContract]
    public class CaptainCurrentRoundInfoAckMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public int Unk1 { get; set; }

        [BlubMember(1)]
        public int Unk2 { get; set; }

        public CaptainCurrentRoundInfoAckMessage()
        {

        }

        public CaptainCurrentRoundInfoAckMessage(int AlphaWins, int BetaWins)
        {
            Unk1 = AlphaWins;
            Unk2 = BetaWins;
        }
    }

    [BlubContract]
    public class SeizeUpdateInfoAckMessage : IGameRuleMessage
    {
        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public SeizeUpdateInfoDto[] Infos { get; set; }
    }

    [BlubContract]
    public class SeizeUpdateInfoByIntrudeAckMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public SeizeIntrudeInfoDto[] Infos { get; set; }
    }

    [BlubContract]
    public class SeizeFeverTimeAckMessage : IGameRuleMessage
    {
    }

    [BlubContract]
    public class SeizeBuffItemGainAckMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public ulong Unk1 { get; set; }

        [BlubMember(1)]
        public ulong Unk2 { get; set; }
    }

    [BlubContract]
    public class SeizeDropBuffItemAckMessage : IGameRuleMessage
    {
        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public ulong[] Unk { get; set; }
    }

    [BlubContract]
    public class SeizeUpKeepScoreUpdateAckMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public int Unk1 { get; set; }

        [BlubMember(1)]
        public int Unk2 { get; set; }
    }

    [BlubContract]
    public class SeizeUpKeepScoreGetAckMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public int Unk1 { get; set; }

        [BlubMember(1)]
        public int Unk2 { get; set; }
    }

    [BlubContract]
    public class RoomChangeMasterReqeustAckMessage : IGameRuleMessage
    {
    }

    [BlubContract]
    public class RoomMixedTeamBriefingInfoAckMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public byte Unk1 { get; set; } //result?

        [BlubMember(1, typeof(ArrayWithIntPrefixSerializer))]
        public MixedTeamBriefingDto[] Unk2 { get; set; }
    }

    [BlubContract]
    public class GameEquipCheckAckMessage : IGameRuleMessage
    {
    }

    [BlubContract]
    public class RoomGameStartAckMessage : IGameRuleMessage
    {
    }

    [BlubContract]
    public class RoomGameLoadingAckMessage : IGameRuleMessage
    {
    }

    [BlubContract]
    public class GameTackUpdateAckMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public int Unk1 { get; set; }

        [BlubMember(1)]
        public short Unk2 { get; set; }
    }

    [BlubContract]
    public class RoomGameEndLoadingAckMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public ulong Unk { get; set; }

        public RoomGameEndLoadingAckMessage()
        {
            Unk = 0;
        }

        public RoomGameEndLoadingAckMessage(ulong player)
        {
            Unk = player;
        }
    }

    [BlubContract]
    public class RoomGamePlayCountDownAckMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public int TimeinMs { get; set; }

        public RoomGamePlayCountDownAckMessage()
        {
        }

        public RoomGamePlayCountDownAckMessage(int timeinMs)
        {
            TimeinMs = timeinMs;
        }
    }

    [BlubContract]
    public class InGameItemDropAckMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public ItemDropAckDto Item { get; set; }
    }

    [BlubContract]
    public class InGameItemGetAckMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public long Unk1 { get; set; }

        [BlubMember(1)]
        public int Unk2 { get; set; }

        [BlubMember(2)]
        public int Unk3 { get; set; }
    }

    [BlubContract]
    public class InGamePlayerResponseOfDeathAckMessage : IGameRuleMessage
    {
    }

    [BlubContract]
    public class ChallengeRankersAckMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public int Unk { get; set; }

        [BlubMember(1)]
        public ChallengeRankerDto[] Rankers { get; set; }
    }

    [BlubContract]
    public class ChallengeRankingListAckMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public int Unk { get; set; }

        [BlubMember(1)]
        public ChallengeRankerDto[] Rankers { get; set; }
    }

    [BlubContract]
    public class PromotionCouponEventIngameGetAckMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public int Unk { get; set; }
    }

    [BlubContract]
    public class RoomEnterPlayerForBookNameTagsAckMessage : IGameRuleMessage
    {
        [BlubMember(0)]
        public long AccountId { get; set; }

        [BlubMember(1)]
        public Team Team { get; set; }

        [BlubMember(2)]
        public PlayerGameMode PlayerGameMode { get; set; }

        [BlubMember(3)]
        public uint Exp { get; set; }

        [BlubMember(4, typeof(StringSerializer))]
        public string Nickname { get; set; }

        [BlubMember(5)]
        public uint Unk1 { get; set; }

        [BlubMember(6)]
        public byte Unk2 { get; set; } 
    }

    [BlubContract]
    public class RoomEnterPlayerInfoListForNameTagAckMessage : IGameRuleMessage
    {
        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public NameTagDto[] Tags { get; set; }

        public RoomEnterPlayerInfoListForNameTagAckMessage()
        {
            Tags = Array.Empty<NameTagDto>();
        }

        public RoomEnterPlayerInfoListForNameTagAckMessage(NameTagDto[] tags)
        {
            Tags = tags;
        }
    }
}
