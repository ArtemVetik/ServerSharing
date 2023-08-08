using ServerSharing.Data;
using System.Text;

namespace ServerSharingTests
{
    [TestFixture]
    public class Test_004_LoadImageTests
    {
        [OneTimeSetUp]
        public async Task Setup()
        {
            await CloudFunction.Clear("records");
        }

        [Test]
        public async Task LoadImage_CorrectId_ShouldLoad()
        {
            var id = await CloudFunction.Upload("userOne", new UploadData() { Image = new byte[] { 255, 0, 255 }, Data = new byte[] { 0, 0, 0 } });

            var response = await CloudFunction.Post(new Request("LOAD_IMAGE", "test_load_user", id));

            Assert.True(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");

            var image = Convert.FromBase64String(response.Body);
            Assert.True(Enumerable.SequenceEqual(image, new byte[] { 255, 0, 255 }), $"Expected: [255, 0, 255], But was: {string.Join(", ", image)}");
        }
    }
}