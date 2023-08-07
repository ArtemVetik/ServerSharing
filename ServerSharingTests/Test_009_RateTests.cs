using Newtonsoft.Json;
using ServerSharing;

namespace ServerSharingTests
{
    [TestFixture]
    public class Test_009_RateTests
    {
        [Test]
        public async Task Rate_001_CorrectId_ShouldLike()
        {
            var id = await CloudFunction.Upload("test_upload", "{}");

            var response = await CloudFunction.Post(new Request("RATE", "some_user", JsonConvert.SerializeObject(new RatingRequestBody()
            {
                Id = id,
                Rating = 5,
            })));

            Assert.True(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");
        }

        [Test]
        public async Task Rate_002_UnknownId_ShouldLike()
        {
            var response = await CloudFunction.Post(new Request("RATE", "some_user", JsonConvert.SerializeObject(new RatingRequestBody()
            {
                Id = "unknown",
                Rating = 5,
            })));
            
            Assert.True(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");
        }

        [Test]
        public async Task Rate_003_InvalidRating_ShouldBeNotSuccess()
        {
            var response = await CloudFunction.Post(new Request("RATE", "some_user", JsonConvert.SerializeObject(new RatingRequestBody()
            {
                Id = "unknown",
                Rating = 10,
            })));

            Assert.False(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");
        }
    }
}