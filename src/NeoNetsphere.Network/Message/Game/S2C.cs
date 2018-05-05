using BlubLib.Serialization;
using NeoNetsphere.Network.Data.Game;
using NeoNetsphere.Network.Serializers;
using ProudNetSrc.Serialization.Serializers;
using System;
using System.Net;

namespace NeoNetsphere.Network.Message.Game
{
    [BlubContract]
    public class LoginReguestAckMessage : IGameMessage
    {
        public LoginReguestAckMessage()
        {
            Unk1 = "";
            Unk2 = "";
            ServerTime = DateTimeOffset.Now;
        }

        public LoginReguestAckMessage(GameLoginResult result, ulong accountId)
            : this()
        {
            AccountId = accountId;
            Result = result;
        }

        public LoginReguestAckMessage(GameLoginResult result)
            : this()
        {
            Result = result;
        }

        [BlubMember(0)]
        public ulong AccountId { get; set; }

        [BlubMember(1)]
        public GameLoginResult Result { get; set; }

        [BlubMember(2, typeof(UnixTimeSerializer))]
        public DateTimeOffset ServerTime { get; set; }

        [BlubMember(3, typeof(StringSerializer))]
        public string Unk1 { get; set; }

        [BlubMember(4, typeof(StringSerializer))]
        public string Unk2 { get; set; }
    }

    [BlubContract]
    public class PlayerAccountInfoAckMessage : IGameMessage
    {
        public PlayerAccountInfoAckMessage()
        {
            Info = new PlayerAccountInfoDto();
        }

        public PlayerAccountInfoAckMessage(PlayerAccountInfoDto info)
        {
            Info = info;
        }

        [BlubMember(0)]
        public PlayerAccountInfoDto Info { get; set; }
    }

    [BlubContract]
    public class CharacterCurrentInfoAckMessage : IGameMessage
    {
        public CharacterCurrentInfoAckMessage()
        {
            Unk1 = 1; // max skill?
            Unk2 = 3; // max weapons?
        }

        [BlubMember(0)]
        public byte Slot { get; set; }

        [BlubMember(1)]
        public byte Unk1 { get; set; }

        [BlubMember(2)]
        public byte Unk2 { get; set; }

        [BlubMember(3)]
        public CharacterStyle Style { get; set; }
    }

    [BlubContract]
    public class CharacterCurrentItemInfoAckMessage : IGameMessage
    {
        public CharacterCurrentItemInfoAckMessage()
        {
            Weapons = new ulong[9];
            Skills = new ulong[1];
            Clothes = new ulong[7];
        }

        [BlubMember(0)]
        public byte Slot { get; set; }

        [BlubMember(3, typeof(ArrayWithIntPrefixAndIndexSerializer))]
        public ulong[] Weapons { get; set; }

        [BlubMember(4, typeof(ArrayWithIntPrefixAndIndexSerializer))]
        public ulong[] Skills { get; set; }

        [BlubMember(5, typeof(ArrayWithIntPrefixAndIndexSerializer))]
        public ulong[] Clothes { get; set; }
    }

    [BlubContract]
    public class ItemInventoryInfoAckMessage : IGameMessage
    {
        public ItemInventoryInfoAckMessage()
        {
            Items = Array.Empty<ItemDto>();
        }

        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public ItemDto[] Items { get; set; }
    }

    [BlubContract]
    public class CharacterDeleteAckMessage : IGameMessage
    {
        public CharacterDeleteAckMessage()
        {
        }

        public CharacterDeleteAckMessage(byte slot)
        {
            Slot = slot;
        }

        [BlubMember(0)]
        public byte Slot { get; set; }
    }

    [BlubContract]
    public class CharacterSelectAckMessage : IGameMessage
    {
        public CharacterSelectAckMessage()
        {
        }

        public CharacterSelectAckMessage(byte slot)
        {
            Slot = slot;
        }

        [BlubMember(0)]
        public byte Slot { get; set; }
    }

    [BlubContract]
    public class CSuccessCreateCharacterAckMessage : IGameMessage
    {
        public CSuccessCreateCharacterAckMessage()
        {
            MaxSkills = 1;
            MaxWeapons = 3;
        }

        public CSuccessCreateCharacterAckMessage(byte slot, CharacterStyle style)
            : this()
        {
            Slot = slot;
            Style = style;
        }

        [BlubMember(0)]
        public byte Slot { get; set; }

        [BlubMember(1)]
        public CharacterStyle Style { get; set; }

        [BlubMember(2)]
        public byte MaxSkills { get; set; }

        [BlubMember(3)]
        public byte MaxWeapons { get; set; }
    }

    [BlubContract]
    public class ServerResultAckMessage : IGameMessage
    {
        public ServerResultAckMessage()
        {
        }

        public ServerResultAckMessage(ServerResult result)
        {
            Result = result;
        }

        [BlubMember(0)]
        public ServerResult Result { get; set; }
    }

