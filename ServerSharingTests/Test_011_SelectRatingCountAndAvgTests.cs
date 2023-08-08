using Newtonsoft.Json;
using NUnit.Framework;
using ServerSharing.Data;

namespace ServerSharingTests
{
    [TestFixture]
    public class Test_011_SelectRatingCountAndAvgTests
    {
        private string _id1;
        private string _id2;

        [OneTimeSetUp]
        public async Task Setup()
        {
            await CloudFunction.Clear("records");
            await CloudFunction.Clear("rating");

            _id1 = await CloudFunction.Upload("user1", new UploadData() { Image = new byte[] { }, Data = new byte[] { } });
            _id2 = await CloudFunction.Upload("user2", new UploadData() { Image = new byte[] { }, Data = new byte[] { } });

            await CloudFunction.Rate("test_rate_1", _id1, 1);
            await CloudFunction.Rate("test_rate_2", _id2, 4);
            await CloudFunction.Rate("test_rate_3", _id1, 5);
        }

        [Test]
        public async Task Select_001_SelectUploadedId1_ShouldCorrectCountAndAvg()
        {
            var selectRequest = "{\"entry_type\":\"uploaded\",\"order_by\":[{\"sort\":\"date\",\"order\":\"desc\"}],\"limit\":20,\"offset\":0}";

            var response = await CloudFunction.Post(Request.Create("SELECT", "user1", selectRequest));
            Assert.True(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");
            var selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);

            Assert.That(selectData[0].RatingCount, Is.EqualTo(2));
            Assert.That(selectData[0].RatingAverage, Is.EqualTo(3f));

            response = await CloudFunction.Post(Request.Create("SELECT", "user2", selectRequest));
            Assert.True(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");
            selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);

            Assert.That(selectData[0].RatingCount, Is.EqualTo(1));
            Assert.That(selectData[0].RatingAverage, Is.EqualTo(4f));
        }

        [Test]
        public async Task Select_002_ChangeRateId1_ShouldChangeAvg()
        {
            await CloudFunction.Rate("test_rate_3", _id1, 3);

            var selectRequest = "{\"entry_type\":\"uploaded\",\"order_by\":[{\"sort\":\"date\",\"order\":\"desc\"}],\"limit\":20,\"offset\":0}";

            var response = await CloudFunction.Post(Request.Create("SELECT", "user1", selectRequest));
            Assert.True(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");
            
            var selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);

            Assert.That(selectData[0].RatingCount, Is.EqualTo(2));
            Assert.That(selectData[0].RatingAverage, Is.EqualTo(2f));
        }
    }
}