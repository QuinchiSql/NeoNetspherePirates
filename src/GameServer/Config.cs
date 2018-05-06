﻿using System;
using System.IO;
using System.Net;
using Hjson;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NeoNetsphere
{
    public class Config
    {
        private static readonly string s_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GameServer.hjson");

        static Config()
        {
            if (!File.Exists(s_path))
            {
                Instance = new Config();
                Instance.Save();
                return;
            }

            using (var fs = new FileStream(s_path, FileMode.Open, FileAccess.Read))
            {
                Instance = JsonConvert.DeserializeObject<Config>(HjsonValue.Load(fs).ToString(Stringify.Plain));
            }
        }

        public Config()
        {
            Name = "NeoNetsphere";
            Id = 1;
            Listener = new IPEndPoint(IPAddress.Loopback, 28003);
            ChatListener = new IPEndPoint(IPAddress.Loopback, 28004);
            RelayListener = new IPEndPoint(IPAddress.Loopback, 28005);
            ListenerThreads = 1;
            WorkerThreads = 2;
            IP = "127.0.0.1";
            PlayerLimit = 100;
            SecurityLevel = SecurityLevel.User;
            AuthAPI = new AuthAPI();
            SaveInterval = TimeSpan.FromMinutes(1);
            NoobMode = true;
            Database = new DatabasesConfig();
            Game = new GameSettings();
            BlockedCountries = new string[] { };
            BlockedAddresses = new string[] { };
        }

        public static Config Instance { get; }

        [JsonProperty("server_name")] public string Name { get; set; }

        [JsonProperty("server_id")] public ushort Id { get; set; }

        [JsonProperty("server_ip")] public string IP { get; set; }

        [JsonProperty("listener")]
        [JsonConverter(typeof(IPEndPointConverter))]
        public IPEndPoint Listener { get; set; }

        [JsonProperty("listener_chat")]
        [JsonConverter(typeof(IPEndPointConverter))]
        public IPEndPoint ChatListener { get; set; }

        [JsonProperty("listener_relay")]
        [JsonConverter(typeof(IPEndPointConverter))]
        public IPEndPoint RelayListener { get; set; }

        [JsonProperty("listener_relay_udp_ports")]
        public int[] RelayUdpPorts { get; set; }

        [JsonProperty("listener_threads")] public int ListenerThreads { get; set; }

        [JsonProperty("worker_threads")] public int WorkerThreads { get; set; }

        [JsonProperty("player_limit")] public int PlayerLimit { get; set; }

        [JsonProperty("security_level")] public SecurityLevel SecurityLevel { get; set; }

        [JsonProperty("blocked_countries")] public string[] BlockedCountries { get; set; }

        [JsonProperty("blocked_addresses")] public string[] BlockedAddresses { get; set; }

        [JsonProperty("auth_webapi")] public AuthAPI AuthAPI { get; set; }

        [JsonProperty("save_interval")]
        [JsonConverter(typeof(TimeSpanConverter))]
        public TimeSpan SaveInterval { get; set; }

        [JsonProperty("noob_mode")] public bool NoobMode { get; set; }

        [JsonProperty("database")] public DatabasesConfig Database { get; set; }

        [JsonProperty("game")] public GameSettings Game { get; set; }

        public void Save()
        {
            var json = JsonConvert.SerializeObject(this, Formatting.None);
            File.WriteAllText(s_path, JsonValue.Parse(json).ToString(Stringify.Hjson));
        }
    }

    public class AuthAPI
    {
        public AuthAPI()
        {
            ApiKey = "empty-key";
            EndPoint = new IPEndPoint(IPAddress.Loopback, 27000);
            UpdateInterval = TimeSpan.FromSeconds(25);
        }

        [JsonProperty("api_key")] public string ApiKey { get; set; }

        [JsonProperty("endpoint")]
        [JsonConverter(typeof(IPEndPointConverter))]
        public IPEndPoint EndPoint { get; set; }

        [JsonProperty("serverlist_update_interval")]
        [JsonConverter(typeof(TimeSpanConverter))]
        public TimeSpan UpdateInterval { get; set; }
    }

    public class DatabasesConfig
    {
        public DatabasesConfig()
        {
            Engine = DatabaseEngine.MySQL;
            Auth = new DatabaseConfig {Filename = null, Database = "S4L"};
            Game = new DatabaseConfig {Filename = null, Database = "S4L"};
        }

        [JsonProperty("engine")]
        [JsonConverter(typeof(StringEnumConverter))]
        public DatabaseEngine Engine { get; set; }

        [JsonProperty("auth")] public DatabaseConfig Auth { get; set; }

        [JsonProperty("game")] public DatabaseConfig Game { get; set; }

        public class DatabaseConfig
        {
            public DatabaseConfig()
            {
                Host = "localhost";
                Port = 3306;
            }

            [JsonProperty("filename")] public string Filename { get; set; }

            [JsonProperty("host")] public string Host { get; set; }

            [JsonProperty("port")] public int Port { get; set; }

            [JsonProperty("username")] public string Username { get; set; }

            [JsonProperty("password")] public string Password { get; set; }

            [JsonProperty("database")] public string Database { get; set; }
        }
    }

    public enum DatabaseEngine
    {
        SQLite,
        MySQL,
        PostgreSQL
    }

    public class GameSettings
    {
        public GameSettings()
        {
            EnableTutorial = true;
            MaxLevel = 100;
            StartLevel = 0;
            StartPEN = 10000;
            StartAP = 10000;
            StartCoins1 = 0;
            StartCoins2 = 0;
            NickRestrictions = new NickRestrictions();

            TouchdownExpRates = new ExperienceRates();
            DeathmatchExpRates = new ExperienceRates();
            ChaserExpRates = new ChaserExperienceRates();
            BRExpRates = new ExperienceRates();
            CaptainExpRates = new ChaserExperienceRates();
        }

        [JsonProperty("enable_tutorial")] public bool EnableTutorial { get; set; }

        [JsonProperty("max_level")] public byte MaxLevel { get; set; }

        [JsonProperty("start_level")] public byte StartLevel { get; set; }

        [JsonProperty("start_pen")] public int StartPEN { get; set; }

        [JsonProperty("start_ap")] public int StartAP { get; set; }

        [JsonProperty("start_coins1")] public int StartCoins1 { get; set; }

        [JsonProperty("start_coins2")] public int StartCoins2 { get; set; }

        [JsonProperty("durability_loss_per_death")]
        public int DurabilityLossPerDeath { get; set; }

        [JsonProperty("durability_loss_per_minute")]
        public int DurabilityLossPerMinute { get; set; }

        [JsonProperty("nick_restrictions")] public NickRestrictions NickRestrictions { get; set; }

        [JsonProperty("exp_rates_touchdown")] public ExperienceRates TouchdownExpRates { get; set; }

        [JsonProperty("exp_rates_deathmatch")] public ExperienceRates DeathmatchExpRates { get; set; }

        [JsonProperty("exp_rates_chaser")] public ChaserExperienceRates ChaserExpRates { get; set; }

        [JsonProperty("exp_rates_battleroyal")]
        public ExperienceRates BRExpRates { get; set; }

        [JsonProperty("exp_rates_captain")] public ChaserExperienceRates CaptainExpRates { get; set; }
    }

    public class NickRestrictions
    {
        public NickRestrictions()
        {
            MinLength = 4;
            MaxLength = 30;
            MaxRepeat = 3;
            WhitespaceAllowed = false;
            AsciiOnly = true;
        }

        [JsonProperty("min_length")] public int MinLength { get; set; }

        [JsonProperty("max_length")] public int MaxLength { get; set; }

        [JsonProperty("max_repeat")] public int MaxRepeat { get; set; }

        [JsonProperty("allow_whitespace")] public bool WhitespaceAllowed { get; set; }

        [JsonProperty("only_ascii")] public bool AsciiOnly { get; set; }
    }

    public class ExperienceRates
    {
        [JsonProperty("score_factor")] public float ScoreFactor { get; set; } = 0.7f;

        [JsonProperty("first_place_bonus")] public float FirstPlaceBonus { get; set; } = 50;

        [JsonProperty("second_place_bonus")] public float SecondPlaceBonus { get; set; } = 30;

        [JsonProperty("third_place_bonus")] public float ThirdPlaceBonus { get; set; } = 10;

        [JsonProperty("player_count_factor")] public float PlayerCountFactor { get; set; } = 0.06f;

        [JsonProperty("exp_per_min")] public float ExpPerMin { get; set; } = 20f;
    }

    public class ChaserExperienceRates : ExperienceRates
    {
        [JsonProperty("exp_per_first_point")] public float ExpPerFirstPoint { get; set; } = 5f;

        [JsonProperty("exp_per_second_point")] public float ExpPerSecondPoint { get; set; } = 3f;

        [JsonProperty("exp_per_third_point")] public float ExpPerThirdPoint { get; set; } = 2f;
    }
}
