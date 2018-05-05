using System;
using System.Collections.Generic;
using System.Linq;
using BlubLib.Threading.Tasks;
using NeoNetsphere.Database.Game;
using NeoNetsphere.Network.Message.Game;
using Netsphere;

namespace NeoNetsphere
{
    internal class WeaponManager
    {
        private readonly Character _character;
        public readonly PlayerItem[] _items = new PlayerItem[3];
        internal readonly AsyncLock _sync = new AsyncLock();

        internal WeaponManager(Character @char, PlayerCharacterDto dto)
        {
            _character = @char;
            var plr = _character.CharacterManager.Player;

            _items[0] = plr.Inventory[(ulong) (dto.Weapon1Id ?? 0)];
            _items[1] = plr.Inventory[(ulong) (dto.Weapon2Id ?? 0)];
            _items[2] = plr.Inventory[(ulong) (dto.Weapon3Id ?? 0)];
        }

        internal WeaponManager(Character @char)
        {
            _character = @char;
        }

        public void Equip(PlayerItem item, WeaponSlot slot)
        {
            //using (_sync.Lock())
            {
                if (item == null)
                    throw new ArgumentNullException(nameof(item));

                if (!CanEquip(item, slot))
                    throw new CharacterException($"Cannot equip item {item.ItemNumber} on slot {slot}");

                switch (slot)
                {
                    case WeaponSlot.Weapon1:
                    case WeaponSlot.Weapon2:
                    case WeaponSlot.Weapon3:
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

        public void UnEquip(WeaponSlot slot)
        {
            //using (_sync.Lock())
            {
                var plr = _character.CharacterManager.Player;
                if (plr.Room != null && plr.RoomInfo.State != PlayerState.Lobby) // Cant change items while playing
                    throw new CharacterException("Can't change items while playing");

                PlayerItem item;
                switch (slot)
                {
                    case WeaponSlot.Weapon1:
                    case WeaponSlot.Weapon2:
                    case WeaponSlot.Weapon3:
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

        public PlayerItem GetItem(WeaponSlot slot)
        {
            //using (_sync.Lock())
            {
                switch (slot)
                {
                    case WeaponSlot.Weapon1:
                    case WeaponSlot.Weapon2:
                    case WeaponSlot.Weapon3:
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

        public bool CanEquip(PlayerItem item, WeaponSlot slot)
        {
            // ReSharper disable once UseNullPropagation
            if (item == null)
                return false;

            if (item.ItemNumber.Category != ItemCategory.Weapon)
                return false;

            if (slot < WeaponSlot.Weapon1 || slot > WeaponSlot.Weapon3)
                return false;

            if (_items[(int) slot] != null) // Slot needs to be empty
                return false;

            var plr = _character.CharacterManager.Player;
            if (plr.Room != null && plr.RoomInfo.State != PlayerState.Lobby) // Cant change items while playing
                return false;

            foreach (var @char in plr.CharacterManager)
                if (@char.Weapons.GetItems().Any(i => i?.Id == item.Id)
                ) // Dont allow items that are already equipped on a character
                    return false;

            return true;
        }
    }
}
