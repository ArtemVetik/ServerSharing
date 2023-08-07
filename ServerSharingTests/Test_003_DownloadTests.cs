using ServerSharing;

namespace ServerSharingTests
{
    [TestFixture]
    public class Test_003_DownloadTests
    {
        private string _userOne1;
        private string _userTwo1;
        private string _userTwo2;
        private string _userThree1;

        [OneTimeSetUp]
        public async Task Setup()
        {
            await CloudFunction.Clear("records");
            await CloudFunction.Clear("downloads");

            _userOne1 = await CloudFunction.Upload("userOne", "{\"data\":123}");
            _userTwo1 = await CloudFunction.Upload("userTwo", "{\"name\":\"Joe\"}");
            _userTwo2 = await CloudFunction.Upload("userTwo", "{}");
            _userThree1 = await CloudFunction.Upload("userThree", "{\"record_id\": 1234567890}");
        }

        [Test]
        public async Task Download_001_FewUsers_SouldCorrectData()
        {
            var response = await CloudFunction.Post(new Request("DOWNLOAD", "test_download_1", _userOne1));

            Assert.True(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");
            Assert.That(response.Body == "{\"data\":123}", $"Actual body: {response.Body}");

            response = await CloudFunction.Post(new Request("DOWNLOAD", "test_download_2", _userTwo2));

            Assert.True(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");
            Assert.That(response.Body == "{}", $"Actual body: {response.Body}");
        }

        [Test]
        public async Task Download_002_TwiceDownload_SouldReturnSameData()
        {
            var response = await CloudFunction.Post(new Request("DOWNLOAD", "test_download_3", _userThree1));

            Assert.True(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");
            Assert.That(response.Body == "{\"record_id\": 1234567890}", $"Actual body: {response.Body}");

            response = await CloudFunction.Post(new Request("DOWNLOAD", "test_download_3", _userThree1));

            Assert.True(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");
            Assert.That(response.Body == "{\"record_id\": 1234567890}", $"Actual body: {response.Body}");
        }

        [Test]
        public async Task Download_003_InvalidId_SouldBeNotSuccess()
        {
            var response = await CloudFunction.Post(new Request("DOWNLOAD", "test_download_4", "invalid_id"));

            Assert.False(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");
        }
    }
}