    [BlubContract]
    public class NickCheckAckMessage : IGameMessage
    {
        public NickCheckAckMessage()
        {
        }

        public NickCheckAckMessage(bool isTaken)
        {
            IsTaken = isTaken;
        }

        [BlubMember(0, typeof(IntBooleanSerializer))]
        public bool IsTaken { get; set; }
    }

    [BlubContract]
    public class ItemUseItemAckMessage : IGameMessage
    {
        public ItemUseItemAckMessage()
        {
        }

        public ItemUseItemAckMessage(UseItemAction action, byte characterSlot, byte equipSlot, ulong itemId)
        {
            CharacterSlot = characterSlot;
            EquipSlot = equipSlot;
            ItemId = itemId;
            Action = action;
        }

        [BlubMember(0)]
        public byte CharacterSlot { get; set; }

        [BlubMember(1)]
        public byte EquipSlot { get; set; }

        [BlubMember(2)]
        public ulong ItemId { get; set; }

        [BlubMember(3)]
        public UseItemAction Action { get; set; }
    }

    [BlubContract]
    public class ItemUpdateInventoryAckMessage : IGameMessage
    {
        public ItemUpdateInventoryAckMessage()
        {
            Item = new ItemDto();
        }

        public ItemUpdateInventoryAckMessage(InventoryAction action, ItemDto item)
        {
            Action = action;
            Item = item;
        }

        [BlubMember(0)]
        public InventoryAction Action { get; set; }

        [BlubMember(1)]
        public ItemDto Item { get; set; }
    }

    [BlubContract]
    public class RoomCurrentCharacterSlotAckMessage : IGameMessage
    {
        public RoomCurrentCharacterSlotAckMessage()
        {
        }

        public RoomCurrentCharacterSlotAckMessage(uint unk, byte slot)
        {
            Unk = unk;
            Slot = slot;
        }

        [BlubMember(0)]
        public uint Unk { get; set; }

        [BlubMember(1)]
        public byte Slot { get; set; }
    }

    [BlubContract]
    public class RoomEnterPlayerInfoAckMessage : IGameMessage
    {
        public RoomEnterPlayerInfoAckMessage()
        {
            Player = new RoomPlayerDto();
        }

        public RoomEnterPlayerInfoAckMessage(RoomPlayerDto plr)
        {
            Player = plr;
        }

        [BlubMember(0)]
        public RoomPlayerDto Player { get; set; }
    }

    [BlubContract]
    public class RoomEnterClubInfoAckMessage : IGameMessage
    {
        public RoomEnterClubInfoAckMessage()
        {
            Club = new PlayerClubInfoDto();
        }

        public RoomEnterClubInfoAckMessage(PlayerClubInfoDto club)
        {
            Club = club;
        }
        
        [BlubMember(0)]
        public PlayerClubInfoDto Club { get; set; }
    }

    [BlubContract]
    public class RoomPlayerInfoListForEnterPlayerAckMessage : IGameMessage
    {
        public RoomPlayerInfoListForEnterPlayerAckMessage()
        {
            Players = Array.Empty<RoomPlayerDto>();
        }

        public RoomPlayerInfoListForEnterPlayerAckMessage(RoomPlayerDto[] players)
        {
            Players = players;
        }

        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public RoomPlayerDto[] Players { get; set; }
    }

    [BlubContract]
    public class RoomPlayerInfoListForEnterPlayerForCollectBookAckMessage : IGameMessage
    {
        public RoomPlayerInfoListForEnterPlayerForCollectBookAckMessage()
        {
            Count = 0;
            //Players = Array.Empty<RoomPlayerDto>();
        }
        
        //[BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        //public CollectBookInfo[] Players { get; set; }

        [BlubMember(0)]
        public int Count { get; set; }
    }

    [BlubContract]
    public class RoomClubInfoListForEnterPlayerAckMessage : IGameMessage
    {
        public RoomClubInfoListForEnterPlayerAckMessage()
        {
            Clubs = Array.Empty<PlayerClubInfoDto>();
        }

        public RoomClubInfoListForEnterPlayerAckMessage(PlayerClubInfoDto[] infos)
        {
            Clubs = infos;
        }

        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public PlayerClubInfoDto[] Clubs { get; set; }
    }

    [BlubContract]
    public class RoomEnterRoomInfoAck2Message : IGameMessage
    {
        [BlubMember(0)]
        public uint RoomId { get; set; }

        [BlubMember(1)]
        public NeoNetsphere.GameRule GameRule { get; set; }

        [BlubMember(2)]
        public byte MapId { get; set; }

        [BlubMember(3)]
        public byte PlayerLimit { get; set; }

        [BlubMember(4)]
        public GameState GameState { get; set; }

        [BlubMember(5)]
        public GameTimeState GameTimeState { get; set; }

        [BlubMember(6)]
        public uint TimeLimit { get; set; }

        [BlubMember(7)]
        public uint Unk1 { get; set; }

