using Newtonsoft.Json;

namespace ServerSharing.Data
{
    [Serializable]
    public class UploadData
    {
        [JsonProperty("meta")] public RecordMetadata Metadata { get; init; }
        [JsonProperty("image")] public byte[] Image { get; init; }
        [JsonProperty("data")] public byte[] Data { get; init; }
    }
}