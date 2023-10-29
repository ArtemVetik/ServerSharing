using Newtonsoft.Json;
using ServerSharing.Data;

namespace ServerSharing.Tests
{
    public static class CloudFunction
    {
        private static readonly string FunctionId = "d4e2m4vp43u2gjs1an5h";
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
            var response = await CloudFunction.Post(Request.Create("CLEAR", "", tableName));

            if (response.IsSuccess == false)
                throw new InvalidOperationException($"Can't clear {tableName} table: {response.Body}");
        }

        public static async Task<string> Upload(string userId, UploadData body)
        {
            var response = await CloudFunction.Post(Request.Create("UPLOAD", userId, JsonConvert.SerializeObject(body)));

            if (response.IsSuccess == false)
                throw new InvalidOperationException($"Upload error: {response.Body}");

            return response.Body;
        }

        public static async Task Download(string userId, string id)
        {
            var response = await CloudFunction.Post(Request.Create("DOWNLOAD", userId, id));

            if (response.IsSuccess == false)
                throw new InvalidOperationException($"Download error: {response.Body}");
        }

        public static async Task Like(string userId, string id)
        {
            var response = await CloudFunction.Post(Request.Create("LIKE", userId, id));

            if (response.IsSuccess == false)
                throw new InvalidOperationException($"Like error: {response.Body}");
        }

        public static async Task Rate(string userId, string id, sbyte rating)
        {
            var response = await CloudFunction.Post(Request.Create("RATE", userId, JsonConvert.SerializeObject(new RatingRequestBody()
            {
                Id = id,
                Rating = rating,
            })));

            if (response.IsSuccess == false)
                throw new InvalidOperationException($"Rate error: {response.Body}");
        }

        public static async Task Delete(string userId, string id)
        {
            var response = await CloudFunction.Post(Request.Create("DELETE", userId, id));

            if (response.IsSuccess == false)
                throw new InvalidOperationException($"Delete error: {response.Body}");
        }
    }
}