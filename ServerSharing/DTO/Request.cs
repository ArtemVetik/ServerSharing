namespace ServerSharing
{
    public record Request(string method, string user_id, string body);
}