        [BlubMember(8)]
        public uint TimeSync { get; set; }

        [BlubMember(9)]
        public uint ScoreLimit { get; set; }

        [BlubMember(10)]
        public byte Unk2 { get; set; }

        [BlubMember(11, typeof(IPEndPointAddressStringSerializer))]
        public IPEndPoint RelayEndPoint { get; set; } 

        [BlubMember(12)]
        public byte Unk3 { get; set; }

        [BlubMember(13)]
        public uint Unk4 { get; set; }

        [BlubMember(14)]
        public ushort Unk5 { get; set; }

        [BlubMember(15)]
        public byte FMBURNMode { get; set; }

        [BlubMember(16)]
        public ulong Unk6 { get; set; }

        public RoomEnterRoomInfoAck2Message()
        {
            RelayEndPoint = new IPEndPoint(0, 0);
            Unk3 = 108;
            Unk4 = 2544631917;
            Unk5 = 3107;
        }
    }

    [BlubContract]
    public class RoomEnterRoomInfoAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public uint RoomId { get; set; }

        [BlubMember(1)]
        public NeoNetsphere.GameRule GameRule { get; set; }

        [BlubMember(2)]
        public byte MapId { get; set; }

        [BlubMember(3)]
        public byte PlayerLimit { get; set; }

        [BlubMember(4)]
        public GameState GameState { get; set; }

        [BlubMember(5)]
        public GameTimeState GameTimeState { get; set; }

        [BlubMember(6)]
        public uint TimeLimit { get; set; }

        [BlubMember(7)]
        public uint Unk1 { get; set; }

        [BlubMember(8)]
        public uint TimeSync { get; set; }

        [BlubMember(9)]
        public uint ScoreLimit { get; set; }

        [BlubMember(10)]
        public byte Unk2 { get; set; }

        [BlubMember(11, typeof(IPEndPointAddressStringSerializer))]
        public IPEndPoint RelayEndPoint { get; set; }

        [BlubMember(12)]
        public byte Unk3 { get; set; }

        [BlubMember(13)]
        public uint Unk4 { get; set; }

        [BlubMember(14)]
        public ushort Unk5 { get; set; }
        
