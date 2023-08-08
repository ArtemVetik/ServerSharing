using System;
using Newtonsoft.Json;

namespace ServerSharing.Data
{
    public class SelectResponseData
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("metadata")] public RecordMetadata Metadata { get; set; }
        [JsonProperty("datetime")] public DateTime Datetime { get; set; }
        [JsonProperty("downloads")] public ulong Downloads { get; set; }
        [JsonProperty("likes")] public ulong Likes { get; set; }
        [JsonProperty("rating_count")] public ulong RatingCount { get; set; }
        [JsonProperty("rating_average")] public double RatingAverage { get; set; }
    }
}