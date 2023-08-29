using Newtonsoft.Json;
using ServerSharing.Data;

namespace ServerSharing
{
    public class Program
    {
        private static readonly HttpClient _client = new();

        public static async Task Main(string[] args)
        {
            var request = new Request() { method = "CLEAR", user_id = "thisUser", body = "records" };
            var content = new StringContent(JsonConvert.SerializeObject(request));
            var response = await _client.PostAsync("https://functions.yandexcloud.net/d4e2m4vp43u2gjs1an5h?integration=raw", content);
            var responseString = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseString);

            request = new Request() { method = "CLEAR", user_id = "thisUser", body = "downloads" };
            content = new StringContent(JsonConvert.SerializeObject(request));
            response = await _client.PostAsync("https://functions.yandexcloud.net/d4e2m4vp43u2gjs1an5h?integration=raw", content);
            responseString = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseString);

            request = new Request() { method = "CLEAR", user_id = "thisUser", body = "likes" };
            content = new StringContent(JsonConvert.SerializeObject(request));
            response = await _client.PostAsync("https://functions.yandexcloud.net/d4e2m4vp43u2gjs1an5h?integration=raw", content);
            responseString = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseString);

            return;
            //var body = new SelectRequestBody()
            //{
            //    EntryType = EntryType.Downloaded,
            //    OrderBy = new SelectRequestBody.SelectOrderBy[]
            //    {
            //        new SelectRequestBody.SelectOrderBy() { Sort = Sort.RaingAverage, Order = Order.Desc },
            //        new SelectRequestBody.SelectOrderBy() { Sort = Sort.RaingCount, Order = Order.Desc }
            //    },
            //    Limit = 20,
            //    Offset = 0,
            //};

            //var request = new Request() { method = "SELECT", user_id = "thisUser", body = JsonConvert.SerializeObject(body) };
            //var content = new StringContent(JsonConvert.SerializeObject(request));

            //var response = await _client.PostAsync("https://functions.yandexcloud.net/d4eva0ud0d8cncdqa8uv?integration=raw", content);

            //var responseString = await response.Content.ReadAsStringAsync();

            //Console.WriteLine(responseString);
        }
    }
}