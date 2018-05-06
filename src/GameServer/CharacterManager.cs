using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlubLib.Threading.Tasks;
using Dapper.FastCrud;
using NeoNetsphere.Database.Game;
using NeoNetsphere.Network;
using NeoNetsphere.Network.Message.Game;
using NeoNetsphere.Resource;
using Netsphere;
using Serilog;
using Serilog.Core;

namespace NeoNetsphere
{
    internal class CharacterManager : IReadOnlyCollection<Character>
    {
        // ReSharper disable once InconsistentNaming
        private static readonly ILogger Logger =
            Log.ForContext(Constants.SourceContextPropertyName, nameof(CharacterManager));

        private readonly Dictionary<byte, Character> _characters = new Dictionary<byte, Character>();

        private readonly ConcurrentStack<Character> _charactersToDelete = new ConcurrentStack<Character>();
        private readonly AsyncLock _sync = new AsyncLock();

        internal CharacterManager(Player plr, PlayerDto dto)
        {
            Player = plr;
            CurrentSlot = dto.CurrentCharacterSlot;

            foreach (var @char in dto.Characters.Select(@char => new Character(this, @char)))
                if (!_characters.TryAdd(@char.Slot, @char))
                    Logger
                        .ForAccount(Player)
                        .Warning("Multiple characters on slot {slot}", @char.Slot);
        }

        public Player Player { get; }
        public Character CurrentCharacter => GetCharacter(CurrentSlot);
        public byte CurrentSlot { get; private set; }

        /// <summary>
        ///     Returns the character on the given slot.
        ///     Returns null if the character does not exist
        /// </summary>
        public Character this[byte slot] => GetCharacter(slot);

        public int Count => _characters.Count;

