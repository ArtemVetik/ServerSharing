using System.Text.Json;

namespace ServerSharing
{
    public class Program
    {
        private static readonly HttpClient client = new HttpClient();

        public static async Task Main(string[] args)
        {
            var body = new
            {
                column = "date",
                order = "desc",
                limit = 20,
                offset = 0,
                onlyself = false,
            };

            var request = new Request("SELECT", "server", JsonSerializer.Serialize(body));
            var content = new StringContent(JsonSerializer.Serialize(request));

            var response = await client.PostAsync("https://functions.yandexcloud.net/d4eva0ud0d8cncdqa8uv?integration=raw", content);

            var responseString = await response.Content.ReadAsStringAsync();

            Console.WriteLine(responseString);
        }
    }
}