using System.Text.Json.Serialization;

namespace ServerSharing
{
    public class SelectResponseData
    {
        [JsonPropertyName("id")] public string Id { get; init; }
        [JsonPropertyName("body")] public string Body { get; init; }
        [JsonPropertyName("datetime")] public DateTime Datetime { get; init; }
        [JsonPropertyName("downloads")] public ulong Downloads { get; init; }
        [JsonPropertyName("likes")] public ulong Likes { get; init; }
        [JsonPropertyName("rating_count")] public ulong RatingCount { get; init; }
        [JsonPropertyName("rating_average")] public double RatingAverage { get; init; }
    }
}