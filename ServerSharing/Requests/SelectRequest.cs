using System.Text;
using Newtonsoft.Json;
using Ydb.Sdk.Table;
using Ydb.Sdk.Value;
using ServerSharing.Data;

namespace ServerSharing
{
    public partial class SelectRequest : BaseRequest
    {
        public SelectRequest(TableClient tableClient, Request request)
            : base(tableClient, request)
        { }

        protected async override Task<Response> Handle(TableClient client, Request request)
        {
            var response = await new SelectQueryFactory(client, request.user_id, request.body).CreateQuery();

            if (response.Status.IsSuccess == false)
                return new Response((uint)response.Status.StatusCode, response.Status.StatusCode.ToString(), string.Empty);

            var queryResponse = (ExecuteDataQueryResponse)response;
            var resultSet = queryResponse.Result.ResultSets[0];

            var responseData = new List<SelectResponseData>();

            foreach (var row in resultSet.Rows)
            {
                var downloadsCount = row["downloads.count"];
                var likesCount = row["likes.count"];
                var ratingsCount = row["ratings.count"];
                var ratingsAverage = row["ratings.avg"];

                responseData.Add(new SelectResponseData()
                {
                    Id = Encoding.UTF8.GetString(row["records.id"].GetString()),
                    Metadata = JsonConvert.DeserializeObject<RecordMetadata>(row["records.body"].GetOptionalJson()),
                    Datetime = row["records.date"].GetOptionalDatetime() ?? DateTime.MinValue,
                    Downloads = downloadsCount.TypeId == YdbTypeId.OptionalType ? (downloadsCount.GetOptionalUint64() ?? 0) : downloadsCount.GetUint64(),
                    Likes = likesCount.TypeId == YdbTypeId.OptionalType ? (likesCount.GetOptionalUint64() ?? 0) : likesCount.GetUint64(),
                    RatingCount = ratingsCount.TypeId == YdbTypeId.OptionalType ? (ratingsCount.GetOptionalUint64() ?? 0) : ratingsCount.GetUint64(),
                    RatingAverage = ratingsAverage.TypeId == YdbTypeId.OptionalType ? (ratingsAverage.GetOptionalDouble() ?? 0) : ratingsAverage.GetDouble(),
                });
            }

            return new Response((uint)response.Status.StatusCode, response.Status.StatusCode.ToString(), JsonConvert.SerializeObject(responseData));
        }
    }
}