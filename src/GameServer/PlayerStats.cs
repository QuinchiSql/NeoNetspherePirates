using Dapper.FastCrud;
using NeoNetsphere.Database.Game;
using NeoNetsphere.Network.Data.Chat;
using NeoNetsphere.Network.Data.Game;
using Netsphere.Game.GameRules;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace NeoNetsphere
{
    internal class StatsManager
    {
        private Player Player;
        public DMStats DeathMatch { get; }
        public TDStats TouchDown { get; }
        public ChaserStats Chaser { get; }
        public BRStats BattleRoyal { get; }
        public CPTStats Captain { get; }
        public SiegeStats Siege { get; }
        private BaseStats statistics;
        private bool FriendlyMode;

        public StatsManager(Player player, PlayerDto playerDto)
        {
            Player = player;
            DeathMatch = new DMStats(Player, playerDto);
            TouchDown = new TDStats(Player, playerDto);
            Chaser = new ChaserStats(Player, playerDto);
            BattleRoyal = new BRStats(Player, playerDto);
            Captain = new CPTStats(Player, playerDto);
            Siege = new SiegeStats(Player, playerDto);
        }

        public void OnJoin(GameRuleBase game)
        {
            statistics = null;
            FriendlyMode = !game.CountMatch;

            if(FriendlyMode)
            {
                switch (game.GameRule)
                {
                    case GameRule.BattleRoyal:
                        statistics = new BRStats(Player);
                        break;
                    case GameRule.Captain:
                        statistics = new CPTStats(Player);
                        break;
                    case GameRule.Chaser:
                        statistics = new ChaserStats(Player);
                        break;
                    case GameRule.Deathmatch:
                        statistics = new DMStats(Player);
                        break;
                    case GameRule.Touchdown:
                        statistics = new TDStats(Player);
                        break;
                    case GameRule.Siege:
                        statistics = new SiegeStats(Player);
                        break;
                }
            }
            else
            {
                switch (game.GameRule)
                {
                    case GameRule.BattleRoyal:
                        statistics = BattleRoyal;
                        break;
                    case GameRule.Captain:
                        statistics = Captain;
                        break;
                    case GameRule.Chaser:
                        statistics = Chaser;
                        break;
                    case GameRule.Deathmatch:
                        statistics = DeathMatch;
                        break;
                    case GameRule.Touchdown:
                        statistics = TouchDown;
                        break;
                    case GameRule.Siege:
                        statistics = Siege;
                        break;
                }
            }
        }

        public ulong Won
        {
            get => statistics?.Won ?? 0;
            set
            {
                if (statistics == null || FriendlyMode)
                    return;

                Player.TotalWins++;

                statistics.Won = value;
            }
        }

        public ulong Loss
        {
            get => statistics?.Loss ?? 0;
            set
            {
                if (statistics == null || FriendlyMode)
                    return;

                Player.TotalLosses++;

                statistics.Loss = value;
            }
        }

        public ulong Kills
        {
            get => statistics?.Kills ?? 0;
            set
            {
                if (statistics == null || FriendlyMode)
                    return;

                statistics.Kills = value;
            }
        }

        public ulong KillAssists
        {
            get => statistics?.KillAssists ?? 0;
            set
            {
                if (statistics == null || FriendlyMode)
                    return;

                statistics.KillAssists = value;
            }
        }

        public ulong Deaths
        {
            get => statistics?.Deaths ?? 0;
            set
            {
                if (statistics == null || FriendlyMode)
                    return;

                statistics.Deaths = value;
            }
        }

        public ulong Heal
        {
            get => statistics?.Heal ?? 0;
            set
            {
                if (statistics == null || FriendlyMode)
                    return;

                statistics.Heal = value;
            }
        }

        public T GetStats<T>()
        {
            return (T)(object)statistics;
        }

        public void Save(IDbConnection db)
        {
            DeathMatch.Save(db);
            TouchDown.Save(db);
            Chaser.Save(db);
            BattleRoyal.Save(db);
            Captain.Save(db);
            Siege.Save(db);
        }
    }

    internal abstract class BaseStats
    {
        protected ulong _won;
        protected ulong _loss;
        protected ulong _kills;
        protected ulong _killAssists;
        protected ulong _heal;
        protected ulong _deaths;
        protected bool _needsSave;
        protected bool _existsInDatabase;

        public Player Player { get; set; }

        public ulong Won
        {
            get => _won;

            set
            {
                if (_won == value)
                    return;

                _won = value;
                _needsSave = true;
            }
        }

        public ulong Loss
        {
            get => _loss;
            set
            {
                if (_loss == value)
                    return;

                _loss = value;
                _needsSave = true;
            }
        }

        public ulong Kills
        {
            get => _kills;
            set
            {
                if (_kills == value)
                    return;

                _needsSave = true;
                _kills = value;
            }
        }

        public ulong KillAssists
        {
            get => _killAssists;
            set
            {
                if (_killAssists == value)
                    return;

                _needsSave = true;
                _killAssists = value;
            }
        }

        public ulong Deaths
        {
            get => _deaths;
            set
            {
                if (_deaths == value)
                    return;

                _needsSave = true;
                _deaths = value;
            }
        }

        public ulong Heal
        {
            get => _heal;

            set
            {
                if (_heal == value)
                    return;

                _heal = value;
                _needsSave = true;
            }
        }

        public abstract void Save(IDbConnection db);

        public float WinRate => _won + _loss > 0 ? (float)_won / (float)(_won + _loss) : 0.5f;

        public BaseStats(Player player)
        {
            Player = player;
        }
    }

    internal class DMStats : BaseStats
    {
        public float KDRate => Deaths > 0 ? ((Kills * 2) + KillAssists) / (Deaths * 2) : 1.0f;

        public DMStats(Player player)
            : base(player)
        { }

        public DMStats(Player player, PlayerDto playerDto)
            : base(player)
        {
            var dm = playerDto.DeathMatchInfo.FirstOrDefault();
            _existsInDatabase = false;
            if (dm != null)
            {
                _existsInDatabase = true;
                _won = dm.Won;
                _loss = dm.Loss;
                _kills = dm.Kills;
                _killAssists = dm.KillAssists;
                _deaths = dm.Deaths;
            }
        }

        public override void Save(IDbConnection db)
        {
            if (!_needsSave)
                return;

            var update = new PlayerDeathMatchDto
            {
                PlayerId = (int)Player.Account.Id,
                Won = Won,
                Loss = Loss,
                Kills = Kills,
                KillAssists = KillAssists,
                Deaths = Deaths
            };

            if (_existsInDatabase)
            {
                db.Update(update);
            }
            else
            {
                _existsInDatabase = true;
                db.Insert(update);
            }
        }

        public DMStatsDto GetStatsDto()
        {
            return new DMStatsDto
            {
                Won = (uint)Won,
                Lost = (uint)Loss,
                Kills = (uint)Kills,
                KillAssists = (uint)KillAssists,
                Deaths = (uint)Deaths
            };
        }

        public DMUserDataDto GetUserDataDto()
        {
            var kdr = Deaths > 0 ? ((Kills * 2.0f) + KillAssists) / (Deaths * 2.0f) : Kills > 0 ? 1.0f : 0.0f;
            var total = (float)(Won + Loss);
            var winrate = total > 0 ? Won / total : 0.5f;

            return new DMUserDataDto
            {
                KillDeath = kdr,
                WinRate = winrate
            };
        }
    }

    internal class TDStats : BaseStats
    {
        private ulong _td;
        private ulong _tdassist;
        private ulong _offense;
        private ulong _offenseAssist;
        private ulong _offenseRebound;
        private ulong _defense;
        private ulong _defenseAssist;

        public ulong TD
        {
            get => _td;

            set
            {
                if (_td == value)
                    return;

                _td = value;
                _needsSave = true;
            }
        }

        public ulong TDAssist
        {
            get => _tdassist;

            set
            {
                if (_tdassist == value)
                    return;

                _tdassist = value;
                _needsSave = true;
            }
        }

        public ulong Offense
        {
            get => _offense;

            set
            {
                if (_offense == value)
                    return;

                _offense = value;
                _needsSave = true;
            }
        }

        public ulong OffenseAssist
        {
            get => _offenseAssist;

            set
            {
                if (_offenseAssist == value)
                    return;

                _offenseAssist = value;
                _needsSave = true;
            }
        }

        public ulong OffenseRebound
        {
            get => _offenseRebound;

            set
            {
                if (_offenseRebound == value)
                    return;

                _offenseRebound = value;
                _needsSave = true;
            }
        }

        public ulong Defense
        {
            get => _defense;

            set
            {
                if (_defense == value)
                    return;

                _defense = value;
                _needsSave = true;
            }
        }

        public ulong DefenseAssist
        {
            get => _defenseAssist;

            set
            {
                if (_defenseAssist == value)
                    return;

                _defenseAssist = value;
                _needsSave = true;
            }
        }
        
        public ulong TotalScore => 5 * (TD * 2 + TDAssist) + 2 * (_kills + OffenseAssist + DefenseAssist + OffenseRebound + 2 * (Offense + Defense)) + _killAssists + Heal;

        public TDStats(Player player)
            : base(player)
        { }

        public TDStats(Player player, PlayerDto playerDto)
            : base(player)
        {
            var td = playerDto.TouchDownInfo.FirstOrDefault();
            _existsInDatabase = false;

            if (td != null)
            {
                _existsInDatabase = true;
                _won = td.Won;
                _loss = td.Loss;
                _td = td.TD;
                _tdassist = td.TDAssist;
                _offense = td.Offense;
                _offenseAssist = td.OffenseAssist;
                _offenseRebound = td.OffenseRebound;
                _defense = td.Defense;
                _defenseAssist = td.DefenseAssist;
                _kills = td.Kill;
                _killAssists = td.KillAssist;
                _heal = td.Heal;
            }
        }

        public override void Save(IDbConnection db)
        {
            if (!_needsSave)
                return;

            var update = new PlayerTouchDownDto
            {
                PlayerId = (int)Player.Account.Id,
                Won = Won,
                Loss = Loss,
                TD = TD,
                TDAssist = TDAssist,
                Offense = Offense,
                OffenseAssist = OffenseAssist,
                Defense = Defense,
                DefenseAssist = DefenseAssist,
                Kill = Kills,
                KillAssist = KillAssists,
                OffenseRebound = OffenseRebound,
                Heal = Heal
            };
            if (_existsInDatabase)
            {
                db.Update(update);
            }
            else
            {
                db.Insert(update);
                _existsInDatabase = true;
            }
        }

        public TDStatsDto GetStatsDto()
        {
            return new TDStatsDto
            {
                Unk1 = (uint)Won,//Win
                Unk2 = (uint)Loss,//Loss
                Unk3 = (uint)TD,
                Unk4 = 0, // Unk3 * 20/unk4
                Unk5 = (uint)TDAssist, // TDAssits
                Unk6 = (uint)Kills,//Kill
                Unk7 = (uint)KillAssists,//KillAssists
                Unk8 = (uint)Defense,//DefenseScore
                Unk9 = (uint)DefenseAssist,//Defense Assists
                Unk10 = (uint)Offense,//Offense
                Unk11 = (uint)OffenseAssist,//Offensive Assis
                Unk12 = (uint)OffenseRebound,//Offense Rebound
                Unk13 = (uint)Heal,//Heal x1
                Unk14 = 0,
                Unk15 = 0,
                Unk16 = 0,
                Unk17 = 0,
                Unk18 = 0
            };
        }

        public TDUserDataDto GetUserDataDto()
        {
            return new TDUserDataDto
            {
                TotalScore = TotalScore,
                DefenseScore = 2 * (Defense * 2 + DefenseAssist),
                OffenseScore = 2 * (Offense * 2 + OffenseAssist + OffenseRebound),
                KillScore = Kills * 2 + KillAssists,
                RecoveryScore = Heal,
                TDScore = 10 * TD + 5 * TDAssist,
                WinRate = WinRate
            };
        }
    }

    internal class ChaserStats :BaseStats
    {
        private ulong _chasedWon;
        private ulong _chasedRound;
        private ulong _chaserWon;
        private ulong _chaserRounds;
        
        public ulong ChasedWon
        {
            get => _chasedWon;
            set
            {
                if (_chasedWon == value)
                    return;

                _chasedWon = value;
                _needsSave = true;
            }
        }

        public ulong ChasedRounds
        {
            get => _chasedRound;
            set
            {
                if (_chasedRound == value)
                    return;

                _chasedRound = value;
                _needsSave = true;
            }
        }

        public ulong ChaserWon
        {
            get => _chaserWon;
            set
            {
                if (_chaserWon == value)
                    return;

                _chaserWon = value;
                _needsSave = true;
            }
        }

        public ulong ChaserRounds
        {
            get => _chaserRounds;
            set
            {
                if (_chaserRounds == value)
                    return;

                _chaserRounds = value;
                _needsSave = true;
            }
        }

        public ChaserStats(Player player)
            : base(player)
        { }

        public ChaserStats(Player player, PlayerDto playerDto)
            : base(player)
        {
            var chs = playerDto.ChaserInfo.FirstOrDefault();
            _existsInDatabase = false;

            if (chs != null)
            {
                _existsInDatabase = true;
                _chasedWon = chs.ChasedWon;
                _chasedRound = chs.ChasedRounds;
                _chaserWon = chs.ChaserWon;
                _chaserRounds = chs.ChasedRounds;
            }
        }

        public override void Save(IDbConnection db)
        {
            if (!_needsSave)
                return;

            var update = new PlayerChaserDto
            {
                PlayerId = (int)Player.Account.Id,
                ChasedRounds = ChasedRounds,
                ChasedWon = ChasedWon,
                ChaserRounds = ChaserRounds,
                ChaserWon = ChaserWon
            };
            if (_existsInDatabase)
            {
                db.Update(update);
            }
            else
            {
                _existsInDatabase = true;
                db.Insert(update);
            }
        }

        public ChaserStatsDto GetStatsDto()
        {
            return new ChaserStatsDto
            {
                ChasedRounds = (uint)ChasedRounds,
                ChasedWon = (uint)ChasedWon,
                ChaserRounds = (uint)ChaserRounds,
                ChaserWon = (uint)ChaserWon
            };
        }

        public ChaserUserDataDto GetUserDataDto()
        {
            return new ChaserUserDataDto
            {
                KillProbability = (ChasedRounds > 0 ? ChasedWon / ChasedRounds : 1.0f) * 100.0f,
                //SurvivalProbability = (ChaserRounds > 0 ? ChaserWon / ChaserRounds : 1.0f) * 100.0f
                Kills = 0
            };
        }
    }

    internal class BRStats : BaseStats
    {
        private ulong _firstKillAssists;
        private ulong _firstKilled;
        private ulong _firstPlace;

        public ulong FirstKilled
        {
            get => _firstKilled;
            set
            {
                if (_firstKilled == value)
                    return;

                _needsSave = true;
                _firstKilled = value;
            }
        }

        public ulong FirstKillAssists
        {
            get => _firstKillAssists;
            set
            {
                if (_firstKillAssists == value)
                    return;

                _needsSave = true;
                _firstKillAssists = value;
            }
        }

        public ulong FirstPlace
        {
            get => _firstPlace;
            set
            {
                if (_firstPlace == value)
                    return;

                _needsSave = true;
                _firstPlace = value;
            }
        }

        public BRStats(Player player)
            : base(player)
        { }

        public BRStats(Player player, PlayerDto playerDto)
            : base(player)
        {
            var br = playerDto.BattleRoyalInfo.FirstOrDefault();
            _existsInDatabase = false;

            if (br != null)
            {
                _existsInDatabase = true;
                Won = br.Won;
                Loss = br.Loss;
                FirstKilled = br.FirstKilled;
                FirstPlace = br.FirstPlace;
            }
        }

        public override void Save(IDbConnection db)
        {
            if (!_needsSave)
                return;

            var update = new PlayerBattleRoyalDto
            {
                PlayerId = (int)Player.Account.Id,
                Won = Won,
                Loss = Loss,
                KillAssists = KillAssists,
                Kills = Kills,
                FirstKilled = FirstKilled,
                FirstPlace = FirstPlace
            };

            if (_existsInDatabase)
            {
                db.Update(update);
            }
            else
            {
                db.Insert(update);
                _existsInDatabase = true;
            }
        }

        public BRStatsDto GetStatsDto()
        {
            return new BRStatsDto
            {
                Won = (uint)Won,
                Lost = (uint)Loss,
                Unk3 = 3,
                FirstKilled = (uint)FirstKilled,
                FirstPlace = (uint)FirstPlace
            };
        }

        public BRUserDataDto GetUserDataDto()
        {
            float rooms = _won + _loss;
            var score = rooms > 0 ? (_firstKilled / _firstPlace) / rooms : 0.0f;
            return new BRUserDataDto
            {
                CountFirstPlaceKilled = (uint)FirstKilled,
                CountFirstPlace = (uint)FirstPlace
            };
        }
    }

    internal class CPTStats : BaseStats
    {
        private ulong _cptKills;
        private ulong _cptCount;
        
        public ulong CPTKilled
        {
            get => _cptKills;
            set
            {
                if (_cptKills == value)
                    return;

                _needsSave = true;
                _cptKills = value;
            }
        }

        public ulong CPTCount
        {
            get => _cptCount;
            set
            {
                if (_cptCount == value)
                    return;

                _needsSave = true;
                _cptCount = value;
            }
        }

        public CPTStats(Player player)
            : base(player)
        { }

        public CPTStats(Player player, PlayerDto playerDto)
            : base(player)
        {
            var cpt = playerDto.CaptainInfo.FirstOrDefault();
            _existsInDatabase = false;

            if (cpt != null)
            {
                _existsInDatabase = true;
                _won = cpt.Won;
                _loss = cpt.Loss;
                _cptKills = cpt.CPTKilled;
                _cptCount = cpt.CPTCount;
            }
        }

        public override void Save(IDbConnection db)
        {
            if (!_needsSave)
                return;

            var update = new PlayerCaptainDto
            {
                PlayerId = (int)Player.Account.Id,
                Won = Won,
                Loss = Loss,
                CPTCount = CPTCount,
                CPTKilled = CPTKilled
            };

            if (_existsInDatabase)
            {
                db.Update(update);
            }
            else
            {
                db.Insert(update);
                _existsInDatabase = true;
            }
        }

        public CPTStatsDto GetStatsDto()
        {
            return new CPTStatsDto
            {
                Won = (uint)Won,
                Lost = (uint)Loss,
                Captain = (uint)CPTCount,
                CaptainKilled = (uint)CPTKilled
            };
        }

        public CPTUserDataDto GetUserDataDto()
        {
            float rooms = _won + _loss;

            return new CPTUserDataDto
            {
                Kills = (uint)CPTKilled,
                Domination = (uint)CPTCount
            };
        }
    }

    internal class SiegeStats : BaseStats
    {
        public SiegeStats(Player plater)
            :base(plater)
        { }

        public SiegeStats(Player player, PlayerDto playerDto)
            : base(player)
        {

        }

        public override void Save(IDbConnection db)
        {
            if (!_needsSave)
                return;

            //var update = new PlayerSiegeDto
            //{
            //    PlayerId = (int)Player.Account.Id,
            //    Won = Won,
            //    Loss = Loss,
            //    CPTCount = CPTCount,
            //    CPTKilled = CPTKilled
            //};

            //if (_existsInDatabase)
            //{
            //    db.Update(update);
            //}
            //else
            //{
            //    db.Insert(update);
            //    _existsInDatabase = true;
            //}
        }

        public SiegeStatsDto GetStatsDto()
        {
            return new SiegeStatsDto
            {
                Unk1 = 1,
                Unk2 = 2,
                Unk3 = 3,
                Unk4 = 4,
                Unk5 = 5,
                Unk6 = 6,
                Unk7 = 7,
                Unk8 = 8,
                Unk9 = 9,
                Unk10 = 10,
                Unk11 = 11
            };
        }

        public SiegeUserDataDto GetUserDataDto()
        {
            return new SiegeUserDataDto
            {
                WinRate = 0.5f,
                BattleScore = 0,
                CaptureScore = 0,
                ItemObtainScore = 0,
                MainCoreCaptureScore = 0
            };
        }
    }
}
