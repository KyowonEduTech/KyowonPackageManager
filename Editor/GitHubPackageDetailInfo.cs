using System;
using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json;


namespace KyowonPackageManager.Editor
{
    [Serializable]
    public class Dist
    {
        [JsonProperty(PropertyName = "integrity")]
        public string Integrity { get; set; }

        [JsonProperty(PropertyName = "shasum")]
        public string Shasum { get; set; }

        [JsonProperty(PropertyName = "tarball")]
        public string Tarball { get; set; }
    }

    [Serializable]
    public class VersionInfo
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }

        [JsonProperty(PropertyName = "dist")]
        public Dist Dist { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "author")]
        public object Author { get; set; }

        [JsonProperty(PropertyName = "dependencies")]
        public Dictionary<string, string> Dependencies { get; set; }

        [JsonProperty(PropertyName = "readme")]
        public string Readme { get; set; }
    }

    [Serializable]
    public class GitHubPackageDetailInfo
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "dist-tags")]
        public DistTags dist_tags{ get; set; }

        [JsonProperty(PropertyName = "versions")]
        public Dictionary<string, VersionInfo> Versions { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "author")]
        public object Author { get; set; }
    }

    [Serializable]
    public class DistTags
    {
        [JsonProperty(PropertyName = "latest")]
        public string Latest { get; set; }
    }
}