        public RoomEnterRoomInfoAckMessage()
        {
            RelayEndPoint = new IPEndPoint(0, 0);
            Unk3 = 108;
            Unk4 = 2544631917;
            Unk5 = 3107;
        }
    }

    [BlubContract]
    public class RoomLeavePlayerInfoAckMessage : IGameMessage
    {
        public RoomLeavePlayerInfoAckMessage()
        {
        }

        public RoomLeavePlayerInfoAckMessage(ulong accountId)
        {
            AccountId = accountId;
        }

        [BlubMember(0)]
        public ulong AccountId { get; set; }
    }

    [BlubContract]
    public class TimeSyncAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public uint ClientTime { get; set; }

        [BlubMember(1)]
        public uint ServerTime { get; set; }
    }

    [BlubContract]
    public class RoomChangeRoomInfoAckMessage : IGameMessage
    {
        public RoomChangeRoomInfoAckMessage()
        {
            Room = new RoomDto();
        }

        public RoomChangeRoomInfoAckMessage(RoomDto room)
        {
            Room = room;
        }

        [BlubMember(0)]
        public RoomDto Room { get; set; }
    }

    [BlubContract]
    public class RoomChangeRoomInfoAck2Message : IGameMessage
    {
        public RoomChangeRoomInfoAck2Message()
        {
            Room = new RoomDto();
        }

        public RoomChangeRoomInfoAck2Message(RoomDto room)
        {
            Room = room;
        }

        [BlubMember(0)]
        public RoomDto Room { get; set; }
    }

    [BlubContract]
    public class NewShopUpdateEndAckMessage : IGameMessage
    {
    }

    [BlubContract]
    public class ChannelListInfoAckMessage : IGameMessage
    {
        public ChannelListInfoAckMessage()
        {
            Channels = Array.Empty<ChannelInfoDto>();
        }

        public ChannelListInfoAckMessage(ChannelInfoDto[] channels)
        {
            Channels = channels;
        }

        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public ChannelInfoDto[] Channels { get; set; }
    }

    [BlubContract]
    public class RoomDeployAck2Message : IGameMessage
    {
        public RoomDeployAck2Message()
        {
            Room = new RoomDto();
        }

        public RoomDeployAck2Message(RoomDto room)
        {
            Room = room;
        }

        [BlubMember(0)]
        public RoomDto Room { get; set; }
    }

    [BlubContract]
    public class RoomDeployAckMessage : IGameMessage
    {
        public RoomDeployAckMessage()
        {
            Room = new RoomDto();
        }

        public RoomDeployAckMessage(RoomDto room)
        {
            Room = room;
        }

        [BlubMember(0)]
        public RoomDto Room { get; set; }
    }

    [BlubContract]
    public class RoomDisposeAckMessage : IGameMessage
    {
        public RoomDisposeAckMessage()
        {
        }

        public RoomDisposeAckMessage(uint roomId)
        {
            RoomId = roomId;
        }

        [BlubMember(0)]
        public uint RoomId { get; set; }
    }

    [BlubContract]
    public class PlayerInfoAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public ulong AccountID { get; set; }

        [BlubMember(1, typeof(StringSerializer))]
        public string Nickname { get; set; }

        [BlubMember(2)]
        public int Unk1 { get; set; }

        [BlubMember(3)]
        public int Unk2 { get; set; }

        [BlubMember(4)]
        public byte Unk3 { get; set; }

        [BlubMember(5)]
        public int Unk4 { get; set; }

        [BlubMember(6)]
        public int Unk5 { get; set; }

        [BlubMember(7)]
        public int Unk6 { get; set; }

        [BlubMember(8)]
        public int Unk7 { get; set; }

        [BlubMember(9)]
        public int Unk8 { get; set; }

        [BlubMember(10)]
        public int Unk9 { get; set; }
    }

    [BlubContract]
    public class ItemBuyItemAckMessage : IGameMessage
    {
        public ItemBuyItemAckMessage()
        {
            Ids = Array.Empty<ulong>();
            Item = new ShopItemDto();
        }

        public ItemBuyItemAckMessage(ItemBuyResult result)
            : this()
        {
            Result = result;
        }

        public ItemBuyItemAckMessage(ulong[] ids, ShopItemDto item)
        {
            Ids = ids;
            Result = ItemBuyResult.OK;
            Item = item;
        }

        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public ulong[] Ids { get; set; }

        [BlubMember(1)]
        public ItemBuyResult Result { get; set; }

        [BlubMember(2)]
        public ShopItemDto Item { get; set; }
    }

    [BlubContract]
    public class ItemRepairItemAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public ItemRepairResult Result { get; set; }

        [BlubMember(1)]
        public ulong ItemId { get; set; }
    }

    [BlubContract]
    public class ItemDurabilityItemAckMessage : IGameMessage
    {
        public ItemDurabilityItemAckMessage()
        {
            Items = Array.Empty<ItemDurabilityInfoDto>();
        }

        public ItemDurabilityItemAckMessage(ItemDurabilityInfoDto[] items)
        {
            Items = items;
        }

        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public ItemDurabilityInfoDto[] Items { get; set; }
    }

    [BlubContract]
    public class ItemRefundItemAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public ulong ItemId { get; set; }

        [BlubMember(1)]
        public ItemRefundResult Result { get; set; }
    }

    [BlubContract]
    public class MoneyRefreshCashInfoAckMessage : IGameMessage
    {
        public MoneyRefreshCashInfoAckMessage()
        {
        }

        public MoneyRefreshCashInfoAckMessage(uint pen, uint ap)
        {
            PEN = pen;
            AP = ap;
        }

        [BlubMember(0)]
        public uint PEN { get; set; }

        [BlubMember(1)]
        public uint AP { get; set; }
    }

    [BlubContract]
    public class AdminActionAckMessage : IGameMessage
    {
        public AdminActionAckMessage()
        {
            Message = "";
        }

        [BlubMember(0)]
        public byte Result { get; set; }

        [BlubMember(1, typeof(StringSerializer))]
        public string Message { get; set; }
    }

    [BlubContract]
    public class AdminShowWindowAckMessage : IGameMessage
    {
        public AdminShowWindowAckMessage()
        {
        }

        public AdminShowWindowAckMessage(bool disableConsole)
        {
            DisableConsole = disableConsole;
        }

        [BlubMember(0)]
        public bool DisableConsole { get; set; }
    }

    [BlubContract]
    public class NoticeAdminMessageAckMessage : IGameMessage
    {
        public NoticeAdminMessageAckMessage()
        {
            Message = "";
        }

        public NoticeAdminMessageAckMessage(string message)
        {
            Message = message;
        }

        [BlubMember(0, typeof(StringSerializer))]
        public string Message { get; set; }
    }

    [BlubContract]
    public class CharacterCurrentSlotInfoAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public byte CharacterCount { get; set; }

        [BlubMember(1)]
        public byte MaxSlots { get; set; }

        [BlubMember(2)]
        public byte ActiveCharacter { get; set; }
    }

    [BlubContract]
    public class ItemRefreshInvalidEquipItemAckMessage : IGameMessage
    {
        public ItemRefreshInvalidEquipItemAckMessage()
        {
            Items = Array.Empty<ulong>();
        }

        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public ulong[] Items { get; set; }
    }

    [BlubContract]
    public class ItemClearInvalidEquipItemAckMessage : IGameMessage
    {
        public ItemClearInvalidEquipItemAckMessage()
        {
            Items = Array.Empty<InvalidateItemInfoDto>();
        }

        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public InvalidateItemInfoDto[] Items { get; set; }
    }

    [BlubContract]
    public class CharacterAvatarEquipPresetAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public byte Unk { get; set; }
    }

    [BlubContract]
    public class LicenseMyInfoAckMessage : IGameMessage
    {
        public LicenseMyInfoAckMessage()
        {
            Licenses = Array.Empty<uint>();
        }

        public LicenseMyInfoAckMessage(uint[] licenses)
        {
            Licenses = licenses;
        }

        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public uint[] Licenses { get; set; }
    }

    [BlubContract]
    public class ClubInfoAckMessage : IGameMessage
    {
        public ClubInfoAckMessage()
        {
            ClubInfo = new PlayerClubInfoDto();
        }

        public ClubInfoAckMessage(PlayerClubInfoDto clubInfo)
        {
            ClubInfo = clubInfo;
        }

        [BlubMember(0)]
        public PlayerClubInfoDto ClubInfo { get; set; }
    }

    [BlubContract]
    public class ClubHistoryAckMessage : IGameMessage
    {
        public ClubHistoryAckMessage()
        {
            History = new ClubHistoryDto();
        }

        [BlubMember(0)]
        public ClubHistoryDto History { get; set; }
    }

    [BlubContract]
    public class ItemEquipBoostItemInfoAckMessage : IGameMessage
    {
        public ItemEquipBoostItemInfoAckMessage()
        {
            Items = Array.Empty<ulong>();
        }

        public ItemEquipBoostItemInfoAckMessage(ulong[] items)
        {
            Items = items;
        }
        
        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public ulong[] Items { get; set; }
    }

    [BlubContract]
    public class ClubFindInfoAckMessage : IGameMessage
    {
        public ClubFindInfoAckMessage()
        {
            ClubInfo = new ClubInfoDto();
        }

        [BlubMember(0)]
        public ClubInfoDto ClubInfo { get; set; }
    }

    [BlubContract]
    public class TaskInfoAckMessage : IGameMessage
    {
        public TaskInfoAckMessage()
        {
            Tasks = Array.Empty<TaskDto>();
        }

        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public TaskDto[] Tasks { get; set; }
    }

    [BlubContract]
    public class TaskUpdateAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public uint TaskId { get; set; }

        [BlubMember(1)]
        public ushort Progress { get; set; }
    }

    [BlubContract]
    public class TaskRequestAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public uint TaskId { get; set; }

        [BlubMember(1)]
        public MissionRewardType RewardType { get; set; }

        [BlubMember(2)]
        public uint Reward { get; set; }

        [BlubMember(3)]
        public byte Slot { get; set; }
    }

    [BlubContract]
    public class TaskRemoveAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public uint TaskId { get; set; }
    }

    [BlubContract]
    public class MoenyRefreshCoinInfoAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public uint ArcadeCoins { get; set; }

        [BlubMember(1)]
        public uint BuffCoins { get; set; }
    }

    [BlubContract]
    public class ItemUseEsperChipItemAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public int Unk1 { get; set; }

        [BlubMember(1)]
        public long Unk2 { get; set; }

        [BlubMember(2)]
        public int Unk3 { get; set; }
    }

    [BlubContract]
    public class RequitalArcadeRewardAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public ArcadeRewardDto Reward { get; set; }
    }

    [BlubContract]
    public class PlayeArcadeMapInfoAckMessage : IGameMessage
    {
        public PlayeArcadeMapInfoAckMessage()
        {
            Infos = Array.Empty<ArcadeMapInfoDto>();
        }

        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public ArcadeMapInfoDto[] Infos { get; set; }
    }

    [BlubContract]
    public class PlayerArcadeStageInfoAckMessage : IGameMessage
    {
        public PlayerArcadeStageInfoAckMessage()
        {
            Infos = Array.Empty<ArcadeStageInfoDto>();
        }

        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public ArcadeStageInfoDto[] Infos { get; set; }
    }

    [BlubContract]
    public class MoneyRefreshPenInfoAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public uint Unk { get; set; }
    }

    [BlubContract]
    public class ItemUseCapsuleAckMessage : IGameMessage
    {
        public ItemUseCapsuleAckMessage()
        {
            Rewards = Array.Empty<CapsuleRewardDto>();
        }

        public ItemUseCapsuleAckMessage(byte result)
            : this()
        {
            Result = result;
        }

        public ItemUseCapsuleAckMessage(CapsuleRewardDto[] rewards, byte result)
        {
            Rewards = rewards;
            Result = result;
        }

        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public CapsuleRewardDto[] Rewards { get; set; }

        [BlubMember(1)]
        public byte Result { get; set; }
    }

    [BlubContract]
    public class AdminHGWKickAckMessage : IGameMessage
    {
        public AdminHGWKickAckMessage()
        {
            Message = "";
        }

        [BlubMember(0, typeof(StringSerializer))]
        public string Message { get; set; }
    }

    [BlubContract]
    public class ClubJoinAckMessage : IGameMessage
    {
        public ClubJoinAckMessage()
        {
            Message = "";
        }

        [BlubMember(0)]
        public uint Unk { get; set; }

        [BlubMember(1, typeof(StringSerializer))]
        public string Message { get; set; }
    }

    [BlubContract]
    public class ClubUnJoinAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public uint Result { get; set; }

        public ClubUnJoinAckMessage()
        {
        }

        public ClubUnJoinAckMessage(uint result)
        {
            Result = result;
        }
    }

    [BlubContract]
    public class NewShopUpdateCheckAckMessage : IGameMessage
    {
        public NewShopUpdateCheckAckMessage()
        {
            Date01 = "";
            Date02 = "";
            Date03 = "";
            Date04 = "";
        }

        [BlubMember(0)]
        public uint Unk { get; set; }

        [BlubMember(1, typeof(StringSerializer))]
        public string Date01 { get; set; }

        [BlubMember(2, typeof(StringSerializer))]
        public string Date02 { get; set; }

        [BlubMember(3, typeof(StringSerializer))]
        public string Date03 { get; set; }

        [BlubMember(4, typeof(StringSerializer))]
        public string Date04 { get; set; }
    }

    [BlubContract]
    public class NewShopUpdataInfoAckMessage : IGameMessage
    {
        public NewShopUpdataInfoAckMessage()
        {
            Data = Array.Empty<byte>();
            Date = "";
        }

        [BlubMember(0)]
        public ShopResourceType Type { get; set; }

        [BlubMember(1, typeof(ArrayWithScalarSerializer))]
        public byte[] Data { get; set; }

        [BlubMember(2)]
        public uint DataLength { get; set; } // size of Data?

        [BlubMember(3)]
        public uint Unk2 { get; set; } // checksum?

        [BlubMember(4, typeof(StringSerializer))]
        public string Date { get; set; }
    }

    [BlubContract]
    public class ItemUseChangeNickAckMessage : IGameMessage
    {
        public ItemUseChangeNickAckMessage()
        {
            Unk3 = "";
        }

        [BlubMember(0)]
        public uint Result { get; set; }

        [BlubMember(1)]
        public ulong Unk2 { get; set; }

        [BlubMember(2, typeof(StringSerializer))]
        public string Unk3 { get; set; }
    }

    [BlubContract]
    public class ItemUseRecordResetAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public uint Result { get; set; }

        [BlubMember(1)]
        public ulong Unk2 { get; set; }
    }

    [BlubContract]
    public class ItemUseCoinFillingAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public uint Result { get; set; }
    }

    [BlubContract]
    public class PlayerFindInfoAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public ulong AccountId { get; set; }

        [BlubMember(1)]
        public byte Unk1 { get; set; }

        [BlubMember(2)]
        public int Unk2 { get; set; }

        [BlubMember(3, typeof(StringSerializer))]
        public string Unk3 { get; set; }

        [BlubMember(4)]
        public int Unk4 { get; set; }

        [BlubMember(5)]
        public int Unk5 { get; set; }

        [BlubMember(6)]
        public int Unk6 { get; set; }
    }

    [BlubContract]
    public class ItemDiscardItemAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public uint Result { get; set; }

        [BlubMember(1)]
        public ulong ItemId { get; set; }
    }

    [BlubContract]
    public class ItemInventroyDeleteAckMessage : IGameMessage
    {
        public ItemInventroyDeleteAckMessage()
        {
        }

        public ItemInventroyDeleteAckMessage(ulong itemId)
        {
            ItemId = itemId;
        }

        [BlubMember(0)]
        public ulong ItemId { get; set; }
    }

    [BlubContract]
    public class ClubAddressAckMessage : IGameMessage
    {
        public ClubAddressAckMessage()
        {
            Fingerprint = "";
        }

        public ClubAddressAckMessage(string fingerprint, uint unk2)
        {
            Fingerprint = fingerprint;
            Unk2 = unk2;
        }

        [BlubMember(0, typeof(StringSerializer))]
        public string Fingerprint { get; set; }

        [BlubMember(1)]
        public uint Unk2 { get; set; }
    }

    [BlubContract]
    public class ItemUseChangeNickCancelAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public uint Unk { get; set; }
    }

    [BlubContract]
    public class RequitalEventItemRewardAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public uint Unk1 { get; set; }

        [BlubMember(1)]
        public uint Unk2 { get; set; }

        [BlubMember(2)]
        public uint Unk3 { get; set; }

        [BlubMember(3)]
        public uint Unk4 { get; set; }

        [BlubMember(4)]
        public uint Unk5 { get; set; }

        [BlubMember(5)]
        public uint Unk6 { get; set; }

        [BlubMember(6)]
        public uint Unk7 { get; set; }
    }

    [BlubContract]
    public class RoomListInfoAck2Message : IGameMessage
    {
        public RoomListInfoAck2Message()
        {
            Rooms = Array.Empty<RoomDto>();
        }

        public RoomListInfoAck2Message(RoomDto[] rooms)
        {
            Rooms = rooms;
        }

        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public RoomDto[] Rooms { get; set; }
    }

    [BlubContract]
    public class RoomListInfoAckMessage : IGameMessage
    {
        public RoomListInfoAckMessage()
        {
            Rooms = Array.Empty<RoomDto>();
        }

        public RoomListInfoAckMessage(RoomDto[] rooms)
        {
            Rooms = rooms;
        }

        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public RoomDto[] Rooms { get; set; }
    }

    [BlubContract]
    public class NickDefaultAckMessage : IGameMessage
    {
        [BlubMember(0, typeof(StringSerializer))]
        public string Unk { get; set; }
    }

    [BlubContract]
    public class RequitalGiveItemResultAckMessage : IGameMessage
    {
        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public RequitalGiveItemResultDto[] Unk { get; set; }
    }

    [BlubContract]
    public class ShoppingBasketActionAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public int Unk1 { get; set; }

        [BlubMember(1)]
        public byte Unk2 { get; set; }

        [BlubMember(2)]
        public ShoppingBasketDto Item { get; set; }
    }

    [BlubContract]
    public class ShoppingBasketListInfoAckMessage : IGameMessage
    {
        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public ShoppingBasketDto[] Items { get; set; }

        public ShoppingBasketListInfoAckMessage()
        {
            Items = Array.Empty<ShoppingBasketDto>();
        }

        public ShoppingBasketListInfoAckMessage(ShoppingBasketDto[] items)
        {
            Items = items;
        }
    }

    [BlubContract]
    public class RandomShopUpdateRequestAckMessage : IGameMessage
    {
    }

    [BlubContract]
    public class RandomShopUpdateCheckAckMessage : IGameMessage
    {
        [BlubMember(0, typeof(StringSerializer))]
        public string Unk { get; set; }

        public RandomShopUpdateCheckAckMessage()
        {
            Unk = "";
        }

        public RandomShopUpdateCheckAckMessage(string unk)
        {
            Unk = unk;  
        }
    }

    [BlubContract]
    public class RandomShopUpdateInfoAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public byte Unk1 { get; set; }

        [BlubMember(1, typeof(ArrayWithScalarSerializer))]
        public byte[] Unk2 { get; set; }

        [BlubMember(2)]
        public int Unk3 { get; set; }

        [BlubMember(3)]
        public int Unk4 { get; set; }

        [BlubMember(4, typeof(StringSerializer))]
        public string Unk5 { get; set; }

        public RandomShopUpdateInfoAckMessage()
        {
            Unk5 = "";
        }

        public RandomShopUpdateInfoAckMessage(byte unk1, byte[] unk2, int unk3, int unk4, string unk5)
        {
            Unk1 = unk1;
            Unk2 = unk2;
            Unk3 = unk3;
            Unk4 = unk4;
            Unk5 = unk5;    
        }
    }

    [BlubContract]
    public class RandomShopRollingStartAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public int Unk1 { get; set; }

        [BlubMember(1, typeof(ArrayWithIntPrefixSerializer))]
        public int[] Unk2 { get; set; }
    }

    [BlubContract]
    public class RoomInfoRequestAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public RoomInfoRequestDto Info { get; set; }
    }

    [BlubContract]
    public class RoomInfoRequestAck2Message : IGameMessage
    {
        [BlubMember(0)]
        public RoomInfoRequestDto Info { get; set; }
    }

    [BlubContract]
    public class NoteGiftItemAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public int Unk { get; set; }
    }

    [BlubContract]
    public class NoteImportuneItemAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public int Unk { get; set; }
    }

    [BlubContract]
    public class NoteGiftItemGainAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public int Unk1 { get; set; }

        [BlubMember(1)]
        public ulong Unk2 { get; set; }
    }

    [BlubContract]
    public class RoomQuickJoinAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public byte Unk1 { get; set; }

        [BlubMember(1)]
        public int Unk2 { get; set; }
    }

    [BlubContract]
    public class JorbiWebSessionRedirectAckMessage : IGameMessage
    {
        [BlubMember(0, typeof(StringSerializer))]
        public string Unk1 { get; set; }

        [BlubMember(1, typeof(StringSerializer))]
        public string Unk2 { get; set; }

        [BlubMember(2)]
        public int Unk3 { get; set; }
    }

    [BlubContract]
    public class CardGambleAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public int Unk1 { get; set; }

        [BlubMember(1)]
        public ShopItemDto ShopItem { get; set; }
    }

    [BlubContract]
    public class NoticeItemGainAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public int Unk1 { get; set; }

        [BlubMember(1, typeof(StringSerializer))]
        public string Unk2 { get; set; }

        [BlubMember(2)]
        public ulong Unk3 { get; set; }

        [BlubMember(3)]
        public int Unk4 { get; set; }

        [BlubMember(4)]
        public int Unk5 { get; set; }

        [BlubMember(5)]
        public short Unk6 { get; set; }

        [BlubMember(6)]
        public int Unk7 { get; set; }
    }

    [BlubContract]
    public class PromotionPunkinNoticeAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public int Unk1 { get; set; }

        [BlubMember(1)]
        public byte Unk2 { get; set; }
    }

    [BlubContract]
    public class PromotionPunkinRankersAckMessage : IGameMessage
    {
        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public PromotionPunkinRankerDto[] Unk { get; set; }
    }

    [BlubContract]
    public class RequitalLevelAckMessage : IGameMessage
    {
        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public RequitalLevelDto[] Unk { get; set; }
    }

    [BlubContract]
    public class PromotionAttendanceInfoAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public int Unk1 { get; set; }

        [BlubMember(1, typeof(ArrayWithIntPrefixSerializer))]
        public int[] Unk2 { get; set; }
    }

    [BlubContract]
    public class PromotionAttendanceGiftItemAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public int Unk1 { get; set; }

        [BlubMember(1)]
        public int Unk2 { get; set; }
    }

    [BlubContract]
    public class PromotionCoinEventAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public int Unk1 { get; set; }

        [BlubMember(1)]
        public int Unk2 { get; set; }
    }

    [BlubContract]
    public class PromotionCoinEventDropCoinAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public int Unk { get; set; }
    }

    [BlubContract]
    public class EnchantEnchantItemAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public int Unk1 { get; set; }

        [BlubMember(1)]
        public ulong Unk2 { get; set; }

        [BlubMember(2)]
        public int Unk3 { get; set; }
    }

    [BlubContract]
    public class EnchantRefreshEnchantGaugeAckMessage : IGameMessage
    {
        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public EnchantGaugeDto[] Unk { get; set; }
    }

    [BlubContract]
    public class NoticeEnchantAckMessage : IGameMessage
    {
        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public NoticeEnchantDto[] Unk { get; set; }
    }

    [BlubContract]
    public class PromotionCardShuffleAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public int Unk1 { get; set; }

        [BlubMember(1)]
        public RequitalLevelDto Unk2 { get; set; }
    }

    [BlubContract]
    public class ItemClearEsperChipAckMessage : IGameMessage
    {
        public ItemClearEsperChipAckMessage()
        {
            Unk = Array.Empty<ClearEsperChipDto>();
        }

        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public ClearEsperChipDto[] Unk { get; set; }
    }

    [BlubContract]
    public class ChallengeMyInfoAckMessage : IGameMessage
    {
        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public ChallengeMyInfoDto[] Unk { get; set; }
    }

    [BlubContract]
    public class KRShutDownAckMessage : IGameMessage
    {
    }

    [BlubContract]
    public class RequitalChallengeAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public int Unk1 { get; set; }

        [BlubMember(1, typeof(ArrayWithIntPrefixSerializer))]
        public RequitalLevelDto[] Unk2 { get; set; }
    }

    [BlubContract]
    public class MapOpenInfosMessage : IGameMessage
    {
        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public MapOpenInfoDto[] Unk { get; set; }

        public MapOpenInfosMessage()
        {
            Unk = Array.Empty<MapOpenInfoDto>();
        }

        public MapOpenInfosMessage(MapOpenInfoDto[] unk)
        {
            Unk = unk;
        }
    }

    [BlubContract]
    public class PromotionCouponEventAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public int Unk { get; set; }
    }

    [BlubContract]
    public class TutorialCompletedAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public int Unk { get; set; }
    }

    [BlubContract]
    public class ExpRefreshInfoAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public uint TotalExp { get; set; }

        public ExpRefreshInfoAckMessage()
        {
            
        }

        public ExpRefreshInfoAckMessage(uint totalExp)
        {
            TotalExp = totalExp;
        }
    }

    [BlubContract]
    public class PromotionActiveAckMessage : IGameMessage
    {
        [BlubMember(0, typeof(ArrayWithIntPrefixSerializer))]
        public PromotionActiveDto[] Unk { get; set; }
    }

    [BlubContract]
    public class ClubNoticePointRefreshAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public uint Unk1 { get; set; }

        [BlubMember(1)]
        public uint Unk2 { get; set; }

        [BlubMember(2)]
        public uint Unk3 { get; set; }

        [BlubMember(3)]
        public uint Unk4 { get; set; }

        [BlubMember(4)]
        public uint Unk5 { get; set; }

        [BlubMember(5)]
        public uint Unk6 { get; set; }

        [BlubMember(6)]
        public uint Unk7 { get; set; }

        [BlubMember(7)]
        public uint Unk8 { get; set; }

        [BlubMember(8)]
        public uint Unk9 { get; set; }

        public ClubNoticePointRefreshAckMessage()
        {
            Unk6 = 3;
            Unk8 = 1;
            Unk9 = 2;
        }
    }

    [BlubContract]
    public class ClubNoticeRecordRefreshAckMessage : IGameMessage
    {
        [BlubMember(0)]
        public uint Unk1 { get; set; }

        [BlubMember(1)]
        public uint Unk2 { get; set; }
    }
}
