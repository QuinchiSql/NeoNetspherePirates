using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoNetsphere
{
    public class IpInfo //http://freegeoip.net/json/
    {

        [JsonProperty("ip")]
        public string ip { get; set; }

        [JsonProperty("country_code")]
        public string country_code { get; set; }

        [JsonProperty("country_name")]
        public string country_name { get; set; }

        [JsonProperty("region_code")]
        public string region_code { get; set; }

        [JsonProperty("region_name")]
        public string region_name { get; set; }

        [JsonProperty("city")]
        public string city { get; set; }

        [JsonProperty("zipcode")]
        public string zipcode { get; set; }

        [JsonProperty("latitude")]
        public string latitude { get; set; }

        [JsonProperty("longitude")]
        public string longitude { get; set; }

        [JsonProperty("metro_code")]
        public string metro_code { get; set; }

        [JsonProperty("areacode")]
        public string areacode { get; set; }
    }
    
    public class IpInfo2 //http://ip-api.com/json/
    {

        [JsonProperty("status")]
        public string status { get; set; }

        [JsonProperty("country")]
        public string country { get; set; }
        
        [JsonProperty("countryCode")]
        public string countryCode { get; set; }

        [JsonProperty("region")]
        public string region { get; set; }

        [JsonProperty("regionName")]
        public string regionName { get; set; }

        [JsonProperty("city")]
        public string city { get; set; }

        [JsonProperty("zip")]
        public string zip { get; set; }

        [JsonProperty("lat")]
        public string lat { get; set; }

        [JsonProperty("lon")]
        public string lon { get; set; }

        [JsonProperty("timezone")]
        public string timezone { get; set; }

        [JsonProperty("isp")]
        public string isp { get; set; }

        [JsonProperty("org")]
        public string org { get; set; }

        [JsonProperty("as")]
        public string @as { get; set; }

        [JsonProperty("query")]
        public string query { get; set; }
    }
}
