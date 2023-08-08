using Newtonsoft.Json;

namespace ServerSharing
{
    public class RecordMetadata
    {
        [JsonProperty("name")] public string Name { get; init; }
        [JsonProperty("description")] public string Description { get; init; }
    }
}