using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using BlubLib.Configuration;
using NeoNetsphere.Resource.xml;
using Serilog;
using Serilog.Core;

namespace NeoNetsphere.Resource
{
    internal class ResourceLoader
    {
        // ReSharper disable once InconsistentNaming
        private static readonly ILogger Logger =
            Log.ForContext(Constants.SourceContextPropertyName, nameof(ResourceLoader));

        public ResourceLoader(string resourcePath)
        {
            ResourcePath = resourcePath;
        }

        public string ResourcePath { get; }

        public byte[] GetBytes(string fileName)
        {
            var path = Path.Combine(ResourcePath, fileName.Replace('/', Path.DirectorySeparatorChar));
            return File.Exists(path) ? File.ReadAllBytes(path) : null;
        }

        public IEnumerable<Experience> LoadExperience()
        {
            var dto = Deserialize<ExperienceDto>("xml/experience.x7");

            var i = 0;
            return dto.exp.Select(expDto => new Experience
            {
                Level = i++,
                ExperienceToNextLevel = expDto.require,
                TotalExperience = expDto.accumulate
            });
        }

        public IEnumerable<MapInfo> LoadMaps()
        {
            var stringTable = Deserialize<StringTableDto>("language/xml/gameinfo_string_table.xml");
            var dto = Deserialize<MapInfoDto>("xml/map.x7");

            //Logger.Information($"LoadMaps() called ({dto.map.Length})");
            var MapList = new List<int>();
            foreach (var mapDto in dto.map)
                if (MapList.Contains(mapDto.id) || mapDto.id > 255)
                {
                    //Logger.Warning("Map {mapid} couldnt be loaded!", mapDto.id);
                }
                else
                {
                    MapList.Add(mapDto.id);
                    //Logger.Information($"Loading map {mapDto.id}, {mapDto.resource.bginfo_path}");
                    mapDto.resource.bginfo_path = mapDto.resource.bginfo_path.ToLower();
                    var mapPath = mapDto.resource.bginfo_path;
                    var map = new MapInfo
                    {
                        Id = (byte) mapDto.id,
                        MinLevel = 0,
                        ServerId = 0,
                        ChannelId = 0,
                        RespawnType = 0
                    };
                    var data = GetBytes(mapPath);
                    if (data == null)
                        continue;

                    using (var ms = new MemoryStream(data))
                    {
                        map.Config = IniFile.Load(ms);
                    }

                    foreach (var enabledMode in map.Config["MAPINFO"]
                        .Where(pair => pair.Key.StartsWith("enableMode", StringComparison.InvariantCultureIgnoreCase))
                        .Select(pair => pair.Value))
                        switch (enabledMode.Value.ToLower())
                        {
                            case "sl":
                                if (!map.GameRules.Contains(GameRule.Chaser))
                                    map.GameRules.Add(GameRule.Chaser);
                                break;

                            case "t":
                                if (!map.GameRules.Contains(GameRule.Touchdown))
                                    map.GameRules.Add(GameRule.Touchdown);
                                if (!map.GameRules.Contains(GameRule.PassTouchdown))
                                    map.GameRules.Add(GameRule.PassTouchdown);
                                break;

                            case "c":
                                if (!map.GameRules.Contains(GameRule.Captain))
                                    map.GameRules.Add(GameRule.Captain);
                                break;

                            case "f":
                                if (!map.GameRules.Contains(GameRule.Deathmatch))
                                    map.GameRules.Add(GameRule.Deathmatch);
                                if (!map.GameRules.Contains(GameRule.SnowballFight))
                                    map.GameRules.Add(GameRule.SnowballFight);
                                if (!map.GameRules.Contains(GameRule.BattleRoyal))
                                    map.GameRules.Add(GameRule.BattleRoyal);
                                if (!map.GameRules.Contains(GameRule.Captain))
                                    map.GameRules.Add(GameRule.Captain);
                                break;

                            case "d":
                                if (!map.GameRules.Contains(GameRule.SnowballFight))
                                    map.GameRules.Add(GameRule.SnowballFight);
                                if (!map.GameRules.Contains(GameRule.BattleRoyal))
                                    map.GameRules.Add(GameRule.BattleRoyal);
                                if (!map.GameRules.Contains(GameRule.Deathmatch))
                                    map.GameRules.Add(GameRule.Deathmatch);
                                if (!map.GameRules.Contains(GameRule.Chaser))
                                    map.GameRules.Add(GameRule.Chaser);
                                if (!map.GameRules.Contains(GameRule.Captain))
                                    map.GameRules.Add(GameRule.Captain);
                                break;

                            case "s":
                                map.GameRules.Add(GameRule.Survival);
                                break;

                            case "n":
                                map.GameRules.Add(GameRule.Practice);
                                break;

                            case "a":
                                map.GameRules.Add(GameRule.Arcade);
                                break;

                            case "sz":
                                map.GameRules.Add(GameRule.Siege);
                                break;
                            case "std": // wtf is this?
                                break;
                            case "m": // wtf is this?
                                break;

                            default:
                                throw new Exception("Invalid game rule " + enabledMode);
                        }
                    var name_ = new StringTableStringDto();
                    try
                    {
                        name_ = stringTable.@string.First(s =>
                            s.key.Equals(mapDto.Base.map_name_key, StringComparison.InvariantCultureIgnoreCase));
                    }
                    catch (Exception ex)
                    {
                        name_.eng = "unknown";
                    }
                    var name = name_;
                    if (string.IsNullOrWhiteSpace(name.eng))
                        name.eng = mapDto.Base.map_name_key;

                    map.Name = name.eng;
                    yield return map;
                }
        }

