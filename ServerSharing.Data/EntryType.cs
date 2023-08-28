using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ServerSharing.Data
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum EntryType
    {
        All,
        Downloaded,
        Uploaded,
        Liked,
    }
}