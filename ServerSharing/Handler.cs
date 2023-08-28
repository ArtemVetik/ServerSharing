using Ydb.Sdk.Auth;
using Ydb.Sdk;
using Ydb.Sdk.Table;
using Yandex.Cloud.Credentials;
using Yandex.Cloud.Functions;
using Amazon.DynamoDBv2;
using Amazon;
using ServerSharing.Data;

namespace ServerSharing
{
    public class Handler : YcFunction<Request, Task<Response>>
    {
        public async Task<Response> FunctionHandler(Request request, Context context)
        {
            string ydbEndpoint = Environment.GetEnvironmentVariable("YdbEndpoint");
            string ydbDatabase = Environment.GetEnvironmentVariable("YdbDatabase");

            var token = new TokenProvider(new MetadataCredentialsProvider().GetToken());
            var config = new DriverConfig(ydbEndpoint, ydbDatabase, token);
            
            var driver = new Driver(config);
            await driver.Initialize();

            var tableClient = new TableClient(driver, new TableClientConfig());
            
            var awsConfig = new AmazonDynamoDBConfig()
            {
                RegionEndpoint = RegionEndpoint.EUCentral1,
                EndpointProvider = new EndpointProvider(),
            };
            
            var awsAccessKeyId = Environment.GetEnvironmentVariable("awsAccessKeyId");
            var awsSecretAccessKey = Environment.GetEnvironmentVariable("awsSecretAccessKey");

            var awsClient = new AmazonDynamoDBClient(awsAccessKeyId, awsSecretAccessKey, awsConfig);

            try
            {
                BaseRequest requestHandler = request.method switch
                {
                    "UPLOAD" => new UploadRequest(awsClient, tableClient, request),
                    "DELETE" => new DeleteRequest(awsClient, tableClient, request),
                    "LOAD_IMAGE" => new LoadImageRequest(tableClient, request),
                    "DOWNLOAD" => new DownloadRequest(tableClient, request),
                    "SELECT" => new SelectRequest(tableClient, request),
                    "LIKE" => new LikeRequest(tableClient, request),
                    "RATE" => new RateRequest(tableClient, request),
                    "COUNT" => new CountRequest(tableClient, request),
#if TEST_ENVIRONMENT
                "CLEAR" => new ClearRequest(awsClient, tableClient, request),
#endif
                    _ => throw new InvalidOperationException($"Method {request.method} not found")
                };

                return await requestHandler.Handle();
            }
            finally
            {
                awsClient.Dispose();
                tableClient.Dispose();
                driver.Dispose();
            }
        }
    }
}