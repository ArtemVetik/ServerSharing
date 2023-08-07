using Newtonsoft.Json;
using ServerSharing;

namespace ServerSharingTests
{
    [TestFixture]
    public class Test_007_SelectLikeTests
    {
        [OneTimeSetUp]
        public async Task Setup()
        {
            await CloudFunction.Clear("records");
            await CloudFunction.Clear("likes");

            var id = await CloudFunction.Upload("user1", "{\"id\": 1}");
            await CloudFunction.Like("test_download_1", id);

            id = await CloudFunction.Upload("user2", "{\"id\": 2}");
            await CloudFunction.Like("test_download_2", id);

            id = await CloudFunction.Upload("user3", "{\"id\": 3}");
            await CloudFunction.Like("test_download_1", id);

            id = await CloudFunction.Upload("user4", "{\"id\": 4}");
            await CloudFunction.Like("test_download_3", id);
        }

        [Test]
        public async Task Download_001_CorrectUser_ShouldCorrectBody()
        {
            var selectRequest = "{\"entry_type\":\"liked\",\"order_by\":[{\"sort\":\"date\",\"order\":\"desc\"}],\"limit\":20,\"offset\":0}";
            var response = await CloudFunction.Post(new Request("SELECT", "test_download_1", selectRequest));

            Assert.True(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");

            var selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);
            Assert.That(selectData.Count, Is.EqualTo(2));
            Assert.That(selectData.Any(data => data.Body == "{\"id\": 1}"));
            Assert.That(selectData.Any(data => data.Body == "{\"id\": 3}"));

            response = await CloudFunction.Post(new Request("SELECT", "test_download_3", selectRequest));

            Assert.True(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");

            selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);
            Assert.That(selectData.Count, Is.EqualTo(1));
            Assert.That(selectData.Any(data => data.Body == "{\"id\": 4}"));
        }

        [Test]
        public async Task Download_002_UnknownUser_ShouldReturnEmptyData()
        {
            var selectRequest = "{\"entry_type\":\"liked\",\"order_by\":[{\"sort\":\"date\",\"order\":\"desc\"}],\"limit\":20,\"offset\":0}";
            var response = await CloudFunction.Post(new Request("SELECT", "unknown", selectRequest));

            Assert.True(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");

            var selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);
            Assert.That(selectData.Count, Is.EqualTo(0));
        }
    }
}