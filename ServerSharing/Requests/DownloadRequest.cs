using Ydb.Sdk.Table;
using Ydb.Sdk.Value;
using System.Text;
using ServerSharing.Data;
using Amazon.DynamoDBv2.Model;
using System.Net;
using Amazon.DynamoDBv2;

namespace ServerSharing
{
    public class DownloadRequest : BaseRequest
    {
        private readonly AmazonDynamoDBClient _awsClient;

        public DownloadRequest(AmazonDynamoDBClient awsClient, TableClient tableClient, Request request)
            : base(tableClient, request)
        {
            _awsClient = awsClient;
        }

        protected async override Task<Response> Handle(TableClient client, Request request)
        {
            var awsRequest = new GetItemRequest(Tables.Data, new Dictionary<string, AttributeValue>()
            {
                { "id", new AttributeValue {S = request.body } }
            });

            var awsResponse = await _awsClient.GetItemAsync(awsRequest);

            if (awsResponse.HttpStatusCode != HttpStatusCode.OK)
                throw new InvalidOperationException("Error GetItemAsync: " + awsResponse);

            if (awsResponse.Item.Count == 0)
                throw new InvalidOperationException("Record not found!");

            var response = await client.SessionExec(async session =>
            {
                var query = $@"
                    DECLARE $user_id AS string;
                    DECLARE $id AS string;

                    INSERT INTO `{Tables.Downloads}` (user_id, id)
                    VALUES ($user_id, $id);

                    UPDATE `{Tables.Records}`
                    SET download_count = if (download_count is null, 1u, download_count + 1u)
                    WHERE id = $id;
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

            var data = awsResponse.Item["data"].B.ToArray();
            return new Response((uint)Ydb.Sdk.StatusCode.Success, Ydb.Sdk.StatusCode.Success.ToString(), Convert.ToBase64String(data));
        }
    }
}