        public IEnumerable<ItemEffect> LoadEffects()
        {
            var dto = Deserialize<ItemEffectDto>("xml/item_effect.x7");
            var stringTable = Deserialize<StringTableDto>("language/xml/item_effect_string_table.xml");

            foreach (var itemEffectDto in dto.item.Where(itemEffect => itemEffect.id != 0))
            {
                var itemEffect = new ItemEffect
                {
                    Id = itemEffectDto.id
                };

                foreach (var attributeDto in itemEffectDto.attribute)
                    itemEffect.Attributes.Add(new ItemEffectAttribute
                    {
                        Attribute = (Attribute) Enum.Parse(typeof(Attribute), attributeDto.effect.Replace("_", ""),
                            true),
                        Value = attributeDto.value,
                        Rate = float.Parse(attributeDto.rate, CultureInfo.InvariantCulture)
                    });

                var name = stringTable.@string.First(s =>
                    s.key.Equals(itemEffectDto.text_key, StringComparison.InvariantCultureIgnoreCase));
                if (string.IsNullOrWhiteSpace(name.eng))
                    name.eng = itemEffectDto.NAME;

                itemEffect.Name = name.eng;
                yield return itemEffect;
            }
        }

        public IEnumerable<GameTempo> LoadGameTempos()
        {
            var dto = Deserialize<ConstantInfoDto>("xml/constant_info.x7");

            foreach (var gameTempoDto in dto.GAMEINFOLIST)
            {
                var tempo = new GameTempo
                {
                    Name = gameTempoDto.TEMPVALUE.value
                };

                var values = gameTempoDto.GAMETEPMO_COMMON_TOTAL_VALUE;
                tempo.ActorDefaultHPMax =
                    float.Parse(values.GAMETEMPO_actor_default_hp_max, CultureInfo.InvariantCulture);
                tempo.ActorDefaultMPMax =
                    float.Parse(values.GAMETEMPO_actor_default_mp_max, CultureInfo.InvariantCulture);
                tempo.ActorDefaultMoveSpeed = values.GAMETEMPO_fastrun_required_mp;

                yield return tempo;
            }
        }

        #region DefaultItems

