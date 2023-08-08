using Amazon.Runtime.Endpoints;

namespace ServerSharing
{
    public class EndpointProvider : IEndpointProvider
    {
        public Endpoint ResolveEndpoint(EndpointParameters parameters)
        {
            return new Endpoint(Environment.GetEnvironmentVariable("DocumentEndpoint"));
        }
    }
}