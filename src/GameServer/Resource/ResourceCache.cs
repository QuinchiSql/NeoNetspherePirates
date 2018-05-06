using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BlubLib.Caching;
using Dapper.FastCrud;
using NeoNetsphere.Database.Auth;
using NeoNetsphere.Database.Game;
using NeoNetsphere.Network;
using Serilog;
using Serilog.Core;

namespace NeoNetsphere.Resource
{
    internal class ResourceCache
    {
        // ReSharper disable once InconsistentNaming
        private static readonly ILogger Logger =
            Log.ForContext(Constants.SourceContextPropertyName, nameof(ResourceCache));

        private readonly ICache _cache = new MemoryCache();
        private readonly ResourceLoader _loader;

        public ResourceCache()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            path = Path.Combine(path, "data");
            _loader = new ResourceLoader(path);
        }

        public void PreCache()
        {
            Logger.Information("Caching: Clubs");
            GetClubs();

            Logger.Information("Caching: Channels");
            GetChannels();

            Logger.Information("Caching: Effects");
            GetEffects();

            Logger.Information("Caching: Items");
            GetItems();

            Logger.Information("Caching: DefaultItems");
            GetDefaultItems();

            Logger.Information("Caching: Shop");
            GetShop();

            Logger.Information("Caching: Experience");
            GetExperience();

            Logger.Information("Caching: Maps");
            GetMaps();

            Logger.Information("Caching: GameTempos");
            GetGameTempos();
        }

        public IReadOnlyList<ChannelDto> GetChannels()
        {
            var value = _cache.Get<IReadOnlyList<ChannelDto>>(ResourceCacheType.Channels);
            if (value == null)
            {
                Logger.Information("Caching...");
                using (var db = GameDatabase.Open())
                {
                    value = db.Find<ChannelDto>().ToList();
                }

                _cache.Set(ResourceCacheType.Channels, value);
            }

            return value;
        }

        public IReadOnlyList<DBClubInfoDto> GetClubs()
        {
            var value = _cache.Get<IReadOnlyList<DBClubInfoDto>>(ResourceCacheType.Clubs);
            if (value == null)
            {
                Logger.Information("Caching...");
                using (var db = GameDatabase.Open())
                {
                    var Clubs = db.Find<ClubDto>().ToList();
                    var ClubPlayers = db.Find<ClubPlayerDto>().ToList();

                    var DBClubInfoList = new List<DBClubInfoDto>();
                    foreach (var clubDto in Clubs)
                    {
                        var ClubInfo = new DBClubInfoDto();
                        ClubInfo.ClubDto = clubDto;

                        var DBPlayerInfoList = new List<ClubPlayerInfo>();
                        foreach (var playerInfoDto in ClubPlayers.Where(p => p.ClubId == clubDto.Id))
                        {
                            AccountDto account;
                            using (var dbC = AuthDatabase.Open())
                            {
                                account = dbC.Find<AccountDto>(statement => statement
                                        .Where($"{nameof(AccountDto.Id):C} = @{nameof(playerInfoDto.PlayerId)}")
                                        .WithParameters(new {playerInfoDto.PlayerId}))
                                    .FirstOrDefault();

                                DBPlayerInfoList.Add(new ClubPlayerInfo
                                {
                                    AccountId = (ulong) playerInfoDto.PlayerId,
                                    State = (ClubState) playerInfoDto.State,
                                    IsMod = playerInfoDto.IsMod,
                                    Account = account
                                });
                            }
                        }

                        ClubInfo.PlayerDto = DBPlayerInfoList.ToArray();
                        DBClubInfoList.Add(ClubInfo);
                    }

                    value = DBClubInfoList.ToArray();
                }

                _cache.Set(ResourceCacheType.Clubs, value);
            }

            return value;
        }

        public IReadOnlyDictionary<uint, ItemEffect> GetEffects()
        {
            var value = _cache.Get<IReadOnlyDictionary<uint, ItemEffect>>(ResourceCacheType.Effects);
            if (value == null)
            {
                Logger.Information("Caching...");
                value = _loader.LoadEffects().ToDictionary(effect => effect.Id);
                _cache.Set(ResourceCacheType.Effects, value);
            }

            return value;
        }

        public IReadOnlyDictionary<ItemNumber, ItemInfo> GetItems()
        {
            var value = _cache.Get<IReadOnlyDictionary<ItemNumber, ItemInfo>>(ResourceCacheType.Items);
            if (value == null)
            {
                Logger.Information("Caching...");
                value = _loader.LoadItems_3().ToDictionary(item => item.ItemNumber);
                _cache.Set(ResourceCacheType.Items, value);
            }

            return value;
        }

        public IReadOnlyList<DefaultItem> GetDefaultItems()
        {
            var value = _cache.Get<IReadOnlyList<DefaultItem>>(ResourceCacheType.DefaultItems);
            if (value == null)
            {
                Logger.Information("Caching...");
                value = _loader.LoadDefaultItems().ToList();
                _cache.Set(ResourceCacheType.DefaultItems, value);
            }

            return value;
        }

        public ShopResources GetShop()
        {
            var value = _cache.Get<ShopResources>(ResourceCacheType.Shop);
            if (value == null)
            {
                Logger.Information("Caching...");
                value = new ShopResources();
                _cache.Set(ResourceCacheType.Shop, value);
            }

            if (string.IsNullOrWhiteSpace(value.Version))
                value.Load();

            return value;
        }

        public IReadOnlyDictionary<int, Experience> GetExperience()
        {
            var value = _cache.Get<IReadOnlyDictionary<int, Experience>>(ResourceCacheType.Exp);
            if (value == null)
            {
                Logger.Information("Caching...");
                value = _loader.LoadExperience().ToDictionary(e => e.Level);
                _cache.Set(ResourceCacheType.Exp, value);
            }

            return value;
        }

        public IReadOnlyDictionary<int, MapInfo> GetMaps()
        {
            var value = _cache.Get<IReadOnlyDictionary<int, MapInfo>>(ResourceCacheType.Maps);
            if (value == null)
            {
                Logger.Information("Caching...");
                value = _loader.LoadMaps().ToDictionary(maps => maps.Id);
                _cache.Set(ResourceCacheType.Maps, value);
            }

            return value;
        }

        public IReadOnlyDictionary<string, GameTempo> GetGameTempos()
        {
            var value = _cache.Get<IReadOnlyDictionary<string, GameTempo>>(ResourceCacheType.GameTempo);
            if (value == null)
            {
                Logger.Information("Caching...");

                value = _loader.LoadGameTempos().ToDictionary(t => t.Name);
                _cache.Set(ResourceCacheType.GameTempo, value);
            }

            return value;
        }

        public void Clear()
        {
            Logger.Information("Clearing cache");
            _cache.Clear();
        }

        public void Clear(ResourceCacheType type)
        {
            Logger.Information($"Clearing cache for {type}");

            if (type == ResourceCacheType.Shop)
            {
                GetShop().Clear();
                return;
            }

            _cache.Remove(type.ToString());
        }
    }

    internal static class ResourceCacheExtensions
    {
        public static T Get<T>(this ICache cache, ResourceCacheType type)
            where T : class
        {
            return cache.Get<T>(type.ToString());
        }

        public static void Set(this ICache cache, ResourceCacheType type, object value)
        {
            cache.Set(type.ToString(), value);
        }

        public static void Set(this ICache cache, ResourceCacheType type, object value, TimeSpan ts)
        {
            cache.Set(type.ToString(), value, ts);
        }
    }
}