        public IEnumerable<DefaultItem> LoadDefaultItems()
        {
            var dto = Deserialize<DefaultItemDto>("xml/default_item.x7");

            foreach (var itemDto in dto.male.item)
            {
                var item = new DefaultItem
                {
                    ItemNumber = new ItemNumber(itemDto.category, itemDto.sub_category, itemDto.number),
                    Gender = CharacterGender.Male,
                    //Slot = (byte) ParseDefaultItemSlot(itemDto.Value),
                    Variation = itemDto.variation
                };
                yield return item;
            }
            foreach (var itemDto in dto.female.item)
            {
                var item = new DefaultItem
                {
                    ItemNumber = new ItemNumber(itemDto.category, itemDto.sub_category, itemDto.number),
                    Gender = CharacterGender.Female,
                    //Slot = (byte) ParseDefaultItemSlot(itemDto.Value),
                    Variation = itemDto.variation
                };
                yield return item;
            }
        }

        //private static CostumeSlot ParseDefaultItemSlot(string slot)
        //{
        //    Func<string, bool> equals = str => slot.Equals(str, StringComparison.InvariantCultureIgnoreCase);

        //    if (equals("hair"))
        //        return CostumeSlot.Hair;

        //    if (equals("face"))
        //        return CostumeSlot.Face;

        //    if (equals("coat"))
        //        return CostumeSlot.Shirt;

        //    if (equals("pants"))
        //        return CostumeSlot.Pants;

        //    if (equals("gloves"))
        //        return CostumeSlot.Gloves;

        //    if (equals("shoes"))
        //        return CostumeSlot.Shoes;

        //    throw new Exception("Invalid slot " + slot);
        //}

        #endregion

        private T Deserialize<T>(string fileName)
        {
            var serializer = new XmlSerializer(typeof(T));

            var path = Path.Combine(ResourcePath, fileName.Replace('/', Path.DirectorySeparatorChar));
            using (var r = new StreamReader(path))
            {
                return (T) serializer.Deserialize(r);
            }
        }

        #region Items

        public IEnumerable<ItemInfo> LoadItems()
        {
            var dto = Deserialize<ItemInfoDto>("xml/iteminfo.x7");
            var stringTable = Deserialize<StringTableDto>("language/xml/iteminfo_string_table.xml");

            foreach (var categoryDto in dto.category)
            foreach (var subCategoryDto in categoryDto.sub_category)
            foreach (var itemDto in subCategoryDto.item)
            {
                var id = new ItemNumber(categoryDto.id, subCategoryDto.id, itemDto.number);
                ItemInfo item;

                switch (id.Category)
                {
                    case ItemCategory.Skill:
                        item = LoadAction(id, itemDto);
                        break;

                    case ItemCategory.Weapon:
                        item = LoadWeapon(id, itemDto);
                        break;

                    default:
                        item = new ItemInfo();
                        break;
                }

                item.ItemNumber = id;
                item.Level = itemDto.@base.base_info.require_level;
                item.MasterLevel = itemDto.@base.base_info.require_master;
                item.Gender = ParseGender(itemDto.SEX);
                item.Image = itemDto.client.icon.image;

                if (itemDto.@base.license != null)
                    item.License = ParseItemLicense(itemDto.@base.license.require);

                var name = stringTable.@string.FirstOrDefault(s =>
                    s.key.Equals(itemDto.@base.base_info.name_key, StringComparison.InvariantCultureIgnoreCase));
                if (string.IsNullOrWhiteSpace(name?.eng))
                    item.Name = name != null ? name.key : itemDto.NAME;
                else
                    item.Name = name.eng;

                yield return item;
            }
        }

