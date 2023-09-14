using Newtonsoft.Json;
using System;

namespace ServerSharing.Data
{
    [Serializable]
    public class RecordMetadata
    {
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("compatibility_version")] public string CompatibilityVersion { get; set; }
    }
}