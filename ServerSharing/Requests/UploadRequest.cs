using Ydb.Sdk.Table;
using Ydb.Sdk.Value;
using System.Text;

namespace ServerSharing
{
    public class UploadRequest : BaseRequest
    {
        public UploadRequest(TableClient tableClient, Request request)
            : base(tableClient, request)
        { }

        protected async override Task<Response> Handle(TableClient client, Request request)
        {
            var response = await client.SessionExec(async session =>
            {
                var query = $@"
                    DECLARE $id AS string;
                    DECLARE $user_id AS string;
                    DECLARE $body AS json;
                    DECLARE $date AS Datetime;

                    UPSERT INTO `{Tables.Records}` (id, user_id, body, date)
                    VALUES ($id, $user_id, $body, $date);
                ";

                return await session.ExecuteDataQuery(
                    query: query,
                    txControl: TxControl.BeginSerializableRW().Commit(),
                    parameters: new Dictionary<string, YdbValue>
                    {
                        { "$id", YdbValue.MakeString(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString())) },
                        { "$user_id", YdbValue.MakeString(Encoding.UTF8.GetBytes(request.user_id)) },
                        { "$body", YdbValue.MakeJson(request.body) },
                        { "$date", YdbValue.MakeDatetime(DateTime.UtcNow) }
                    }
                );
            });

            return new Response((uint)response.Status.StatusCode, response.Status.StatusCode.ToString(), string.Empty);
        }
    }
}