        public IEnumerable<ItemInfo> LoadItems_2()
        {
            var dto = Deserialize<ItemInfoDto_2>("xml/item.x7");
            var stringTable = Deserialize<StringTableDto_2>("language/xml/iteminfo_string_table.x7");
            var ids = new List<ItemNumber>();
            foreach (var itemDto in dto.item)
            {
                var id = new ItemNumber(itemDto.item_key);
                if (!ids.Contains(id))
                {
                    ids.Add(id);
                    var item = new ItemInfo();
                    item.ItemNumber = id;
                    item.Level = 0;
                    item.MasterLevel = 0;
                    item.Gender = ParseGender_2(itemDto.Base.sex);
                    item.Image = itemDto.graphic.icon_image;

                    var name = stringTable.@string.FirstOrDefault(s =>
                        s.key.Equals(itemDto.Base.name_key, StringComparison.InvariantCultureIgnoreCase));
                    if (!string.IsNullOrWhiteSpace(name?.eng) && name?.eng.ToLower() != "no trans" && name?.eng.ToLower() != "not trans")
                        yield return item;
                }
            }
        }
        public IEnumerable<ItemInfo> LoadItems_3()
        {
            var dto = Deserialize<ItemInfoDto_2>("xml/item.x7");
            var dto2 = Deserialize<ItemInfoDto_3>("xml/dumpeditems.xml");
            var stringTable = Deserialize<StringTableDto_2>("language/xml/iteminfo_string_table.x7");
            var ids = new Dictionary<ItemNumber, ItemInfo>();
            foreach (var itemDto in dto.item)
            {
                var id = new ItemNumber(itemDto.item_key);
                if (!ids.Keys.Contains(id))
                {
                    var item = new ItemInfo();
                    item.ItemNumber = id;
                    item.Level = 0;
                    item.MasterLevel = 0;
                    item.Gender = ParseGender_2(itemDto.Base.sex);
                    item.Image = itemDto.graphic.icon_image;
                    ids.Add(id, item);
                }
            }
            foreach (var itemdto in dto2.Item)
            {
                ItemInfo item;
                ids.TryGetValue(new ItemNumber(itemdto.ID), out item);
                if (item != null)
                {
                    item.Colors = (int)itemdto.Color_Count;
                    item.Name = itemdto.Name;
                    
                    if(!string.IsNullOrWhiteSpace(item.Name) && 
                        item.Name != "not trans" && 
                        item.Name != "no trans" && 
                        !string.IsNullOrWhiteSpace(item.Image))
                        yield return item;
                }
            }
        }

        private static ItemLicense ParseItemLicense(string license)
        {
            Func<string, bool> equals = str => license.Equals(str, StringComparison.InvariantCultureIgnoreCase);

            if (equals("license_none"))
                return ItemLicense.None;

            if (equals("LICENSE_CHECK_NONE"))
                return ItemLicense.None;

            if (equals("LICENSE_PLASMA_SWORD"))
                return ItemLicense.PlasmaSword;

            if (equals("license_counter_sword"))
                return ItemLicense.CounterSword;

            if (equals("LICENSE_STORM_BAT"))
                return ItemLicense.StormBat;

            if (equals("LICENSE_ASSASSIN_CLAW"))
                return ItemLicense.None; // ToDo

            if (equals("LICENSE_SUBMACHINE_GUN"))
                return ItemLicense.SubmachineGun;

            if (equals("license_revolver"))
                return ItemLicense.Revolver;

            if (equals("license_semi_rifle"))
                return ItemLicense.SemiRifle;

            if (equals("LICENSE_SMG3"))
                return ItemLicense.None; // ToDo

            if (equals("license_HAND_GUN"))
                return ItemLicense.None; // ToDo

            if (equals("LICENSE_SMG4"))
                return ItemLicense.None; // ToDo

            if (equals("LICENSE_HEAVYMACHINE_GUN"))
                return ItemLicense.HeavymachineGun;

            if (equals("LICENSE_GAUSS_RIFLE"))
                return ItemLicense.GaussRifle;

            if (equals("license_rail_gun"))
                return ItemLicense.RailGun;

            if (equals("license_cannonade"))
                return ItemLicense.Cannonade;

            if (equals("LICENSE_CENTRYGUN"))
                return ItemLicense.Sentrygun;

            if (equals("license_centi_force"))
                return ItemLicense.SentiForce;

            if (equals("LICENSE_SENTINEL"))
                return ItemLicense.SentiNel;

            if (equals("license_mine_gun"))
                return ItemLicense.MineGun;

            if (equals("LICENSE_MIND_ENERGY"))
                return ItemLicense.MindEnergy;

            if (equals("license_mind_shock"))
                return ItemLicense.MindShock;

            // SKILLS

            if (equals("LICENSE_ANCHORING"))
                return ItemLicense.Anchoring;

            if (equals("LICENSE_FLYING"))
                return ItemLicense.Flying;

            if (equals("LICENSE_INVISIBLE"))
                return ItemLicense.Invisible;

            if (equals("license_detect"))
                return ItemLicense.Detect;

            if (equals("LICENSE_SHIELD"))
                return ItemLicense.Shield;

            if (equals("LICENSE_BLOCK"))
                return ItemLicense.Block;

            if (equals("LICENSE_BIND"))
                return ItemLicense.Bind;

            if (equals("LICENSE_METALLIC"))
                return ItemLicense.Metallic;

            throw new Exception("Invalid license " + license);
        }

