using Newtonsoft.Json;
using NUnit.Framework;
using ServerSharing.Data;

namespace ServerSharingTests
{
    [TestFixture]
    public class Test_005_SelectDownloadedTests
    {
        [OneTimeSetUp]
        public async Task Setup()
        {
            await CloudFunction.Clear("records");
            await CloudFunction.Clear("downloads");

            var image = new byte[] { 0 };
            var data = new byte[] { 0 };

            var id = await CloudFunction.Upload("user1", new UploadData() { Metadata = new RecordMetadata() { Name = "User1" }, Image = image, Data = data });
            await CloudFunction.Download("test_download_1", id);

            id = await CloudFunction.Upload("user2", new UploadData() { Metadata = new RecordMetadata() { Name = "User2" }, Image = image, Data = data });
            await CloudFunction.Download("test_download_2", id);

            id = await CloudFunction.Upload("user3", new UploadData() { Metadata = new RecordMetadata() { Name = "User3" }, Image = image, Data = data });
            await CloudFunction.Download("test_download_1", id);

            id = await CloudFunction.Upload("user4", new UploadData() { Metadata = new RecordMetadata() { Name = "User4" }, Image = image, Data = data });
            await CloudFunction.Download("test_download_3", id);
        }

        [Test]
        public async Task Download_001_CorrectUser_ShouldCorrectBody()
        {
            var selectRequest = "{\"entry_type\":\"downloaded\",\"order_by\":[{\"sort\":\"date\",\"order\":\"desc\"}],\"limit\":20,\"offset\":0}";
            var response = await CloudFunction.Post(Request.Create("SELECT", "test_download_1", selectRequest));

            Assert.True(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");

            var selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);
            Assert.That(selectData, Has.Count.EqualTo(2));
            Assert.That(selectData.Any(data => data.Metadata.Name == "User1"));
            Assert.That(selectData.Any(data => data.Metadata.Name == "User3"));

            response = await CloudFunction.Post(Request.Create("SELECT", "test_download_3", selectRequest));

            Assert.True(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");

            selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);
            Assert.That(selectData, Has.Count.EqualTo(1));
            Assert.That(selectData.Any(data => data.Metadata.Name == "User4"));
        }

        [Test]
        public async Task Download_002_UnknownUser_ShouldReturnEmptyData()
        {
            var selectRequest = "{\"entry_type\":\"downloaded\",\"order_by\":[{\"sort\":\"date\",\"order\":\"desc\"}],\"limit\":20,\"offset\":0}";
            var response = await CloudFunction.Post(Request.Create("SELECT", "unknown", selectRequest));

            Assert.True(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");

            var selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);
            Assert.That(selectData, Is.Empty);
        }
    }
}