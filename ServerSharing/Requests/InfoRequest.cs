using Newtonsoft.Json;
using Ydb.Sdk.Table;
using ServerSharing.Data;

namespace ServerSharing
{
    public class InfoRequest : BaseRequest
    {
        public InfoRequest(TableClient tableClient, Request request)
            : base(tableClient, request)
        { }

        protected async override Task<Response> Handle(TableClient client, Request request)
        {
            var response = await new InfoQueryFactory(client, request.user_id, request.body).Create();

            if (response.Status.IsSuccess == false)
                return new Response((uint)response.Status.StatusCode, response.Status.StatusCode.ToString(), string.Empty);

            var queryResponse = (ExecuteDataQueryResponse)response;
            var resultSet = queryResponse.Result.ResultSets[0];

            if (resultSet.Rows.Count == 0)
                throw new InvalidOperationException("Record not found!");

            var responseData = resultSet.Rows[0].CreateResponseData();

            return new Response((uint)response.Status.StatusCode, response.Status.StatusCode.ToString(), JsonConvert.SerializeObject(responseData));
        }
    }
}