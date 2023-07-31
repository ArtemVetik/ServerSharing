using Newtonsoft.Json;

namespace ServerSharing
{
    public class Program
    {
        private static readonly HttpClient client = new HttpClient();
        public static async Task Main(string[] args)
        {
            var body = new SelectRequestParameters()
            {
                EntryType = EntryType.Downloaded,
                Sort = Sort.Date,
                Order = Order.Desc,
                Limit = 20,
                Offset = 0,
            };

            var request = new Request("SELECT", "thisUser", JsonConvert.SerializeObject(body));
            var content = new StringContent(JsonConvert.SerializeObject(request));

            var response = await client.PostAsync("https://functions.yandexcloud.net/d4eva0ud0d8cncdqa8uv?integration=raw", content);

            var responseString = await response.Content.ReadAsStringAsync();

            Console.WriteLine(responseString);
        }
    }
}