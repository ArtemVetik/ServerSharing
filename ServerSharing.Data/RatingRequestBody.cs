using Newtonsoft.Json;
using System;

namespace ServerSharing.Data
{
    [Serializable]
    public class RatingRequestBody
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("rating")] public sbyte Rating { get; set; }
    }
}