using Ydb.Sdk.Auth;
using Ydb.Sdk;
using Ydb.Sdk.Table;
using Yandex.Cloud.Credentials;
using Yandex.Cloud.Functions;

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

            using var driver = new Driver(config);
            await driver.Initialize();

            using var tableClient = new TableClient(driver, new TableClientConfig());

            BaseRequest requestHandler = request.method switch
            {
                "UPLOAD" => new UploadRequest(tableClient, request),
                "DELETE" => new DeleteRequest(tableClient, request),
                "DOWNLOAD" => new DownloadRequest(tableClient, request),
                "SELECT" => new SelectRequest(tableClient, request),
                "LIKE" => new LikeRequest(tableClient, request),
                _ => throw new InvalidOperationException($"Method {request.method} not found")
            };

            return await requestHandler.Handle();
        }
    }
}