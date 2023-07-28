using System.Text.Json.Serialization;

namespace ServerSharing
{
    public class SelectResponseData
    {
        [JsonPropertyName("id")] public string Id { get; init; }
        [JsonPropertyName("record")] public string Record { get; init; }
        [JsonPropertyName("datetime")] public DateTime Datetime { get; init; }
        [JsonPropertyName("downloads")] public ulong Downloads { get; init; }
        [JsonPropertyName("likes")] public ulong Likes { get; init; }
    }
}