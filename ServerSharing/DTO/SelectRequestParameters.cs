using Newtonsoft.Json;

namespace ServerSharing
{
    public class SelectRequestParameters
    {
        [JsonProperty("entry_type")] public EntryType EntryType { get; init; }
        [JsonProperty("sort")] public Sort Sort { get; init; }
        [JsonProperty("order")] public Order Order { get; init; }
        [JsonProperty("limit")] public ulong Limit { get; init; }
        [JsonProperty("offset")] public ulong Offset { get; init; }
    }
}