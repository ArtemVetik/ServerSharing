using Newtonsoft.Json;

namespace ServerSharing.Data
{
    public class SelectRequestBody
    {
        [JsonProperty("entry_type")] public EntryType EntryType { get; set; }
        [JsonProperty("order_by")] public SelectOrderBy[] OrderBy { get; set; }
        [JsonProperty("limit")] public ulong Limit { get; set; }
        [JsonProperty("offset")] public ulong Offset { get; set; }

        public class SelectOrderBy
        {
            [JsonProperty("sort")] public Sort Sort { get; set; }
            [JsonProperty("order")] public Order Order { get; set; }
        }
    }
}