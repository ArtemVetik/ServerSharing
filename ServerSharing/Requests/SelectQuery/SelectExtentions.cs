using Newtonsoft.Json;
using ServerSharing.Data;
using System.Text;
using Ydb.Sdk.Value;

namespace ServerSharing
{
    internal static class SelectExtentions
    {
        public static SelectResponseData CreateResponseData(this ResultSet.Row row)
        {
            var downloadsCount = row["downloads.count"];
            var likesCount = row["likes.count"];
            var ratingsCount = row["ratings.count"];
            var ratingsAverage = row["ratings.avg"];

            return new SelectResponseData()
            {
                Id = Encoding.UTF8.GetString(row["records.id"].GetString()),
                Metadata = JsonConvert.DeserializeObject<RecordMetadata>(row["records.body"].GetOptionalJson()),
                Datetime = row["records.date"].GetOptionalDatetime() ?? DateTime.MinValue,
                Downloads = downloadsCount.TypeId == YdbTypeId.OptionalType ? (downloadsCount.GetOptionalUint64() ?? 0) : downloadsCount.GetUint64(),
                Likes = likesCount.TypeId == YdbTypeId.OptionalType ? (likesCount.GetOptionalUint64() ?? 0) : likesCount.GetUint64(),
                RatingCount = ratingsCount.TypeId == YdbTypeId.OptionalType ? (ratingsCount.GetOptionalUint64() ?? 0) : ratingsCount.GetUint64(),
                RatingAverage = ratingsAverage.TypeId == YdbTypeId.OptionalType ? (ratingsAverage.GetOptionalDouble() ?? 0) : ratingsAverage.GetDouble(),
                MyRating = row["myRating"].GetOptionalInt8(),
                MyLike = row["myLike"].GetBool(),
                MyDownload = row["myDownload"].GetBool(),
                MyRecord = row["myRecord"].GetBool(),
            };
        }
    }
}