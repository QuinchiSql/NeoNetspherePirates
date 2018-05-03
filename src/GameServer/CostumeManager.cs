using System;
using System.Collections.Generic;
using System.Linq;
using NeoNetsphere.Database.Game;
using NeoNetsphere.Network.Message.Game;
using Netsphere;

namespace NeoNetsphere
{
    internal class CostumeManager
    {
        private readonly Character _character;
        private readonly PlayerItem[] _items = new PlayerItem[8];
        private readonly object _sync = new object();

        internal CostumeManager(Character @char, PlayerCharacterDto dto)
        {
            _character = @char;
            var plr = _character.CharacterManager.Player;

            _items[0] = plr.Inventory[(ulong) (dto.HairId ?? 0)];
            _items[1] = plr.Inventory[(ulong) (dto.FaceId ?? 0)];
            _items[2] = plr.Inventory[(ulong) (dto.ShirtId ?? 0)];
            _items[3] = plr.Inventory[(ulong) (dto.PantsId ?? 0)];
            _items[4] = plr.Inventory[(ulong) (dto.GlovesId ?? 0)];
            _items[5] = plr.Inventory[(ulong) (dto.ShoesId ?? 0)];
            _items[6] = plr.Inventory[(ulong) (dto.AccessoryId ?? 0)];
            _items[7] = plr.Inventory[(ulong) (dto.PetId ?? 0)];
        }

        internal CostumeManager(Character @char)
        {
            _character = @char;
        }

        public void Equip(PlayerItem item, CostumeSlot slot)
        {
            lock (_sync)
            {
                if (item == null)
                    throw new ArgumentNullException(nameof(item));

                if (!CanEquip(item, slot))
                    throw new CharacterException($"Cannot equip item {item.ItemNumber} on slot {slot}");

                switch (slot)
                {
                    case CostumeSlot.Hair:
                    case CostumeSlot.Face:
                    case CostumeSlot.Shirt:
                    case CostumeSlot.Pants:
                    case CostumeSlot.Gloves:
                    case CostumeSlot.Shoes:
                    case CostumeSlot.Accessory:
                    case CostumeSlot.Pet:
                        if (_items[(int) slot] != item)
                        {
                            _character.NeedsToSave = true;
                            _items[(int) slot] = item;
                        }

                        break;

                    default:
                        throw new CharacterException("Invalid slot: " + (byte) slot);
                }

                var plr = _character.CharacterManager.Player;
                plr.Session.SendAsync(new ItemUseItemAckMessage
                {
                    CharacterSlot = _character.Slot,
                    ItemId = item?.Id ?? 0,
                    Action = UseItemAction.Equip,
                    EquipSlot = (byte) slot
                });
            }
        }

        public void UnEquip(CostumeSlot slot)
        {
            lock (_sync)
            {
                var plr = _character.CharacterManager.Player;
                if (plr.Room != null && plr.RoomInfo.State != PlayerState.Lobby) // Cant change items while playing
                    throw new CharacterException("Can't change items while playing");

                PlayerItem item;
                switch (slot)
                {
                    case CostumeSlot.Hair:
                    case CostumeSlot.Face:
                    case CostumeSlot.Shirt:
                    case CostumeSlot.Pants:
                    case CostumeSlot.Gloves:
                    case CostumeSlot.Shoes:
                    case CostumeSlot.Accessory:
                    case CostumeSlot.Pet:
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

        public PlayerItem GetItem(CostumeSlot slot)
        {
            lock (_sync)
            {
                switch (slot)
                {
                    case CostumeSlot.Hair:
                    case CostumeSlot.Face:
                    case CostumeSlot.Shirt:
                    case CostumeSlot.Pants:
                    case CostumeSlot.Gloves:
                    case CostumeSlot.Shoes:
                    case CostumeSlot.Accessory:
                    case CostumeSlot.Pet:
                        return _items[(int) slot];

                    default:
                        throw new CharacterException("Invalid slot: " + slot);
                }
            }
        }

        public IReadOnlyList<PlayerItem> GetItems()
        {
            lock (_sync)
            {
                return _items;
            }
        }

        public bool CanEquip(PlayerItem item, CostumeSlot slot)
        {
            lock (_sync)
            {
                // ReSharper disable once UseNullPropagation
                if (item == null)
                    return false;

                if (item.ItemNumber.Category != ItemCategory.Costume)
                    return false;

                if (slot > CostumeSlot.Pet || slot < CostumeSlot.Hair)
                    return false;

                if (_items[(int) slot] != null) // Slot needs to be empty
                    return false;

                var plr = _character.CharacterManager.Player;
                if (plr.Room != null && plr.RoomInfo.State != PlayerState.Lobby) // Cant change items while playing
                    return false;

                foreach (var @char in plr.CharacterManager)
                    if (@char.Costumes.GetItems().Any(i => i?.Id == item.Id)
                    ) // Dont allow items that are already equipped on a character
                        return false;

                return true;
            }
        }
    }
}
