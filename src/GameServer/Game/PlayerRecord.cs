using System;
using System.IO;
using NeoNetsphere;

// ReSharper disable once CheckNamespace
namespace Netsphere.Game
{
    internal abstract class PlayerRecord
    {
        protected PlayerRecord(Player player)
        {
            Player = player;
        }

        public Player Player { get; }
        public abstract uint TotalScore { get; }
        public uint Kills { get; set; }
        public uint KillAssists { get; set; }
        public uint Suicides { get; set; }
        public uint Deaths { get; set; }

        public virtual uint GetPenGain(out uint bonusPen)
        {
            bonusPen = 0;
            return 0;
        }

        public virtual int GetExpGain(out int bonusExp)
        {
            bonusExp = 0;
            return 0;
        }

        public virtual void Reset()
        {
            Kills = 0;
            KillAssists = 0;
            Suicides = 0;
            Deaths = 0;
        }

        public virtual void Serialize(BinaryWriter w, bool isResult)
        {
            w.Write(Player.Account.Id); //Int64
            w.Write((byte) Player.RoomInfo.Team.Team); //Int8
            w.Write((byte) Player.RoomInfo.State); //Int8
            w.Write(Convert.ToByte(Player.RoomInfo.IsReady)); //Int8
            w.Write((uint) Player.RoomInfo.Mode); //Int32
            w.Write(TotalScore); //Int32
            w.Write(0); //Int32

            uint bonusPen = 0;
            var bonusExp = 0;
            var rankUp = false;
            if (isResult)
            {
                w.Write(GetPenGain(out bonusPen)); //Int32

                var expGain = GetExpGain(out bonusExp);

                rankUp = Player.GainExp(expGain);

                w.Write(expGain); //Int32    
            }
            else
            {
                w.Write(0);
                w.Write(0);
            }

            w.Write(Player.TotalExperience); //Int32
            w.Write(rankUp); //Int8
            w.Write(bonusExp); //Int32
            w.Write(bonusPen); //Int32
            w.Write(0); //Int32

            /*                                                              
                1 PC Room(korean internet cafe event)                       
                2 PEN+                                                      
                4 EXP+                                                      
                8 20%                                                       
                16 25%                                                      
                32 30%                                                      
            */
            w.Write(0); //Int32
            w.Write((byte) 0); //Int8
            w.Write((byte) 0); //Int8
            w.Write((byte) 0); //Int8
            w.Write(0); //Int32
            w.Write(0); //Int32
            w.Write(0); //Int32
            w.Write(0); //Int32

            //NEW - UNKNOWN
            w.Write(0); //Int32
            w.Write((byte)0); //Int8 -- player room index?? team?
            w.Write(0); //Int32
            w.Write(0); //Int32
        }
    }
}
