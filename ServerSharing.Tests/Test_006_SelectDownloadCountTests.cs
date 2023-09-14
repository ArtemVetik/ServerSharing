using Newtonsoft.Json;
using NUnit.Framework;
using ServerSharing.Data;

namespace ServerSharing.Tests
{
    [TestFixture]
    public class Test_006_SelectDownloadCountTests
    {
        private string _id1;
        private string _id2;

        [OneTimeSetUp]
        public async Task Setup()
        {
            await CloudFunction.Clear("records");
            await CloudFunction.Clear("downloads");

            _id1 = await CloudFunction.Upload("user1", new UploadData() { Image = new byte[] { }, Data = new byte[] { } });
            _id2 = await CloudFunction.Upload("user2", new UploadData() { Image = new byte[] { }, Data = new byte[] { } });

            await CloudFunction.Download("test_download_1", _id1);
            await CloudFunction.Download("test_download_2", _id1);
            await CloudFunction.Download("test_download_3", _id2);
        }

        [Test]
        public async Task Select_001_SelectUploadedId1_ShouldReturnCorrectCount()
        {
            var selectRequest = "{\"entry_type\":\"uploaded\",\"order_by\":[{\"sort\":\"date\",\"order\":\"desc\"}],\"limit\":20,\"offset\":0}";
            var response = await CloudFunction.Post(Request.Create("SELECT", "user1", selectRequest));

            Assert.True(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");

            var selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);

            Assert.That(selectData[0].Downloads, Is.EqualTo(2));
        }

        [Test]
        public async Task Select_002_SelectDownloadedId1_ShouldReturnCorrectCount()
        {
            var selectRequest = "{\"entry_type\":\"downloaded\",\"order_by\":[{\"sort\":\"date\",\"order\":\"desc\"}],\"limit\":20,\"offset\":0}";
            var response = await CloudFunction.Post(Request.Create("SELECT", "test_download_1", selectRequest));

            Assert.True(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");

            var selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);

            Assert.That(selectData[0].Downloads, Is.EqualTo(2));
        }

        [Test]
        public async Task Select_003_TwiceDownload_ShouldBeNotIncreaseDownloadCount()
        {
            await CloudFunction.Download("test_download_1", _id1);
            await CloudFunction.Download("test_download_2", _id1);

            var selectRequest = "{\"entry_type\":\"uploaded\",\"order_by\":[{\"sort\":\"date\",\"order\":\"desc\"}],\"limit\":20,\"offset\":0}";
            var response = await CloudFunction.Post(Request.Create("SELECT", "user1", selectRequest));

            Assert.True(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");

            var selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);

            Assert.That(selectData[0].Downloads, Is.EqualTo(2));
        }
    }
}