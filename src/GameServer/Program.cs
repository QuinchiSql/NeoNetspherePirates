using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;
using BlubLib;
using Dapper;
using Dapper.FastCrud;
using DotNetty.Transport.Channels;
using Microsoft.Data.Sqlite;
using MySql.Data.MySqlClient;
using NeoNetsphere.Database.Game;
using NeoNetsphere.Network;
using NeoNetsphere.Network.Message.Game;
using Newtonsoft.Json;
using ProudNetSrc;
using ProudNetSrc.Serialization.Messages;
using Serilog;
using Serilog.Core;
using Serilog.Formatting.Json;

namespace NeoNetsphere
{
    internal class Program
    {
        private static readonly object s_exitMutex = new object();
        private static bool s_hasExited;
        public static Version GlobalVersion;

        public static Stopwatch AppTime { get; } = Stopwatch.StartNew();

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
        private static void Main()
        {
            GlobalVersion = Assembly.GetExecutingAssembly().GetName().Version;
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> {new IPEndPointConverter()}
            };

            var jsonlog = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GameServer.json");
            var logfile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GameServer.log");
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(new JsonFormatter(), jsonlog)
                .WriteTo.File(logfile)
                .WriteTo.Console(outputTemplate: "[{Level} {SourceContext}] {Message}{NewLine}{Exception}")
                .MinimumLevel.Verbose()
                .CreateLogger();
            var Logger = Log.ForContext(Constants.SourceContextPropertyName, "-Initialiazor");
#if !DEBUG
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
#endif
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
            Logger.Information("============================================");
            Logger.Information("Initializing GameServer build {ver}...",
                $"{GlobalVersion.Major}.{GlobalVersion.Major / 2 + GlobalVersion.Minor + GlobalVersion.Build + GlobalVersion.Revision}");
#if NEWIDS
            Logger.Information("Set mode NEWIDS");
#endif
#if LATESTS4
            Logger.Information("Set mode LATESTS4");
#endif
            Logger.Information("============================================");
            Logger.Information("Initializing Database...");

            AuthDatabase.Initialize();
            GameDatabase.Initialize();

            Logger.Information("============================================");
            Logger.Information("Starting Serverinstances and ResourceCache...");

            ItemIdGenerator.Initialize();
            CharacterIdGenerator.Initialize();
            DenyIdGenerator.Initialize();

            var listenerThreads = new MultithreadEventLoopGroup(Config.Instance.ListenerThreads);
            var workerThreads = new MultithreadEventLoopGroup(Config.Instance.WorkerThreads);
            var workerThread = new SingleThreadEventLoop();
            ChatServer.Initialize(new Configuration
            {
                SocketListenerThreads = listenerThreads,
                SocketWorkerThreads = workerThreads,
                WorkerThread = workerThread,
#if DEBUG
                Logger = Logger,
#endif
            });
            RelayServer.Initialize(new Configuration
            {
                SocketListenerThreads = listenerThreads,
                SocketWorkerThreads = workerThreads,
                WorkerThread = workerThread,
#if DEBUG
Logger = Logger,
#endif
            });
            GameServer.Initialize(new Configuration
            {
                SocketListenerThreads = listenerThreads,
                SocketWorkerThreads = workerThreads,
                WorkerThread = workerThread,
#if DEBUG
                Logger = Logger,
#endif
            });

            var filler = new Thread(() =>
            {
                FillShop();
            });
            // FillShop();
            filler.Start();

            ChatServer.Instance.Listen(Config.Instance.ChatListener);
            RelayServer.Instance.Listen(Config.Instance.RelayListener, IPAddress.Parse(Config.Instance.IP),
                Config.Instance.RelayUdpPorts);
            GameServer.Instance.Listen(Config.Instance.Listener);

            Logger.Information("Serverinstances successfully started, ready for connections!");
            Logger.Information("============================================\n");

            Console.CancelKeyPress += OnCancelKeyPress;
            while (true)
            {
                var input = Console.ReadLine();
                if (input == null)
                    break;

                if (input.Equals("exit", StringComparison.InvariantCultureIgnoreCase) ||
                    input.Equals("quit", StringComparison.InvariantCultureIgnoreCase) ||
                    input.Equals("stop", StringComparison.InvariantCultureIgnoreCase))
                    break;

                var args = input.GetArgs();
                if (args.Length == 0)
                    continue;

                Task.Run(() =>
                {
                    if (!GameServer.Instance.CommandManager.Execute(null, args))
                        Console.WriteLine("Unknown command");
                });
            }

            Exit();
        }

