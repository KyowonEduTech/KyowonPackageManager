using System;
using Unity.Plastic.Newtonsoft.Json;


namespace KyowonPackageManager.Editor
{
    [Serializable]
    public class GitHubPackageInfo
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "package_type")]
        public string PackageType { get; set; }

        [JsonProperty(PropertyName = "owner")]
        public Owner Owner { get; set; }

        [JsonProperty(PropertyName = "version_count")]
        public int VersionCount { get; set; }

        [JsonProperty(PropertyName = "visibility")]
        public string Visibility { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }
    }

    [Serializable]
    public class Owner
    {
        [JsonProperty(PropertyName = "login")]
        public string Login { get; set; }
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }
    }

    [Serializable]
    public class GitHubApiVersion
    {
        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }
    }
}