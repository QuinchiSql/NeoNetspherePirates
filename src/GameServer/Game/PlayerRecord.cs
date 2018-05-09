using System;
using System.IO;
using System.Linq;
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
            var exp = GetExpGain(out var bonus);
            return (uint)(exp - bonus);
        }

        public virtual int GetExpGain(out int bonusExp)
        {
            var place = 1;
            ExperienceRates ExpRates = null;
            var game = Config.Instance.Game;
            bonusExp = 0;

            switch (Player.Room.GameRuleManager.GameRule.GameRule)
            {
                case GameRule.Arcade:
                    break;
                case GameRule.Arena:
                    break;
                case GameRule.BattleRoyal:
                    ExpRates = game.BRExpRates;
                    break;
                case GameRule.Captain:
                    ExpRates = game.CaptainExpRates;
                    break;
                case GameRule.Challenge:
                    break;
                case GameRule.Chaser:
                    ExpRates = game.ChaserExpRates;
                    break;
                case GameRule.CombatTrainingDM:
                    break;
                case GameRule.CombatTrainingTD:
                    break;
                case GameRule.Deathmatch:
                    ExpRates = game.DeathmatchExpRates;
                    break;
                case GameRule.Horde:
                    break;
                case GameRule.PassTouchdown:
                    break;
                case GameRule.Practice:
                    break;
                case GameRule.SemiTouchdown:
                    break;
                case GameRule.Siege:
                    break;
                case GameRule.SnowballFight:
                    break;
                case GameRule.Survival:
                    break;
                case GameRule.Touchdown:
                    ExpRates = game.TouchdownExpRates;
                    break;
                case GameRule.Tutorial:
                    break;
                case GameRule.Warfare:
                    break;
            }

            if (ExpRates == null)
                return 0;

            var plrs = Player.Room.TeamManager.Players
                .Where(plr => plr.RoomInfo.State == PlayerState.Waiting &&
                    plr.RoomInfo.Mode == PlayerGameMode.Normal)
                .ToArray();

            foreach (var plr in plrs.OrderByDescending(plr => plr.RoomInfo.Stats.TotalScore))
            {
                if (plr == Player)
                    break;

                place++;
                if (place > 3)
                    break;
            }

            var rankingBonus = 1.0f;
            switch (place)
            {
                case 1:
                    rankingBonus += ExpRates.FirstPlaceBonus / 100.0f;
                    break;

                case 2:
                    rankingBonus += ExpRates.SecondPlaceBonus / 100.0f;
                    break;

                case 3:
                    rankingBonus += ExpRates.ThirdPlaceBonus / 100.0f;
                    break;
            }

            var TimeExp = ExpRates.ExpPerMin * Player.RoomInfo.PlayTime.Minutes;
            var PlayersExp = plrs.Length * ExpRates.PlayerCountFactor;
            var ScoreExp = ExpRates.ExpPerMin * TotalScore;

            var ExpGained = (TimeExp + PlayersExp + ScoreExp) * rankingBonus;

            bonusExp = (uint)(ExpGained * Player.GetExpRate());

            return (uint)ExpGained + bonusExp;
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
            w.Write((byte) 0); //Int8 -- player room index?? team?
            w.Write(0); //Int32
            w.Write(0); //Int32
        }
    }
}
