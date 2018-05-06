using System;
using System.Net;
using NeoNetsphere;

// ReSharper disable once CheckNamespace
namespace Netsphere
{
    internal class RoomCreationOptions
    {
        public string Name { get; set; }
        public GameRule GameRule { get; set; }
        public TimeSpan TimeLimit { get; set; }
        public ushort ScoreLimit { get; set; }
        public string Password { get; set; }
        public bool IsFriendly { get; set; }
        public byte PlayerLimit { get; set; }
        public byte ItemLimit { get; set; }
        public bool IsNoIntrusion { get; set; }
        public bool HasSpectator { get; set; }
        public int SpectatorLimit { get; set; }
        public int MapId { get; set; }
        public byte UniqueId { get; set; }
        public bool IsBurning { get; set; }
        public bool IsWithoutStats { get; set; }
        public bool IsRandom { get; set; }
        public byte FMBURNMode => GetFMBurnModeInfo();

        public IPEndPoint ServerEndPoint { get; set; }
        public Player Creator { get; set; }

        internal virtual byte GetFMBurnModeInfo()
        {
            byte FMBurnMode = 0;
            if (IsFriendly && IsWithoutStats)
                FMBurnMode = 5;
            else if (IsWithoutStats)
                FMBurnMode = 4;
            else if (IsFriendly && IsBurning)
                FMBurnMode = 3;
            else if (IsBurning)
                FMBurnMode = 2;
            else if (IsFriendly)
                FMBurnMode = 1;
            else if (!IsFriendly && !IsBurning)
                FMBurnMode = 0;
            return FMBurnMode;
        }
    }
}
