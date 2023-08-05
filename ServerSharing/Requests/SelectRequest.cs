using System.Text;
using Newtonsoft.Json;
using Ydb.Sdk.Table;
using Ydb.Sdk.Value;

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
                var downloadsCount = row[$"{Downloads.Name}.count"];
                var likesCount = row[$"{Likes.Name}.count"];

                responseData.Add(new SelectResponseData()
                {
                    Id = Encoding.UTF8.GetString(row[Records.Id].GetString()),
                    Body = row[Records.Body].GetOptionalJson(),
                    Datetime = row[Records.Date].GetOptionalDatetime() ?? DateTime.MinValue,
                    Downloads = downloadsCount.TypeId == YdbTypeId.OptionalType ? (downloadsCount.GetOptionalUint64() ?? 0) : downloadsCount.GetUint64(),
                    Likes = likesCount.TypeId == YdbTypeId.OptionalType ? (likesCount.GetOptionalUint64() ?? 0) : likesCount.GetUint64(),
                });
            }

            return new Response((uint)response.Status.StatusCode, response.Status.StatusCode.ToString(), JsonConvert.SerializeObject(responseData));
        }
    }
}