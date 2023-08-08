
namespace ServerSharing.Data
{
    public class Request
    {
        public string method { get; set; }
        public string user_id { get; set; }
        public string body { get; set; }

        public static Request Create(string method, string userId, string body)
        {
            return new Request()
            {
                method = method,
                user_id = userId,
                body = body,
            };
        }
    }
}