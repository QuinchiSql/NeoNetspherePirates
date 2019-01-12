using System;
using System.Collections.Generic;
using Dapper.FastCrud;
using ExpressMapper.Extensions;
using NeoNetsphere.Database.Game;
using NeoNetsphere.Network;
using NeoNetsphere.Network.Data.Game;
using NeoNetsphere.Network.Message.Game;
using Serilog;
using Serilog.Core;

namespace NeoNetsphere
{
    internal class Player
    {
        // ReSharper disable once InconsistentNaming
        private static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(Player));

        private uint _ap;
        private uint _coins1;
        private uint _coins2;
        private byte _level;
        private uint _pen;
        private string _playtime;
        private uint _totalExperience;
        private uint _totallosses;
        private uint _totalwins;
        private byte _tutorialState;
        public StatsManager stats;

        public bool LoggedIn = false;

        public Player(GameSession session, Account account, PlayerDto dto)
        {
            Session = session;
            Account = account;

            _playtime = dto.PlayTime;
            _tutorialState = dto.TutorialState;
            _level = dto.Level;
            _totalExperience = dto.TotalExperience;
            _pen = (uint) dto.PEN;
            _ap = (uint) dto.AP;
            _coins1 = (uint) dto.Coins1;
            _coins2 = (uint) dto.Coins2;
            TotalMatches = (uint) dto.TotalMatches;
            _totallosses = (uint) dto.TotalLosses;
            _totalwins = (uint) dto.TotalWins;

            Settings = new PlayerSettingManager(this, dto);
            DenyManager = new DenyManager(this, dto);
            Mailbox = new Mailbox(this, dto);
            stats = new StatsManager(this, dto);

            Inventory = new Inventory(this, dto);
            CharacterManager = new CharacterManager(this, dto);
            Club = GameServer.Instance.ClubManager.GetClubByAccount(account.Id);

            RoomInfo = new PlayerRoomInfo();
        }

        /// <summary>
        ///     Gains experiences and levels up if the player earned enough experience
        /// </summary>
        /// <param name="amount">Amount of experience to earn</param>
        /// <returns>true if the player leveled up</returns>
        public bool GainExp(int amount)
        {
            Logger.ForAccount(this)
                .Debug("Gained {amount} exp", amount);

            var expTable = GameServer.Instance.ResourceCache.GetExperience();
            var expInfo = expTable.GetValueOrDefault(Level);
            if (expInfo == null)
            {
                Logger.ForAccount(this)
                    .Warning("Level {level} not found", Level);

                return false;
            }

            // We cant earn exp when we reached max level
            if (expInfo.ExperienceToNextLevel == 0 || Level >= Config.Instance.Game.MaxLevel)
                return false;

            var leveledUp = false;
            TotalExperience += (uint) amount;


            // Did we level up?
            // Using a loop for multiple level ups
            while (expInfo.ExperienceToNextLevel != 0 &&
                   expInfo.ExperienceToNextLevel <= (int) (TotalExperience - expInfo.TotalExperience))
            {
                var newLevel = Level + 1;
                expInfo = expTable.GetValueOrDefault(newLevel);

                if (expInfo == null)
                {
                    Logger.ForAccount(this)
                        .Warning("Can't level up because level {level} not found", newLevel);
                    break;
                }

                Logger.ForAccount(this)
                    .Debug("Leveled up to {level}", newLevel);

                // ToDo level rewards

                Level++;
                leveledUp = true;
            }


            Session.SendAsync(new ExpRefreshInfoAckMessage(TotalExperience));
            Session.SendAsync(new PlayerAccountInfoAckMessage(this.Map<Player, PlayerAccountInfoDto>()));
            return leveledUp;
        }

        /// <param name="attribute">The attribute to retrieve</param>
        /// <returns></returns>
        /// <summary>
        ///     Sends a message to the game master console
        /// </summary>
        /// <param name="message">The message to send</param>
        public void SendConsoleMessage(string message)
        {
            Session.SendAsync(new AdminActionAckMessage {Result = 0, Message = message});
        }

        /// <summary>
        ///     Sends a notice message
        /// </summary>
        /// <param name="message">The message to send</param>
        public void SendNotice(string message)
        {
            Session.SendAsync(new NoticeAdminMessageAckMessage(message));
        }

