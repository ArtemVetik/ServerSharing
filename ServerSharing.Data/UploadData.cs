using Newtonsoft.Json;
using System;

namespace ServerSharing.Data
{
    [Serializable]
    public class UploadData
    {
        [JsonProperty("meta")] public RecordMetadata Metadata { get; set; }
        [JsonProperty("image")] public byte[] Image { get; set; }
        [JsonProperty("data")] public byte[] Data { get; set; }
    }
}