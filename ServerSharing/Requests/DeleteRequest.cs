using Ydb.Sdk.Table;
using Ydb.Sdk.Value;
using System.Text;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2;
using System.Net;
using ServerSharing.Data;

namespace ServerSharing
{
    public class DeleteRequest : BaseRequest
    {
        private readonly AmazonDynamoDBClient _awsClient;

        public DeleteRequest(AmazonDynamoDBClient awsClient, TableClient tableClient, Request request)
            : base(tableClient, request)
        { 
            _awsClient = awsClient;
        }

        protected async override Task<Response> Handle(TableClient client, Request request)
        {
            var awsRequest = new BatchWriteItemRequest(new Dictionary<string, List<WriteRequest>>()
            {
                {
                    Tables.Images, new List<WriteRequest>()
                    {
                         new WriteRequest(new Amazon.DynamoDBv2.Model.DeleteRequest(new Dictionary<string, AttributeValue>()
                         {
                             { "id", new AttributeValue() { S = request.body } }
                         })),
                    }
                },
                {
                    Tables.Data, new List<WriteRequest>()
                    {
                         new WriteRequest(new Amazon.DynamoDBv2.Model.DeleteRequest(new Dictionary<string, AttributeValue>()
                         {
                             { "id", new AttributeValue() { S = request.body } }
                         })),
                    }
                },
            });

            var awsResponse = await _awsClient.BatchWriteItemAsync(awsRequest);

            if (awsResponse.HttpStatusCode != HttpStatusCode.OK)
                throw new InvalidOperationException("Error BatchWriteItemAsync: " + awsResponse);

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