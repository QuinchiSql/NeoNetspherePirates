using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoNetsphere
{
    public class IpInfo
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
}