        /// <summary>
        ///     Saves all pending changes to the database
        /// </summary>
        public void Save()
        {
            using (var db = GameDatabase.Open())
            {
                if (NeedsToSave)
                {
                    db.Update(new PlayerDto
                    {
                        Id = (int) Account.Id,
                        PlayTime = PlayTime,
                        TutorialState = TutorialState,
                        Level = Level,
                        TotalExperience = TotalExperience,
                        PEN = (int) PEN,
                        AP = (int) AP,
                        Coins1 = (int) Coins1,
                        Coins2 = (int) Coins2,
                        CurrentCharacterSlot = CharacterManager.CurrentSlot,
                        TotalMatches = (int) (TotalWins + TotalLosses),
                        TotalLosses = (int) TotalLosses,
                        TotalWins = (int) TotalWins
                    });
                    NeedsToSave = false;
                }

                Settings.Save(db);
                Inventory.Save(db);
                CharacterManager.Save(db);
                DenyManager.Save(db);
                Mailbox.Save(db);
                stats.Save(db);
            }
        }

        /// <summary>
        ///     Disconnects the player
        /// </summary>
        public void Disconnect()
        {
            Session?.Dispose();
        }

        #region Properties

        internal bool NeedsToSave { get; set; }

        public GameSession Session { get; set; }
        public ChatSession ChatSession { get; set; }
        public RelaySession RelaySession { get; set; }

        public PlayerSettingManager Settings { get; }

        public DenyManager DenyManager { get; }
        public Mailbox Mailbox { get; }

        public Account Account { get; set; }
        public Club Club { get; set; }
        public CharacterManager CharacterManager { get; }
        public Inventory Inventory { get; }
        public Channel Channel { get; internal set; }

        public Room Room { get; internal set; }
        public PlayerRoomInfo RoomInfo { get; }
        public PlayerLocationInfo LocationInfo { get; set; } = new PlayerLocationInfo();

        internal bool SentPlayerList { get; set; }

        public string PlayTime
        {
            get
            {
                if (_playtime == "")
                    _playtime = TimeSpan.FromSeconds(0).ToString();
                return _playtime;
            }
            set
            {
                if (_playtime == value)
                    return;
                _playtime = value;
                NeedsToSave = true;
            }
        }

        public byte TutorialState
        {
            get => _tutorialState;
            set
            {
                if (_tutorialState == value)
                    return;
                _tutorialState = value;
                NeedsToSave = true;
            }
        }

        public byte Level
        {
            get => _level;
            set
            {
                if (_level == value)
                    return;
                _level = value;
                NeedsToSave = true;
            }
        }

        public uint TotalExperience
        {
            get => _totalExperience;
            set
            {
                if (_totalExperience == value)
                    return;
                _totalExperience = value;
                NeedsToSave = true;
            }
        }

        public uint PEN
        {
            get => _pen;
            set
            {
                if (_pen == value)
                    return;
                _pen = value;
                NeedsToSave = true;
            }
        }

        public uint AP
        {
            get => _ap;
            set
            {
                if (_ap == value)
                    return;
                _ap = value;
                NeedsToSave = true;
            }
        }

        public uint Coins1
        {
            get => _coins1;
            set
            {
                if (_coins1 == value)
                    return;
                _coins1 = value;
                NeedsToSave = true;
            }
        }

        public uint Coins2
        {
            get => _coins2;
            set
            {
                if (_coins2 == value)
                    return;
                _coins2 = value;
                NeedsToSave = true;
            }
        }

        public uint TotalWins
        {
            get => _totalwins;
            set
            {
                if (_totalwins == value)
                    return;
                _totalwins = value;
                TotalMatches = _totalwins + _totallosses;
                NeedsToSave = true;
            }
        }

        public uint TotalLosses
        {
            get => _totallosses;
            set
            {
                if (_totallosses == value)
                    return;
                _totallosses = value;
                TotalMatches = _totalwins + _totallosses;
                NeedsToSave = true;
            }
        }

        public uint TotalMatches { get; private set; }

        #endregion

        //private static int GetAttributeValueFromItems(Attribute attribute, IEnumerable<PlayerItem> items)
        //{
        //    return items.Where(item => item != null)
        //        .Select(item => item.GetItemEffects())
        //        .Where(effect => effect != null)
        //        .SelectMany(effect => effect.Attributes)
        //        .Where(attrib => attrib.Attribute == attribute)
        //        .Sum(attrib => attrib.Value);
        //}
        //
        //private static float GetAttributeRateFromItems(Attribute attribute, IEnumerable<PlayerItem> items)
        //{
        //    return items.Where(item => item != null)
        //        .Select(item => item.GetItemEffects())
        //        .Where(effect => effect != null)
        //        .SelectMany(effect => effect.Attributes)
        //        .Where(attrib => attrib.Attribute == attribute)
        //        .Sum(attrib => attrib.Rate);
        //}
    }
}
