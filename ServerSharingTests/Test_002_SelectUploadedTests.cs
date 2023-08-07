using Newtonsoft.Json;
using ServerSharing;

namespace ServerSharingTests
{
    [TestFixture]
    public class Test_002_SelectUploadedTests
    {
        private readonly string _userOne = "user_1";
        private readonly string _userTwo = "user_2";

        [OneTimeSetUp]
        public async Task Setup()
        {
            await CloudFunction.Clear("records");
            await CloudFunction.Upload(_userOne, "{\"id\":\"someId\"}");
            await CloudFunction.Upload(_userOne, "{\"id\":\"qwerty\"}");
            await CloudFunction.Upload(_userTwo, "{\"id\":\"secondUser\"}");
        }

        [Test]
        public async Task Select_001_UserOne_ShouldCorrectCount()
        {
            var selectRequest = "{\"entry_type\":\"uploaded\",\"order_by\":[{\"sort\":\"date\",\"order\":\"desc\"}],\"limit\":20,\"offset\":0}";
            var response = await CloudFunction.Post(new Request("SELECT", _userOne, selectRequest));

            Assert.True(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");

            var selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);

            Assert.That(selectData.Count, Is.EqualTo(2));
            Assert.That(selectData.Any(data => data.Body.Contains("someId")));
            Assert.That(selectData.Any(data => data.Body.Contains("qwerty")));
        }

        [Test]
        public async Task Select_002_UserTwo_ShouldCorrectCount()
        {
            var selectRequest = "{\"entry_type\":\"uploaded\",\"order_by\":[{\"sort\":\"date\",\"order\":\"desc\"}],\"limit\":20,\"offset\":0}";
            var response = await CloudFunction.Post(new Request("SELECT", _userTwo, selectRequest));

            Assert.True(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");

            var selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);

            Assert.That(selectData.Count, Is.EqualTo(1));
            Assert.That(selectData.Any(data => data.Body.Contains("secondUser")));
        }

        [Test]
        public async Task Select_003_UnknownUser_SouldEmptyData()
        {
            var selectRequest = "{\"entry_type\":\"uploaded\",\"order_by\":[{\"sort\":\"date\",\"order\":\"desc\"}],\"limit\":20,\"offset\":0}";
            var response = await CloudFunction.Post(new Request("SELECT", "unknown", selectRequest));

            Assert.True(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");

            var selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);

            Assert.That(selectData.Count, Is.EqualTo(0));
        }
    }
}