using Newtonsoft.Json;

namespace ServerSharing
{
    public class Program
    {
        private static readonly HttpClient client = new HttpClient();
        public static async Task Main(string[] args)
        {
            var body = new SelectRequestBody()
            {
                EntryType = EntryType.Downloaded,
                OrderBy = new SelectRequestBody.SelectOrderBy[]
                {
                    new SelectRequestBody.SelectOrderBy() { Sort = Sort.RaingAverage, Order = Order.Desc },
                    new SelectRequestBody.SelectOrderBy() { Sort = Sort.RaingCount, Order = Order.Desc }
                },
                Limit = 20,
                Offset = 0,
            };

            var request = new Request("SELECT", "thisUser", JsonConvert.SerializeObject(body));
            Console.WriteLine(JsonConvert.SerializeObject(request));
            return;
            var content = new StringContent(JsonConvert.SerializeObject(request));

            var response = await client.PostAsync("https://functions.yandexcloud.net/d4eva0ud0d8cncdqa8uv?integration=raw", content);

            var responseString = await response.Content.ReadAsStringAsync();

            Console.WriteLine(responseString);
        }
    }
}