        public IEnumerator<Character> GetEnumerator()
        {
            //using (_sync.Lock())
            {
                return _characters.Values.GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            //using (_sync.Lock())
            {
                return GetEnumerator();
            }
        }

        /// <summary>
        ///     Returns the character on the given slot.
        ///     Returns null if the character does not exist
        /// </summary>
        public Character GetCharacter(byte slot)
        {
            //using (_sync.Lock())
            {
                return _characters.GetValueOrDefault(slot);
            }
        }

        /// <summary>
        ///     Creates a new character
        /// </summary>
        /// <exception cref="CharacterException"></exception>
        public Character Create(byte slot, CharacterGender gender, byte hair, byte face, byte shirt, byte pants)
        {
            //using (_sync.Lock())
            {
                if (Count >= 3)
                    throw new CharacterException("Character limit reached");

                if (_characters.ContainsKey(slot))
                    throw new CharacterException($"Slot {slot} is already in use");

                var defaultItems = GameServer.Instance.ResourceCache.GetDefaultItems();
                if (defaultItems.Get(gender, CostumeSlot.Hair, hair) == null)
                    throw new CharacterException($"Hair variation {hair} does not exist");

                if (defaultItems.Get(gender, CostumeSlot.Face, face) == null)
                    throw new CharacterException($"Face variation {face} does not exist");

                if (defaultItems.Get(gender, CostumeSlot.Shirt, shirt) == null)
                    throw new CharacterException($"Shirt variation {shirt} does not exist");

                if (defaultItems.Get(gender, CostumeSlot.Pants, pants) == null)
                    throw new CharacterException($"Pants variation {pants} does not exist");

                var @char = new Character(this, slot, gender, hair, face, shirt, pants);
                _characters.Add(slot, @char);

                var charStyle = new CharacterStyle(@char.Gender, @char.Hair.Variation, @char.Face.Variation,
                    @char.Shirt.Variation, @char.Pants.Variation, @char.Slot);
                Player.Session.SendAsync(new CSuccessCreateCharacterAckMessage(@char.Slot, charStyle));

                return @char;
            }
        }

        public Character CreateFirst(byte slot, CharacterGender gender, byte hair, byte face, byte shirt, byte pants)
        {
            //using (_sync.Lock())
            {
                if (Count >= 3)
                    throw new CharacterException("Character limit reached");

                if (_characters.ContainsKey(slot))
                    throw new CharacterException($"Slot {slot} is already in use");

                var defaultItems = GameServer.Instance.ResourceCache.GetDefaultItems();
                if (defaultItems.Get(gender, CostumeSlot.Hair, hair) == null)
                    throw new CharacterException($"Hair variation {hair} does not exist");

                if (defaultItems.Get(gender, CostumeSlot.Face, face) == null)
                    throw new CharacterException($"Face variation {face} does not exist");

                if (defaultItems.Get(gender, CostumeSlot.Shirt, shirt) == null)
                    throw new CharacterException($"Shirt variation {shirt} does not exist");

                if (defaultItems.Get(gender, CostumeSlot.Pants, pants) == null)
                    throw new CharacterException($"Pants variation {pants} does not exist");

                var @char = new Character(this, slot, gender, hair, face, shirt, pants);
                _characters.Add(slot, @char);

                var charStyle = new CharacterStyle(@char.Gender, @char.Hair.Variation, @char.Face.Variation,
                    @char.Shirt.Variation, @char.Pants.Variation, @char.Slot);

                return @char;
            }
        }

        /// <summary>
        ///     Selects the character on the given slot
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public async Task Select(byte slot)
        {
            //using (_sync.Lock())
            {
                if (!Contains(slot))
                    throw new ArgumentException($"Slot {slot} does not exist", nameof(slot));

                if (CurrentSlot != slot)
                    Player.NeedsToSave = true;

                CurrentSlot = slot;
                Player.Session.SendAsync(new CharacterSelectAckMessage(CurrentSlot));
            }
        }

        public bool CheckChars()
        {
            var works = false;
            for (var i = 0; i <= 3; i++)
                if (Contains((byte) i))
                    works = true;
            return works;
        }

        /// <summary>
        ///     Removes the character
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public async Task Remove(Character @char)
        {
            //using (_sync.Lock())
            {
                Remove(@char.Slot);
            }
        }

        /// <summary>
        ///     Removes the character on the given slot
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public async Task Remove(byte slot)
        {
            //using (_sync.Lock())
            {
                var @char = GetCharacter(slot);
                if (@char == null)
                    throw new ArgumentException($"Slot {slot} does not exist", nameof(slot));

                _characters.Remove(slot);
                if (@char.ExistsInDatabase)
                    _charactersToDelete.Push(@char);
                Player.Session.SendAsync(new CharacterDeleteAckMessage(slot));
            }
        }

        internal async Task Save(IDbConnection db)
        {
            //using (_sync.Lock())
            {
                if (!_charactersToDelete.IsEmpty)
                {
                    var idsToRemove = new StringBuilder();
                    var firstRun = true;
                    while (_charactersToDelete.TryPop(out var charToDelete))
                    {
                        if (firstRun)
                            firstRun = false;
                        else
                            idsToRemove.Append(',');
                        idsToRemove.Append(charToDelete.Id);
                    }

                    db.BulkDelete<PlayerCharacterDto>(statement => statement
                        .Where($"{nameof(PlayerCharacterDto.Id):C} IN ({idsToRemove})"));
                }

                foreach (var @char in _characters.Values)
                    if (!@char.ExistsInDatabase)
                    {
                        var charDto = new PlayerCharacterDto
                        {
                            Id = @char.Id,
                            PlayerId = (int) Player.Account.Id,
                            Slot = @char.Slot,
                            Gender = (byte) @char.Gender,
                            BasicHair = @char.Hair.Variation,
                            BasicFace = @char.Face.Variation,
                            BasicShirt = @char.Shirt.Variation,
                            BasicPants = @char.Pants.Variation
                        };
                        SetDtoItems(@char, charDto);
                        db.Insert(charDto);
                        @char.ExistsInDatabase = true;
                    }
                    else
                    {
                        if (!@char.NeedsToSave)
                            continue;

                        var charDto = new PlayerCharacterDto
                        {
                            Id = @char.Id,
                            PlayerId = (int) Player.Account.Id,
                            Slot = @char.Slot,
                            Gender = (byte) @char.Gender,
                            BasicHair = @char.Hair.Variation,
                            BasicFace = @char.Face.Variation,
                            BasicShirt = @char.Shirt.Variation,
                            BasicPants = @char.Pants.Variation
                        };
                        SetDtoItems(@char, charDto);
                        db.Update(charDto);
                        @char.NeedsToSave = false;
                    }
            }
        }

        public bool Contains(byte slot)
        {
            return _characters.ContainsKey(slot);
        }

        private void SetDtoItems(Character @char, PlayerCharacterDto charDto)
        {
            PlayerItem item;

            // Weapons
            for (var slot = WeaponSlot.Weapon1; slot <= WeaponSlot.Weapon3; slot++)
            {
                item = @char.Weapons.GetItem(slot);
                var itemId = item != null ? (int?) item.Id : null;

                switch (slot)
                {
                    case WeaponSlot.Weapon1:
                        charDto.Weapon1Id = itemId;
                        break;

                    case WeaponSlot.Weapon2:
                        charDto.Weapon2Id = itemId;
                        break;

                    case WeaponSlot.Weapon3:
                        charDto.Weapon3Id = itemId;
                        break;
                }
            }

            // Skills
            item = @char.Skills.GetItem(SkillSlot.Skill);
            charDto.SkillId = item != null ? (int?) item.Id : null;

            // Costumes
            for (var slot = CostumeSlot.Hair; slot <= CostumeSlot.Pet; slot++)
            {
                item = @char.Costumes.GetItem(slot);
                var itemId = item != null ? (int?) item.Id : null;

                switch (slot)
                {
                    case CostumeSlot.Hair:
                        charDto.HairId = itemId;
                        break;

                    case CostumeSlot.Face:
                        charDto.FaceId = itemId;
                        break;

                    case CostumeSlot.Shirt:
                        charDto.ShirtId = itemId;
                        break;

                    case CostumeSlot.Pants:
                        charDto.PantsId = itemId;
                        break;

                    case CostumeSlot.Gloves:
                        charDto.GlovesId = itemId;
                        break;

                    case CostumeSlot.Shoes:
                        charDto.ShoesId = itemId;
                        break;

                    case CostumeSlot.Accessory:
                        charDto.AccessoryId = itemId;
                        break;

                    case CostumeSlot.Pet:
                        charDto.PetId = itemId;
                        break;
                }
            }
        }
    }
}
