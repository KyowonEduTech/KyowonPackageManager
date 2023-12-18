using System;
using Newtonsoft.Json;


namespace KyowonPackageManager.Editor
{
    [Serializable]
    public class KyowonPackage
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }

        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }
    }
}