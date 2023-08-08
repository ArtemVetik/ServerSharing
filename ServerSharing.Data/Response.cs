namespace ServerSharing.Data
{
    public class Response
    {
        public Response(uint statusCode, string reasonPhrase, string body)
        {
            StatusCode = statusCode;
            Body = body;
            ReasonPhrase = reasonPhrase;
        }

        public bool IsSuccess => StatusCode == (uint)Ydb.Sdk.StatusCode.Success;
        public uint StatusCode { get; init; }
        public string ReasonPhrase { get; init; }
        public string Body { get; init; }
    }
}