using Ydb.Sdk.Table;
using Ydb.Sdk.Value;
using System.Text;

namespace ServerSharing
{
    public class DownloadRequest : BaseRequest
    {
        public DownloadRequest(TableClient tableClient, Request request)
            : base(tableClient, request)
        { }

        protected async override Task<Response> Handle(TableClient client, Request request)
        {
            var response = await client.SessionExec(async session =>
            {
                var query = $@"
                    DECLARE $id AS string;

                    SELECT records.body
                    FROM `{Tables.Records}` records
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

            if (queryResponse.Result.ResultSets[0].Rows.Count == 0)
                throw new InvalidOperationException("Record not found!");

            response = await client.SessionExec(async session =>
            {
                var query = $@"
                    DECLARE $user_id AS string;
                    DECLARE $id AS string;

                    UPSERT INTO `{Tables.Downloads}` (user_id, id)
                    VALUES ($user_id, $id);
                ";

                return await session.ExecuteDataQuery(
                    query: query,
                    txControl: TxControl.BeginSerializableRW().Commit(),
                    parameters: new Dictionary<string, YdbValue>
                    {
                        { "$user_id", YdbValue.MakeString(Encoding.UTF8.GetBytes(request.user_id)) },
                        { "$id", YdbValue.MakeString(Encoding.UTF8.GetBytes(request.body)) }
                    }
                );
            });

            var data = queryResponse.Result.ResultSets[0].Rows[0]["body"].GetOptionalJson();

            return new Response((uint)response.Status.StatusCode, response.Status.StatusCode.ToString(), data);
        }
    }
}