using System.Text.Json.Serialization;

namespace ServerSharing
{
    public class SelectRequestParameters
    {
        [JsonPropertyName("column")] public string Column { get; init; }
        [JsonPropertyName("order")] public string Order { get; init; }
        [JsonPropertyName("limit")] public ulong Limit { get; init; }
        [JsonPropertyName("offset")] public ulong Offset { get; init; }
        [JsonPropertyName("onlyself")] public bool Self { get; init; }
    }
}