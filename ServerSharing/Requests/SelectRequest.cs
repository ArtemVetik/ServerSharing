using Newtonsoft.Json;
using Ydb.Sdk.Table;
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
                responseData.Add(row.CreateResponseData());

            return new Response((uint)response.Status.StatusCode, response.Status.StatusCode.ToString(), JsonConvert.SerializeObject(responseData));
        }
    }
}