        private static Gender ParseGender(string gender)
        {
            Func<string, bool> equals = str => gender.Equals(str, StringComparison.InvariantCultureIgnoreCase);

            if (equals("all"))
                return Gender.None;

            if (equals("woman"))
                return Gender.Female;

            if (equals("man"))
                return Gender.Male;
            return Gender.None;
            //throw new Exception("Invalid gender " + gender);
        }

        private static Gender ParseGender_2(string gender)
        {
            if (gender == "man")
                return Gender.Male;

            if (gender == "woman")
                return Gender.Female;

            if (gender == "unisex")
                return Gender.None;

            return Gender.None;
            //throw new Exception("Invalid gender " + gender);
        }

        private static ItemInfo LoadAction(ItemNumber id, ItemInfoItemDto itemDto)
        {
            if (itemDto.action == null)
            {
                Logger.Warning("Missing action for item {id}", id);
                return new ItemInfoAction();
            }

            var item = new ItemInfoAction
            {
                RequiredMP = float.Parse(itemDto.action.ability.required_mp, CultureInfo.InvariantCulture),
                DecrementMP = float.Parse(itemDto.action.ability.decrement_mp, CultureInfo.InvariantCulture),
                DecrementMPDelay = float.Parse(itemDto.action.ability.decrement_mp_delay, CultureInfo.InvariantCulture)
            };

            if (itemDto.action.@float != null)
                item.ValuesF = itemDto.action.@float
                    .Select(f => float.Parse(f.value.Replace("f", ""), CultureInfo.InvariantCulture)).ToList();

            if (itemDto.action.integer != null)
                item.Values = itemDto.action.integer.Select(i => i.value).ToList();

            return item;
        }

        private static ItemInfo LoadWeapon(ItemNumber id, ItemInfoItemDto itemDto)
        {
            if (itemDto.weapon == null)
                return new ItemInfoWeapon();

            var ability = itemDto.weapon.ability;
            var item = new ItemInfoWeapon
            {
                Type = ability.type,
                RateOfFire = float.Parse(ability.rate_of_fire, CultureInfo.InvariantCulture),
                Power = float.Parse(ability.power, CultureInfo.InvariantCulture),
                MoveSpeedRate = float.Parse(ability.move_speed_rate, CultureInfo.InvariantCulture),
                AttackMoveSpeedRate = float.Parse(ability.attack_move_speed_rate, CultureInfo.InvariantCulture),
                MagazineCapacity = ability.magazine_capacity,
                CrackedMagazineCapacity = ability.cracked_magazine_capacity,
                MaxAmmo = ability.max_ammo,
                Accuracy = float.Parse(ability.accuracy, CultureInfo.InvariantCulture),
                Range = string.IsNullOrWhiteSpace(ability.range)
                    ? 0
                    : float.Parse(ability.range, CultureInfo.InvariantCulture),
                SupportSniperMode = ability.support_sniper_mode > 0,
                SniperModeFov = ability.sniper_mode_fov > 0,
                AutoTargetDistance = ability.auto_target_distance == null
                    ? 0
                    : float.Parse(ability.auto_target_distance, CultureInfo.InvariantCulture)
            };

            if (itemDto.weapon.@float != null)
                item.ValuesF = itemDto.weapon.@float
                    .Select(f => float.Parse(f.value.Replace("f", ""), CultureInfo.InvariantCulture)).ToList();

            if (itemDto.weapon.integer != null)
                item.Values = itemDto.weapon.integer.Select(i => i.value).ToList();

            return item;
        }

        #endregion
    }
}
