using Newtonsoft.Json;
using ServerSharing;

namespace ServerSharingTests
{
    [TestFixture]
    public class Test_011_DeleteTests
    {
        [SetUp]
        public async Task Setup()
        {
            await CloudFunction.Clear("records");
        }

        [Test]
        public async Task Delete_001_CorrectRecord_ShouldDelete()
        {
            var id = await CloudFunction.Upload("user1", "{}");
            var response = await CloudFunction.Post(new Request("DELETE", "user1", id));

            Assert.True(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");

            var selectData = await SelectAll();

            Assert.That(selectData.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task Delete_002_InvalidUser_ShouldNotDelete()
        {
            var id = await CloudFunction.Upload("user1", "{}");
            var response = await CloudFunction.Post(new Request("DELETE", "user1_1", id));

            Assert.True(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");

            var selectData = await SelectAll();

            Assert.That(selectData.Count, Is.EqualTo(1));
        }

        private async Task<List<SelectResponseData>> SelectAll()
        {
            var selectRequest = "{\"entry_type\":\"all\",\"order_by\":[{\"sort\":\"date\",\"order\":\"desc\"}],\"limit\":20,\"offset\":0}";
            var response = await CloudFunction.Post(new Request("SELECT", "test_user", selectRequest));

            if (response.IsSuccess == false)
                throw new InvalidOperationException("Select error: " + response);

            return JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);
        }
    }
}