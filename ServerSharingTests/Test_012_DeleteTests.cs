using Newtonsoft.Json;
using NUnit.Framework;
using ServerSharing.Data;

namespace ServerSharingTests
{
    [TestFixture]
    public class Test_012_DeleteTests
    {
        [SetUp]
        public async Task Setup()
        {
            await CloudFunction.Clear("records");
        }

        [Test]
        public async Task Delete_001_CorrectRecord_ShouldDelete()
        {
            var id = await CloudFunction.Upload("user1", new UploadData() { Image = new byte[] { 1 }, Data = new byte[] { 2 } });
            var response = await CloudFunction.Post(Request.Create("DELETE", "user1", id));
            Assert.That(response.IsSuccess, Is.True, response.Body);

            var selectData = await SelectAll();
            Assert.That(selectData.Count, Is.EqualTo(0));

            response = await CloudFunction.Post(Request.Create("DOWNLOAD", "test_user", id));
            Assert.False(response.IsSuccess, $"DOWNLOAD: {response.StatusCode}, {response.ReasonPhrase}");

            response = await CloudFunction.Post(Request.Create("LOAD_IMAGE", "test_user", id));
            Assert.False(response.IsSuccess, $"LOAD_IMAGE: {response.StatusCode}, {response.ReasonPhrase}");
        }

        [Test]
        public async Task Delete_002_InvalidUser_ShouldNotDelete()
        {
            var id = await CloudFunction.Upload("user1", new UploadData() { Image = new byte[] { 1 }, Data = new byte[] { 2 } });
            var response = await CloudFunction.Post(Request.Create("DELETE", "user1_1", id));
            Assert.That(response.IsSuccess, Is.True, response.Body);

            var selectData = await SelectAll();
            Assert.That(selectData.Count, Is.EqualTo(1));
        }

        private async Task<List<SelectResponseData>> SelectAll()
        {
            var selectRequest = "{\"entry_type\":\"all\",\"order_by\":[{\"sort\":\"date\",\"order\":\"desc\"}],\"limit\":20,\"offset\":0}";
            var response = await CloudFunction.Post(Request.Create("SELECT", "test_user", selectRequest));

            if (response.IsSuccess == false)
                throw new InvalidOperationException("Select error: " + response);

            return JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);
        }
    }
}