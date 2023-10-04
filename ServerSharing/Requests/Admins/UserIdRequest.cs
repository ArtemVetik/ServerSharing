#if ADMIN_ENVIRONMENT
using ServerSharing.Data;
using System.Text;
using Ydb.Sdk.Table;
using Ydb.Sdk.Value;

namespace ServerSharing
{
    public class UserIdRequest : BaseRequest
    {
        public UserIdRequest(TableClient tableClient, Request request) : base(tableClient, request)
        { }

        protected async override Task<Response> Handle(TableClient client, Request request)
        {
            var response = await client.SessionExec(async session =>
            {
                var query = $@"
                    DECLARE $id AS string;

                    SELECT user_id
                    FROM `{Tables.Records}`
                    WHERE id = $id;
                ";

                return await session.ExecuteDataQuery(
                    query: query,
                    txControl: TxControl.BeginSerializableRW().Commit(),
                    parameters: new Dictionary<string, YdbValue>
                    {
                        { "$id", YdbValue.MakeString(Encoding.UTF8.GetBytes(request.body)) }
                    }
                );
            });

            if (response.Status.IsSuccess == false)
                return new Response((uint)response.Status.StatusCode, response.Status.StatusCode.ToString(), string.Empty);

            var queryResponse = (ExecuteDataQueryResponse)response;

            if (queryResponse.Result.ResultSets[0].Rows.Count > 1)
                throw new InvalidOperationException("Several records with this id were found!");

            if (queryResponse.Result.ResultSets[0].Rows.Count == 0)
                throw new InvalidOperationException("Id not found!");

            var user_id = queryResponse.Result.ResultSets[0].Rows[0]["user_id"].GetString();

            return new Response((uint)response.Status.StatusCode, response.Status.StatusCode.ToString(), Encoding.UTF8.GetString(user_id));
        }
    }
}
#endif