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
            var selectRequest = new SelectRequestBody()
            {
                EntryType = EntryType.Downloaded,
                OrderBy = new SelectRequestBody.SelectOrderBy[] { new SelectRequestBody.SelectOrderBy() { Order = Order.Desc, Sort = Sort.Date } },
                Limit = 20,
                Offset = 0
            };
            var response = await CloudFunction.Post(Request.Create("SELECT", "test_download_1", JsonConvert.SerializeObject(selectRequest)));
            Assert.That(response.IsSuccess, Is.True);

            var selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);
            Assert.That(selectData, Has.Count.EqualTo(2));
            Assert.That(selectData.Any(data => data.Metadata.Name == "User1"));
            Assert.That(selectData.Any(data => data.Metadata.Name == "User3"));

            response = await CloudFunction.Post(Request.Create("SELECT", "test_download_3", JsonConvert.SerializeObject(selectRequest)));

            Assert.True(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");

            selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);
            Assert.That(selectData, Has.Count.EqualTo(1));
            Assert.That(selectData.Any(data => data.Metadata.Name == "User4"));
        }

        [Test]
        public async Task Download_002_UnknownUser_ShouldReturnEmptyData()
        {
            var selectRequest = new SelectRequestBody()
            {
                EntryType = EntryType.Downloaded,
                OrderBy = new SelectRequestBody.SelectOrderBy[] { new SelectRequestBody.SelectOrderBy() { Order = Order.Desc, Sort = Sort.Date } },
                Limit = 20,
                Offset = 0
            };
            var response = await CloudFunction.Post(Request.Create("SELECT", "unknown", JsonConvert.SerializeObject(selectRequest)));

            Assert.True(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");

            var selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);
            Assert.That(selectData, Is.Empty);
        }

        [Test]
        public async Task Download_003_DownloadAndLike_ShouldBeNoDuplicate()
        {
            await CloudFunction.Clear("records");
            await CloudFunction.Clear("downloads");

            var id = await CloudFunction.Upload("user1", new UploadData() { Image = new byte[] { 1 }, Data = new byte[] { 2 } });
            await CloudFunction.Download("test_download_1", id);
            await CloudFunction.Like("test_like_1", id);
            await CloudFunction.Like("test_like_2", id);

            var selectRequest = new SelectRequestBody()
            {
                EntryType = EntryType.Downloaded,
                OrderBy = new SelectRequestBody.SelectOrderBy[] { new SelectRequestBody.SelectOrderBy() { Order = Order.Desc, Sort = Sort.Date } },
                Limit = 20,
                Offset = 0
            };
            var response = await CloudFunction.Post(Request.Create("SELECT", "test_download_1", JsonConvert.SerializeObject(selectRequest)));

            Assert.That(response.IsSuccess, Is.True);

            var selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);
            Assert.That(selectData.Count, Is.EqualTo(1));
        }
    }
}