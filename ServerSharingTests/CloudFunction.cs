using Newtonsoft.Json;
using ServerSharing;

namespace ServerSharingTests
{
    public static class CloudFunction
    {
        private static readonly string FunctionId = "d4eva0ud0d8cncdqa8uv";
        private static readonly HttpClient client = new();

        public static async Task<Response> Post(Request request)
        {
            var content = new StringContent(JsonConvert.SerializeObject(request));
            var response = await client.PostAsync($"https://functions.yandexcloud.net/{FunctionId}?integration=raw", content);
            
            var responseString = await response.Content.ReadAsStringAsync();

            var responseObject = JsonConvert.DeserializeObject<Response>(responseString);

            if (responseObject.IsSuccess == false)
                return new Response(responseObject.StatusCode, responseObject.ReasonPhrase, responseString);

            return responseObject;
        }

        public static async Task Clear(string tableName)
        {
            var response = await CloudFunction.Post(new Request("CLEAR", "", tableName));

            if (response.IsSuccess == false)
                throw new InvalidOperationException($"Can't clear {tableName} table: {response.Body}");
        }

        public static async Task<string> Upload(string userId, UploadData body)
        {
            var response = await CloudFunction.Post(new Request("UPLOAD", userId, JsonConvert.SerializeObject(body)));

            if (response.IsSuccess == false)
                throw new InvalidOperationException($"Upload error: {response.Body}");

            return response.Body;
        }

        public static async Task Download(string userId, string id)
        {
            var response = await CloudFunction.Post(new Request("DOWNLOAD", userId, id));

            if (response.IsSuccess == false)
                throw new InvalidOperationException($"Upload error: {response.Body}");
        }

        public static async Task Like(string userId, string id)
        {
            var response = await CloudFunction.Post(new Request("LIKE", userId, id));

            if (response.IsSuccess == false)
                throw new InvalidOperationException($"Upload error: {response.Body}");
        }

        public static async Task Rate(string userId, string id, sbyte rating)
        {
            var response = await CloudFunction.Post(new Request("RATE", userId, JsonConvert.SerializeObject(new RatingRequestBody()
            {
                Id = id,
                Rating = rating,
            })));

            if (response.IsSuccess == false)
                throw new InvalidOperationException($"Upload error: {response.Body}");
        }
    }
}