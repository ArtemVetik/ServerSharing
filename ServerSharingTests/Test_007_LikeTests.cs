using ServerSharing;

namespace ServerSharingTests
{
    [TestFixture]
    public class Test_007_LikeTests
    {
        [Test]
        public async Task Like_001_CorrectId_ShouldLike()
        {
            var id = await CloudFunction.Upload("test_upload", new UploadData() { Image = new byte[] { }, Data = new byte[] { } });

            var response = await CloudFunction.Post(new Request("LIKE", "some_user", id));
            Assert.True(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");
        }

        [Test]
        public async Task Like_002_UnknownId_ShouldLike()
        {
            var response = await CloudFunction.Post(new Request("LIKE", "some_user", "unknown_id"));
            Assert.True(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");
        }
    }
}