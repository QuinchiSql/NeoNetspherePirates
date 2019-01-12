using System;
using System.Collections.Generic;
using System.Linq;
using BlubLib.Threading.Tasks;
using NeoNetsphere.Database.Game;
using NeoNetsphere.Network.Message.Game;
using Netsphere;

namespace NeoNetsphere
{
    internal class SkillManager
    {
        private readonly Character _character;
        public readonly PlayerItem[] _items = new PlayerItem[2];
        internal readonly AsyncLock _sync = new AsyncLock();

        internal SkillManager(Character @char, PlayerCharacterDto dto)
        {
            _character = @char;
            var plr = _character.CharacterManager.Player;

            _items[0] = plr.Inventory[(ulong) (dto.SkillId ?? 0)];
        }

        internal SkillManager(Character @char)
        {
            _character = @char;
        }

        public void Equip(PlayerItem item, SkillSlot slot)
        {
            //using (_sync.Lock())
            {
                if (item == null)
                    throw new ArgumentNullException(nameof(item));

                if (!CanEquip(item, slot))
                    throw new CharacterException($"Cannot equip item {item.ItemNumber} on slot {slot}");

                switch (slot)
                {
                    case SkillSlot.Skill:
                        if (_items[(int) slot] != item)
                        {
                            _character.NeedsToSave = true;
                            _items[(int) slot] = item;
                        }

                        break;

                    default:
                        throw new CharacterException("Invalid slot: " + slot);
                }

                var plr = _character.CharacterManager.Player;
                plr.Session.SendAsync(new ItemUseItemAckMessage
                {
                    CharacterSlot = _character.Slot,
                    ItemId = item.Id,
                    Action = UseItemAction.Equip,
                    EquipSlot = (byte) slot
                });
            }
        }

        public void UnEquip(SkillSlot slot)
        {
            //using (_sync.Lock())
            {
                var plr = _character.CharacterManager.Player;
                if (plr.Room != null && plr.RoomInfo.State != PlayerState.Lobby) // Cant change items while playing
                    throw new CharacterException("Can't change items while playing");

                PlayerItem item;
                switch (slot)
                {
                    case SkillSlot.Skill:
                        item = _items[(int) slot];
                        if (item != null)
                        {
                            _character.NeedsToSave = true;
                            _items[(int) slot] = null;
                        }

                        break;

                    default:
                        throw new CharacterException("Invalid slot: " + slot);
                }

                plr.Session.SendAsync(new ItemUseItemAckMessage
                {
                    CharacterSlot = _character.Slot,
                    ItemId = item?.Id ?? 0,
                    Action = UseItemAction.UnEquip,
                    EquipSlot = (byte) slot
                });
            }
        }

        public PlayerItem GetItem(SkillSlot slot)
        {
            //using (_sync.Lock())
            {
                switch (slot)
                {
                    case SkillSlot.Skill:
                        return _items[(int) slot];

                    default:
                        throw new CharacterException("Invalid slot: " + slot);
                }
            }
        }

        public IReadOnlyList<PlayerItem> GetItems()
        {
            //using (_sync.Lock())
            {
                return _items;
            }
        }

        public bool CanEquip(PlayerItem item, SkillSlot slot)
        {
            //using (_sync.Lock())
            {
                // ReSharper disable once UseNullPropagation
                if (item == null)
                    return false;

                if (item.ItemNumber.Category != ItemCategory.Skill)
                    return false;

                if (slot != SkillSlot.Skill)
                    return false;

                if (_items[(int) slot] != null) // Slot needs to be empty
                    return false;

                var plr = _character.CharacterManager.Player;
                if (plr.Room != null && plr.RoomInfo.State != PlayerState.Lobby) // Cant change items while playing
                    return false;

                foreach (var @char in plr.CharacterManager)
                    if (@char.Skills.GetItems().Any(i => i?.Id == item.Id)
                    ) // Dont allow items that are already equipped on a character
                        return false;

                return true;
            }
        }
    }
}
