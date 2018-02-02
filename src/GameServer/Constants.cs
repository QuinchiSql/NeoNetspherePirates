// ReSharper disable once CheckNamespace

namespace Netsphere
{
    internal enum PlayerSetting
    {
        AllowCombiInvite,
        AllowFriendRequest,
        AllowRoomInvite,
        AllowInfoRequest
    }
    
    internal enum GameRuleState
    {
        Waiting,
        Playing,
        EnteringResult,
        Result,

        FirstHalf,
        EnteringHalfTime,
        HalfTime,
        SecondHalf,
        FullGame,

        Preparing,
    }

    internal enum GameRuleStateTrigger
    {
        StartPrepare,
        StartGame,
        EndGame,
        StartResult,
        StartHalfTime,
        StartSecondHalf
    }
}
