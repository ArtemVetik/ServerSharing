using Newtonsoft.Json;

namespace ServerSharing
{
    public class SelectRequestBody
    {
        [JsonProperty("entry_type")] public EntryType EntryType { get; init; }
        [JsonProperty("order_by")] public SelectOrderBy[] OrderBy { get; init; }
        [JsonProperty("limit")] public ulong Limit { get; init; }
        [JsonProperty("offset")] public ulong Offset { get; init; }

        public class SelectOrderBy
        {
            [JsonProperty("sort")] public Sort Sort { get; init; }
            [JsonProperty("order")] public Order Order { get; init; }
        }
    }
}