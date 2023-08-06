using Ydb.Sdk.Table;
using Ydb.Sdk.Value;
using System.Text;

namespace ServerSharing
{
    public class DeleteRequest : BaseRequest
    {
        public DeleteRequest(TableClient tableClient, Request request)
            : base(tableClient, request)
        { }

        protected async override Task<Response> Handle(TableClient client, Request request)
        {
            var response = await client.SessionExec(async session =>
            {
                var query = $@"
                    DECLARE $id AS string;
                    DECLARE $user_id AS string;

                    DELETE FROM `{Tables.Records}`
                    WHERE id == $id AND user_id == $user_id;
                ";

                return await session.ExecuteDataQuery(
                    query: query,
                    txControl: TxControl.BeginSerializableRW().Commit(),
                    parameters: new Dictionary<string, YdbValue>
                    {
                        { "$id", YdbValue.MakeString(Encoding.UTF8.GetBytes(request.body)) },
                        { "$user_id", YdbValue.MakeString(Encoding.UTF8.GetBytes(request.user_id)) }
                    }
                );
            });

            return new Response((uint)response.Status.StatusCode, response.Status.StatusCode.ToString(), string.Empty);
        }
    }
}