        private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Exit();
        }

        private static void Exit()
        {
            lock (s_exitMutex)
            {
                if (s_hasExited)
                    return;

                Log.Information("Closing...");

                try
                {
                    foreach (var sess in GameServer.Instance.Sessions.Values)
                    {
                        var session = (GameSession) sess;
                        session.Player?.Room?.Leave(session.Player);
                    }

                    GameServer.Instance.Broadcast(new ItemUseChangeNickAckMessage {Result = 0});
                    GameServer.Instance.Broadcast(new ServerResultAckMessage(ServerResult.CreateNicknameSuccess));
                    GameServer.Instance.Broadcast(new RequestAutoPruneAckMessage(), SendOptions.Reliable);
                }
                catch (Exception)
                {
                    //ignored
                }
                
                s_hasExited = true;
                Environment.Exit(0);
            }
        }

        private static void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            Log.Error(e.Exception, "UnobservedTaskException");
        }

        private static void OnUnhandledException(object s, UnhandledExceptionEventArgs args)
        {
            var e = (Exception) args.ExceptionObject;
            Log.Error(e.ToString(), "UnhandledException");
            try
            {
                foreach (var sess in GameServer.Instance.Sessions.Values)
                {
                    var session = (GameSession) sess;
                    session.Player?.Room?.Leave(session.Player);
                }

                GameServer.Instance.Broadcast(new ItemUseChangeNickAckMessage {Result = 0});
                GameServer.Instance.Broadcast(new ServerResultAckMessage(ServerResult.CreateNicknameSuccess));
                GameServer.Instance.Broadcast(new RequestAutoPruneAckMessage(), SendOptions.Reliable);
            }
            catch (Exception)
            {
                //ignored
            }

            Environment.Exit(-1);
        }

        private static void FillShop()
        {
            if (!Config.Instance.NoobMode)
                return;

            using (var db = GameDatabase.Open())
            {
                if (!db.Find<ShopVersionDto>().Any())
                {
                    var version = new ShopVersionDto
                    {
                        Version = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss")
                    };
                    db.Insert(version);
                }

                if (db.Find<ShopEffectGroupDto>().Any() || db.Find<ShopEffectDto>().Any() ||
                    db.Find<ShopPriceGroupDto>().Any() || db.Find<ShopPriceDto>().Any() ||
                    db.Find<ShopItemDto>().Any() || db.Find<ShopItemInfoDto>().Any())
                    return;

                Log.Information("NoobMode: Filling the shop with items");

                using (var transaction = db.BeginTransaction())
                {
                    var effects = new Dictionary<string, Tuple<uint[], uint>> // effectids, groupid/effect
                    {
                        {"None", Tuple.Create(Array.Empty<uint>(), (uint) 0)},
                        {
                            "Shooting Weapon Defense (Head) +5%",
                            Tuple.Create(new uint[] {1100313003, 1100315003, 1100317003}, (uint) 1100315002)
                        },
                        {"SP+6", Tuple.Create(new uint[] {1101301006}, (uint) 1101301006)},
                        {"Attack+1%", Tuple.Create(new uint[] {1102303001}, (uint) 1102303001)},
                        {"Attack+5%", Tuple.Create(new uint[] {1102303003}, (uint) 1102303003)},
                        {"Attack+10%", Tuple.Create(new uint[] {1299600006}, (uint) 1299600006)},
                        {"Defense+5%", Tuple.Create(new uint[] {1103302004}, (uint) 1103302004)},
                        {"HP+4", Tuple.Create(new uint[] {1105300004}, (uint) 1105300004)},
                        {"HP+30", Tuple.Create(new uint[] {1999300011}, (uint) 1999300011)},
                        {"HP+15", Tuple.Create(new uint[] {1999300009}, (uint) 1999300009)},
                        {"SP+40", Tuple.Create(new uint[] {1300301012}, (uint) 1300301012)},
                        {"HP+20 & SP+20", Tuple.Create(new uint[] {1999300010, 1999301011}, (uint) 30001)},
                        {"HP+25 & SP+25", Tuple.Create(new uint[] {1999300012, 1999301013}, (uint) 30003)}
                    };

#region Effects

                    foreach (var pair in effects.ToArray())
                    {
                        var effectGroup = new ShopEffectGroupDto {Name = pair.Key, Effect = pair.Value.Item2};
                        db.Insert(effectGroup, statement => statement.AttachToTransaction(transaction));
                        effects[pair.Key] = Tuple.Create(pair.Value.Item1, (uint) effectGroup.Id);

                        foreach (var effect in pair.Value.Item1)
                            db.Insert(new ShopEffectDto {EffectGroupId = effectGroup.Id, Effect = effect},
                                statement => statement.AttachToTransaction(transaction));
                    }

#endregion

#region Price

                    var priceGroup = new ShopPriceGroupDto
                    {
                        Name = "PEN",
                        PriceType = (byte) ItemPriceType.PEN
                    };
                    var priceGroup2 = new ShopPriceGroupDto
                    {
                        Name = "Item",
                        PriceType = (byte) ItemPriceType.Premium
                    };

                    db.Insert(priceGroup);
                    db.Insert(priceGroup2);
                    var price_none_perm = new ShopPriceDto
                    {
                        PriceGroupId = priceGroup.Id,
                        PeriodType = (byte) ItemPeriodType.None,
                        IsRefundable = true,
                        Durability = 2400,
                        IsEnabled = true,
                        Price = 1
                    };

                    var price_unit_one = new ShopPriceDto
                    {
                        PriceGroupId = priceGroup2.Id,
                        Durability = 1,
                        PeriodType = (byte) ItemPeriodType.Units,
                        IsRefundable = true,
                        Period = 1,
                        IsEnabled = true,
                        Price = 1000
                    };

                    var price_unit_two = new ShopPriceDto
                    {
                        PriceGroupId = priceGroup2.Id,
                        Durability = 1,
                        PeriodType = (byte) ItemPeriodType.Units,
                        IsRefundable = true,
                        Period = 2,
                        IsEnabled = true,
                        Price = 2000
                    };

                    var price_unit_five = new ShopPriceDto
                    {
                        PriceGroupId = priceGroup2.Id,
                        Durability = 1,
                        PeriodType = (byte) ItemPeriodType.Units,
                        IsRefundable = true,
                        Period = 5,
                        IsEnabled = true,
                        Price = 4500
                    };

                    var price_unit_ten = new ShopPriceDto
                    {
                        PriceGroupId = priceGroup2.Id,
                        Durability = 1,
                        PeriodType = (byte) ItemPeriodType.Units,
                        IsRefundable = true,
                        Period = 10,
                        IsEnabled = true,
                        Price = 8000
                    };

                    db.Insert(price_none_perm);
                    db.Insert(price_unit_one);
                    db.Insert(price_unit_two);
                    db.Insert(price_unit_five);
                    db.Insert(price_unit_ten);

#endregion

#region Items

                    Resource.ItemInfo[] items;
                    do
                    {
                        Thread.Sleep(5000);
                        items = GameServer.Instance.ResourceCache.GetItems().Values.ToArray();
                        Log.Information("{count} items cargados", items.Count());
                    }
                    while (!items.Any());

                    for (var i = 0; i < items.Length; ++i)
                    {
                        var item = items[i];
                        var effectToUse = effects["None"];
                        byte mainTab = 0;
                        byte subTab = 0;

                        switch (item.ItemNumber.Category)
                        {
                            case ItemCategory.Card:
                            case ItemCategory.Coupon:
                                mainTab = 4;
                                subTab = 6;
                                break;
                            case ItemCategory.EsperChip:
                                mainTab = 4;
                                subTab = 2;
                                break;
                            case ItemCategory.Boost:
                                switch ((BoostCategory) item.ItemNumber.SubCategory)
                                {
                                    case BoostCategory.Pen:
                                        mainTab = 4;
                                        subTab = 4;
                                        break;
                                    case BoostCategory.Exp:
                                        mainTab = 4;
                                        subTab = 5;
                                        break;
                                    case BoostCategory.Mp:
                                        mainTab = 4;
                                        subTab = 3;
                                        break;
                                    case BoostCategory.Unique:
                                        mainTab = 4;
                                        subTab = 5;
                                        break;
                                }

                                break;
                            case ItemCategory.OneTimeUse:
                                switch ((OneTimeUseCategory) item.ItemNumber.SubCategory)
                                {
                                    case OneTimeUseCategory.Namechange:
                                        mainTab = 4;
                                        subTab = 0;
                                        break;
                                    case OneTimeUseCategory.Stat:
                                        mainTab = 4;
                                        subTab = 0;
                                        break;
                                    case OneTimeUseCategory.Capsule: //clothes+weps
                                        mainTab = 1;
                                        subTab = 0;
                                        break;
                                    case OneTimeUseCategory.FumbiCapsule:
                                        mainTab = 1;
                                        subTab = 4;
                                        break;
                                    case OneTimeUseCategory.Event:
                                        mainTab = 4;
                                        subTab = 0;
                                        break;
                                    default:
                                        continue;
                                }

                                break;
                            case ItemCategory.Weapon:
                                effectToUse = effects["Attack+1%"];
                                mainTab = 2;

                                switch ((WeaponCategory) item.ItemNumber.SubCategory)
                                {
                                    case WeaponCategory.Melee:
                                        subTab = 1;
                                        break;

                                    case WeaponCategory.RifleGun:
                                        subTab = 2;
                                        break;

                                    case WeaponCategory.HeavyGun:
                                        subTab = 4;
                                        break;

                                    case WeaponCategory.Sniper:
                                        subTab = 5;
                                        break;

                                    case WeaponCategory.Sentry:
                                        subTab = 6;
                                        break;

                                    case WeaponCategory.Bomb:
                                        subTab = 7;
                                        break;

                                    case WeaponCategory.Mind:
                                        subTab = 6;
                                        break;
                                }

                                break;

                            case ItemCategory.Skill:
                                mainTab = 2;
                                subTab = 8;
                                if (item.ItemNumber.SubCategory == 0 && item.ItemNumber.Number == 0) // half hp mastery
                                    effectToUse = effects["HP+15"];

                                if (item.ItemNumber.SubCategory == 0 && item.ItemNumber.Number == 1) // hp mastery
                                    effectToUse = effects["HP+30"];

                                if (item.ItemNumber.SubCategory == 0 && item.ItemNumber.Number == 2) // sp mastery
                                    effectToUse = effects["SP+40"];

                                if (item.ItemNumber.SubCategory == 0 && item.ItemNumber.Number == 3) // dual mastery
                                    effectToUse = effects["HP+20 & SP+20"];

                                if (item.ItemNumber.SubCategory == 0 && item.ItemNumber.Number == 5
                                ) // dual mastery - returner
                                    effectToUse = effects["HP+20 & SP+20"];

                                if (item.ItemNumber.SubCategory == 0 && item.ItemNumber.Number == 7
                                ) // unique dual mastery - balanced!
                                    effectToUse = effects["HP+25 & SP+25"];

                                break;

                            case ItemCategory.Costume:
                                mainTab = 3;
                                subTab = (byte) (item.ItemNumber.SubCategory + 2);
                                switch ((CostumeSlot) item.ItemNumber.SubCategory)
                                {
                                    case CostumeSlot.Hair:
                                        effectToUse = effects["Shooting Weapon Defense (Head) +5%"];
                                        break;

                                    case CostumeSlot.Face:
                                        effectToUse = effects["SP+6"];
                                        break;

                                    case CostumeSlot.Shirt:
                                        effectToUse = effects["Attack+5%"];
                                        break;

                                    case CostumeSlot.Pants:
                                        effectToUse = effects["Defense+5%"];
                                        break;

                                    case CostumeSlot.Gloves:
                                        effectToUse = effects["HP+4"];
                                        break;

                                    case CostumeSlot.Shoes:
                                        effectToUse = effects["HP+4"];
                                        break;

                                    case CostumeSlot.Accessory:
                                        effectToUse = effects["SP+6"];
                                        break;

                                    case CostumeSlot.Pet:
                                        effectToUse = effects["SP+6"];
                                        break;
                                }

                                break;

                            default:
                                effectToUse = effects["None"];
                                mainTab = 4;
                                subTab = 6;
                                break;
                        }

                        var shopItem = new ShopItemDto
                        {
                            Id = item.ItemNumber,
                            RequiredGender = (byte) item.Gender,
                            RequiredLicense = (byte) item.License,
                            IsDestroyable = true,
                            MainTab = mainTab,
                            SubTab = subTab,
                            Colors = (byte) item.Colors
                        };
                        db.Insert(shopItem, statement => statement.AttachToTransaction(transaction));

                        var shopItemInfo = new ShopItemInfoDto
                        {
                            ShopItemId = shopItem.Id,
                            PriceGroupId = priceGroup.Id,
                            EffectGroupId = (int) effectToUse.Item2,
                            IsEnabled = true
                        };
                        var shopItemInfo_onetimeuse = new ShopItemInfoDto
                        {
                            ShopItemId = shopItem.Id,
                            PriceGroupId = priceGroup2.Id,
                            EffectGroupId = (int) effectToUse.Item2,
                            IsEnabled = true
                        };
                        if (item.ItemNumber.Category == ItemCategory.Costume || item.ItemNumber.Category ==
                                                                             ItemCategory.Weapon
                                                                             || item.ItemNumber.Category ==
                                                                             ItemCategory.Skill ||
                                                                             item.ItemNumber.Category ==
                                                                             ItemCategory.EsperChip)
                            db.Insert(shopItemInfo, statement => statement.AttachToTransaction(transaction));
                        else
                            db.Insert(shopItemInfo_onetimeuse, statement => statement.AttachToTransaction(transaction));

                        Log.Information($"[{i}/{items.Length}] {item.ItemNumber}: {item.Name} | Colors: {item.Colors}");
                    }

#endregion

                    try
                    {
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }

    internal static class AuthDatabase
    {
        // ReSharper disable once InconsistentNaming
        private static readonly ILogger Logger =
            Log.ForContext(Constants.SourceContextPropertyName, $"-{nameof(AuthDatabase)}");

        private static string _sConnectionString;

        public static void Initialize()
        {
            Logger.Information("Initializing...");
            var config = Config.Instance.Database;

            switch (config.Engine)
            {
                case DatabaseEngine.MySQL:
                    _sConnectionString =
                        $"SslMode=none;Server={config.Auth.Host};Port={config.Auth.Port};Database={config.Auth.Database};Uid={config.Auth.Username};Pwd={config.Auth.Password};Pooling=true;";
                    OrmConfiguration.DefaultDialect = SqlDialect.MySql;

                    using (var con = Open())
                    {
                        if (con.QueryFirstOrDefault($"SHOW DATABASES LIKE \"{config.Auth.Database}\"") == null)
                        {
                            Logger.Error($"Database '{config.Auth.Database}' not found");
                            Environment.Exit(0);
                        }
                    }

                    break;

                case DatabaseEngine.SQLite:
                    _sConnectionString = $"Data Source={config.Auth.Filename};";
                    OrmConfiguration.DefaultDialect = SqlDialect.SqLite;

                    if (!File.Exists(config.Auth.Filename))
                    {
                        Logger.Error($"Database '{config.Auth.Filename}' not found");
                        Environment.Exit(0);
                    }

                    break;

                default:
                    Logger.Error($"Invalid database engine {config.Engine}");
                    Environment.Exit(0);
                    return;
            }
        }

        public static IDbConnection Open()
        {
            var engine = Config.Instance.Database.Engine;
            IDbConnection connection;
            switch (engine)
            {
                case DatabaseEngine.MySQL:
                    connection = new MySqlConnection(_sConnectionString);
                    break;

                case DatabaseEngine.SQLite:
                    connection = new SqliteConnection(_sConnectionString);
                    break;

                default:
                    Logger.Error($"Invalid database engine {engine}");
                    Environment.Exit(0);
                    return null;
            }

            connection.Open();
            return connection;
        }
    }

    internal static class GameDatabase
    {
        // ReSharper disable once InconsistentNaming
        private static readonly ILogger Logger =
            Log.ForContext(Constants.SourceContextPropertyName, $"-{nameof(GameDatabase)}");

        private static string _sConnectionString;

        public static void Initialize()
        {
            Logger.Information("Initializing...");
            var config = Config.Instance.Database;

            switch (config.Engine)
            {
                case DatabaseEngine.MySQL:
                    _sConnectionString =
                        $"SslMode=none;Server={config.Game.Host};Port={config.Game.Port};Database={config.Game.Database};Uid={config.Game.Username};Pwd={config.Game.Password};Pooling=true;";
                    OrmConfiguration.DefaultDialect = SqlDialect.MySql;

                    using (var con = Open())
                    {
                        if (con.QueryFirstOrDefault($"SHOW DATABASES LIKE \"{config.Game.Database}\"") == null)
                        {
                            Logger.Error($"Database '{config.Game.Database}' not found");
                            Environment.Exit(0);
                        }
                    }

                    break;

                case DatabaseEngine.SQLite:
                    _sConnectionString = $"Data Source={config.Game.Filename};";
                    OrmConfiguration.DefaultDialect = SqlDialect.SqLite;

                    if (!File.Exists(config.Game.Filename))
                    {
                        Logger.Error($"Database '{config.Game.Filename}' not found");
                        Environment.Exit(0);
                    }

                    break;

                default:
                    Logger.Error($"Invalid database engine {config.Engine}");
                    Environment.Exit(0);
                    return;
            }
        }

        public static IDbConnection Open()
        {
            var engine = Config.Instance.Database.Engine;
            IDbConnection connection;
            switch (engine)
            {
                case DatabaseEngine.MySQL:
                    connection = new MySqlConnection(_sConnectionString);
                    break;

                case DatabaseEngine.SQLite:
                    connection = new SqliteConnection(_sConnectionString);
                    break;

                default:
                    Log.Error($"Invalid database engine {engine}");
                    Environment.Exit(0);
                    return null;
            }

            connection.Open();
            return connection;
        }
    }
}
