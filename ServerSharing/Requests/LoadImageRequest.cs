using Ydb.Sdk.Table;
using ServerSharing.Data;
using Amazon.DynamoDBv2.Model;
using System.Net;
using Amazon.DynamoDBv2;

namespace ServerSharing
{
    public class LoadImageRequest : BaseRequest
    {
        private readonly AmazonDynamoDBClient _awsClient;

        public LoadImageRequest(AmazonDynamoDBClient awsClient, TableClient tableClient, Request request)
            : base(tableClient, request)
        { 
            _awsClient = awsClient;
        }

        protected async override Task<Response> Handle(TableClient client, Request request)
        {
            var awsRequest = new GetItemRequest(Tables.Images, new Dictionary<string, AttributeValue>()
            {
                { "id", new AttributeValue {S = request.body } }
            });

            var awsResponse = await _awsClient.GetItemAsync(awsRequest);

            if (awsResponse.HttpStatusCode != HttpStatusCode.OK)
                throw new InvalidOperationException("Error GetItemAsync: " + awsResponse);

            if (awsResponse.Item.Count == 0)
                throw new InvalidOperationException("Record not found!");
            
            var data = awsResponse.Item["image"].B.ToArray();

            return new Response((uint)Ydb.Sdk.StatusCode.Success, Ydb.Sdk.StatusCode.Success.ToString(), Convert.ToBase64String(data));
        }
    }
}