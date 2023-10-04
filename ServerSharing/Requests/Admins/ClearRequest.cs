#if ADMIN_ENVIRONMENT
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using ServerSharing.Data;
using Ydb.Sdk.Table;
using Ydb.Sdk.Value;

namespace ServerSharing
{
    public class ClearRequest : BaseRequest
    {
        private readonly AmazonDynamoDBClient _awsClient;

        public ClearRequest(AmazonDynamoDBClient awsClient, TableClient tableClient, Request request)
            : base(tableClient, request)
        {
            _awsClient = awsClient;
        }

        protected async override Task<Response> Handle(TableClient client, Request request)
        {
            var table = request.body switch
            {
                "records" => Tables.Records,
                "downloads" => Tables.Downloads,
                "likes" => Tables.Likes,
                "rating" => Tables.Ratings,
                "images" => Tables.Images,
                "data" => Tables.Data,
                _ => throw new InvalidOperationException()
            };

            if (table == Tables.Images || table == Tables.Data)
                return await ClearAwsTable(table);

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

        private async Task<Response> ClearAwsTable(string table)
        {
            var bynaryAttribute = table == Tables.Data ? "data" : table == Tables.Images ? "image" : throw new ArgumentNullException(nameof(table));

            try
            {
                await _awsClient.DeleteTableAsync(table);
            }
            catch (Exception) { }

            var request = new CreateTableRequest
            {
                AttributeDefinitions = new List<AttributeDefinition>()
                {
                    new AttributeDefinition
                    {
                        AttributeName = "id",
                        AttributeType = "S"
                    },
                    new AttributeDefinition
                    {
                        AttributeName = bynaryAttribute,
                        AttributeType = "B"
                    }
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement
                    {
                        AttributeName = "id",
                        KeyType = "HASH"
                    }
                },
                TableName = table
            };

            var awsResponse = await _awsClient.CreateTableAsync(request);

            return new Response((uint)awsResponse.HttpStatusCode, awsResponse.HttpStatusCode.ToString(), string.Empty);
        }
    }
}
#endif