#if TEST_ENVIRONMENT
using Ydb.Sdk.Table;
using Ydb.Sdk.Value;

namespace ServerSharing
{
    public class ClearRequest : BaseRequest
    {
        public ClearRequest(TableClient tableClient, Request request)
            : base(tableClient, request)
        { }

        protected async override Task<Response> Handle(TableClient client, Request request)
        {
            var table = request.body switch
            {
                "records" => Records.TablePath,
                "downloads" => Downloads.TablePath,
                "likes" => Likes.TablePath,
                "rating" => RatingTable.TablePath,
                _ => throw new InvalidOperationException()
            };

            var response = await client.SessionExec(async session =>
            {
                var query = $@"
                    delete from `{table}` on
                    (select * from `{table}`);
                ";

                return await session.ExecuteDataQuery(
                    query: query,
                    txControl: TxControl.BeginSerializableRW().Commit(),
                    parameters: new Dictionary<string, YdbValue>()
                );
            });

            return new Response((uint)response.Status.StatusCode, response.Status.StatusCode.ToString(), string.Empty);
        }
    }
}
#endif