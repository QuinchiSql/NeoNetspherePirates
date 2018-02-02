using System;
using System.IO;
using System.Net;
using Hjson;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog;
using Serilog.Core;

namespace NeoNetsphere
{
    public class Config
    {
        private static readonly string s_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AuthServer.hjson");

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

            if (Instance.API.ApiKey == "empty-key")
            {
                var rnd = new SecureRandom();
                Instance.API.ApiKey = $"key-{rnd.Next()}";
                Instance.Save();
                Log.ForContext(Constants.SourceContextPropertyName, nameof(Config))
                    .Information("Created random api_key: {key}", Instance.API.ApiKey);
            }
        }

        public Config()
        {
            Listener = new IPEndPoint(IPAddress.Loopback, 28002);
            MaxConnections = 100;
            API = new APIConfig();
            AuthAPI = new AuthAPIConfig();
            NoobMode = true;
            AutoRegister = false;
            Database = new DatabasesConfig();
        }

        public static Config Instance { get; }

        [JsonProperty("listener")]
        [JsonConverter(typeof(IPEndPointConverter))]
        public IPEndPoint Listener { get; set; }

        [JsonProperty("max_connections")]
        public int MaxConnections { get; set; }

        [JsonProperty("auth_api")]
        public AuthAPIConfig AuthAPI { get; set; }

        [JsonProperty("api")]
        public APIConfig API { get; set; }

        [JsonProperty("noob_mode")]
        public bool NoobMode { get; set; }

        [JsonProperty("auto_register")]
        public bool AutoRegister { get; set; }

        [JsonProperty("database")]
        public DatabasesConfig Database { get; set; }

        public void Save()
        {
            var json = JsonConvert.SerializeObject(this, Formatting.None);
            File.WriteAllText(s_path, JsonValue.Parse(json).ToString(Stringify.Hjson));
        }
    }

    public class AuthAPIConfig
    {
        public AuthAPIConfig()
        {
            Listener = new IPEndPoint(IPAddress.Loopback, 28001);
        }

        [JsonProperty("listener")]
        [JsonConverter(typeof(IPEndPointConverter))]
        public IPEndPoint Listener { get; set; }
    }

    public class APIConfig
    {
        public APIConfig()
        {
            ApiKey = "empty-key";
            Listener = new IPEndPoint(IPAddress.Loopback, 27000);
            Timeout = TimeSpan.FromSeconds(30);
        }

        [JsonProperty("api_key")]
        public string ApiKey { get; set; }

        [JsonProperty("listener")]
        [JsonConverter(typeof(IPEndPointConverter))]
        public IPEndPoint Listener { get; set; }

        [JsonProperty("serverlist_timeout")]
        [JsonConverter(typeof(TimeSpanConverter))]
        public TimeSpan Timeout { get; set; }
    }

    public class DatabasesConfig
    {
        public DatabasesConfig()
        {
            Engine = DatabaseEngine.MySQL;
            Auth = new DatabaseConfig {Filename = ""};
        }

        [JsonProperty("engine")]
        [JsonConverter(typeof(StringEnumConverter))]
        public DatabaseEngine Engine { get; set; }

        [JsonProperty("auth")]
        public DatabaseConfig Auth { get; set; }

        public class DatabaseConfig
        {
            public DatabaseConfig()
            {
                Host = "localhost";
                Port = 3306;
            }

            [JsonProperty("filename")]
            public string Filename { get; set; }

            [JsonProperty("host")]
            public string Host { get; set; }

            [JsonProperty("port")]
            public int Port { get; set; }

            [JsonProperty("username")]
            public string Username { get; set; }

            [JsonProperty("password")]
            public string Password { get; set; }

            [JsonProperty("database")]
            public string Database { get; set; }
        }
    }

    public enum DatabaseEngine
    {
        SQLite,
        MySQL,
        PostgreSQL
    }
}
