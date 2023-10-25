using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Kyowon.Package
{
    [Serializable]
    public class KyowonPackageInfo
    {
        public string Name;
        public string Version;
        public string Description;
        public string DisplayName;      // TODO 가져올 것

        [JsonProperty("dist-tags")] private DistTags _distTags;
        public string Latest => _distTags?.Latest;

        public Dictionary<string, VersionInfo> Versions;


        [JsonIgnore] public bool IsInstalled => !string.IsNullOrEmpty(Version);
        [JsonIgnore] public bool HasUpdate => IsInstalled && Version != Latest;
    }
    [Serializable]
    public class KyowonPackageInfo_Installed
    {
        public string Name;
        public string Version;
        public string DisplayName;
    }

    [Serializable]
    public class DistTags
    {
        public string Latest;
    }

    [Serializable]
    public class VersionInfo
    {
        [JsonProperty("dist")]
        private Dist _dist;
        public string URL => _dist?.Tarball;


        public Dictionary<string, string> Dependencies;
        public string Description;
        public string Readme;
    }

    [Serializable]
    public class Dist
    {
        public string Tarball;
    }
}