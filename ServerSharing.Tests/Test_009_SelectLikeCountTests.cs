using Newtonsoft.Json;
using NUnit.Framework;
using ServerSharing.Data;

namespace ServerSharing.Tests
{
    [TestFixture]
    public class Test_009_SelectLikeCountTests
    {
        private string _id1;
        private string _id2;

        [OneTimeSetUp]
        public async Task Setup()
        {
            await CloudFunction.Clear("records");
            await CloudFunction.Clear("likes");

            _id1 = await CloudFunction.Upload("user1", new UploadData() { Image = new byte[] { }, Data = new byte[] { } });
            _id2 = await CloudFunction.Upload("user2", new UploadData() { Image = new byte[] { }, Data = new byte[] { } });

            await CloudFunction.Like("test_like_1", _id1);
            await CloudFunction.Like("test_like_1", _id2);
            await CloudFunction.Like("test_like_2", _id1);
        }

        [Test]
        public async Task Select_001_SelectUploadedId1_ShouldReturnCorrectCount()
        {
            var selectRequest = "{\"entry_type\":\"uploaded\",\"order_by\":[{\"sort\":\"date\",\"order\":\"desc\"}],\"limit\":20,\"offset\":0}";

            var response = await CloudFunction.Post(Request.Create("SELECT", "user1", selectRequest));
            Assert.True(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");
            var selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);
            Assert.That(selectData[0].Likes, Is.EqualTo(2));

            response = await CloudFunction.Post(Request.Create("SELECT", "user2", selectRequest));
            Assert.True(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");
            selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);
            Assert.That(selectData[0].Likes, Is.EqualTo(1));
        }

        [Test]
        public async Task Select_002_SelectLikedId1_ShouldReturnCorrectCount()
        {
            var selectRequest = "{\"entry_type\":\"liked\",\"order_by\":[{\"sort\":\"date\",\"order\":\"desc\"}],\"limit\":20,\"offset\":0}";
            
            var response = await CloudFunction.Post(Request.Create("SELECT", "test_like_1", selectRequest));
            Assert.True(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");
            var selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);
            Assert.That(selectData.First(data => data.Id == _id1).Likes, Is.EqualTo(2));
            Assert.That(selectData.First(data => data.Id == _id2).Likes, Is.EqualTo(1));

            response = await CloudFunction.Post(Request.Create("SELECT", "test_like_2", selectRequest));
            Assert.True(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");
            selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);
            Assert.That(selectData[0].Likes, Is.EqualTo(2));
        }

        [Test]
        public async Task Select_003_TwiceLike_ShouldBeNotIncreaseLikeCount()
        {
            await CloudFunction.Like("test_like_1", _id1);
            await CloudFunction.Like("test_like_2", _id1);

            var selectRequest = "{\"entry_type\":\"liked\",\"order_by\":[{\"sort\":\"date\",\"order\":\"desc\"}],\"limit\":20,\"offset\":0}";
            var response = await CloudFunction.Post(Request.Create("SELECT", "test_like_1", selectRequest));

            Assert.True(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");

            var selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);

            Assert.That(selectData.First(data => data.Id == _id1).Likes, Is.EqualTo(2));
            Assert.That(selectData.First(data => data.Id == _id2).Likes, Is.EqualTo(1));
        }
    }
}