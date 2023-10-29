using Ydb.Sdk.Table;
using Ydb.Sdk.Value;
using System.Text;
using ServerSharing.Data;

namespace ServerSharing
{
    public class DislikeRequest : BaseRequest
    {
        public DislikeRequest(TableClient tableClient, Request request)
            : base(tableClient, request)
        { }

        protected async override Task<Response> Handle(TableClient client, Request request)
        {
            var response = await client.SessionExec(async session =>
            {
                var query = $@"
                    DECLARE $user_id AS string;
                    DECLARE $id AS string;

                    SELECT id
                    FROM `{Tables.Likes}`
                    WHERE id == $id AND user_id == $user_id;

                    DELETE FROM `{Tables.Likes}`
                    WHERE id == $id AND user_id == $user_id;
                ";

                return await session.ExecuteDataQuery(
                    query: query,
                    txControl: TxControl.BeginSerializableRW().Commit(),
                    parameters: new Dictionary<string, YdbValue>
                    {
                        { "$user_id", YdbValue.MakeString(Encoding.UTF8.GetBytes(request.user_id)) },
                        { "$id", YdbValue.MakeString(Encoding.UTF8.GetBytes(request.body)) },
                    }
                );
            });

            var queryResponse = (ExecuteDataQueryResponse)response;
            var resultSet = queryResponse.Result.ResultSets[0];

            if (resultSet.Rows.Count == 0)
                return new Response((uint)response.Status.StatusCode, response.Status.StatusCode.ToString(), string.Empty);

            response = await client.SessionExec(async session =>
            {
                var query = $@"
                    DECLARE $id AS string;
                    
                    UPDATE `{Tables.Records}`
                    SET like_count = like_count - 1u
                    WHERE id = $id;
                ";

                return await session.ExecuteDataQuery(
                    query: query,
                    txControl: TxControl.BeginSerializableRW().Commit(),
                    parameters: new Dictionary<string, YdbValue>
                    {
                        { "$id", YdbValue.MakeString(Encoding.UTF8.GetBytes(request.body)) },
                    }
                );
            });

            return new Response((uint)response.Status.StatusCode, response.Status.StatusCode.ToString(), string.Empty);
        }
    }
}