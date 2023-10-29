using Newtonsoft.Json;
using NUnit.Framework;
using ServerSharing.Data;

namespace ServerSharing.Tests
{
    [TestFixture]
    public class Test_015_DislikeTests
    {
        [SetUp]
        public async Task Setup()
        {
            await CloudFunction.Clear("records");
            await CloudFunction.Clear("likes");
        }

        [Test]
        public async Task SelectLiked_AfterDislikeAll_ShouldReturnEmptyData()
        {
            var id = await CloudFunction.Upload("some_user", new UploadData() { Data = new byte[] { 0 }, Image = new byte[] { 1 } });
            await CloudFunction.Like("user1", id);
            await CloudFunction.Post(new Request() { method = "DISLIKE", user_id = "user1", body = id });

            var selectRequest = new SelectRequestBody()
            {
                EntryType = EntryType.Liked,
                OrderBy = new SelectRequestBody.SelectOrderBy[] { new SelectRequestBody.SelectOrderBy() { Order = Order.Desc, Sort = Sort.Date } },
                Limit = 20,
                Offset = 0
            };
            
            var response = await CloudFunction.Post(Request.Create("SELECT", "user1", JsonConvert.SerializeObject(selectRequest)));
            Assert.That(response.IsSuccess, Is.True);
            var selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);
            Assert.That(selectData.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task SelectLiked_DislikeOtherUserId_ShouldNotDeleteLike()
        {
            var id = await CloudFunction.Upload("some_user", new UploadData() { Data = new byte[] { 0 }, Image = new byte[] { 1 } });
            await CloudFunction.Like("user1", id);
            await CloudFunction.Post(new Request() { method = "DISLIKE", user_id = "user2", body = id });

            var selectRequest = new SelectRequestBody()
            {
                EntryType = EntryType.Liked,
                OrderBy = new SelectRequestBody.SelectOrderBy[] { new SelectRequestBody.SelectOrderBy() { Order = Order.Desc, Sort = Sort.Date } },
                Limit = 20,
                Offset = 0
            };

            var response = await CloudFunction.Post(Request.Create("SELECT", "user1", JsonConvert.SerializeObject(selectRequest)));
            Assert.That(response.IsSuccess, Is.True);
            var selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);
            Assert.That(selectData.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task SelectLiked_Dislike_ShouldDecreaseLikeCount()
        {
            var id = await CloudFunction.Upload("some_user", new UploadData() { Data = new byte[] { 0 }, Image = new byte[] { 1 } });
            await CloudFunction.Like("user1", id);

            await CloudFunction.Post(new Request() { method = "DISLIKE", user_id = "user1", body = id });
            await CloudFunction.Post(new Request() { method = "DISLIKE", user_id = "user1", body = id });

            var selectRequest = new SelectRequestBody()
            {
                EntryType = EntryType.All,
                OrderBy = new SelectRequestBody.SelectOrderBy[] { new SelectRequestBody.SelectOrderBy() { Order = Order.Desc, Sort = Sort.Date } },
                Limit = 20,
                Offset = 0
            };

            var response = await CloudFunction.Post(Request.Create("SELECT", "user1", JsonConvert.SerializeObject(selectRequest)));
            Assert.That(response.IsSuccess, Is.True);
            var selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);
            Assert.That(selectData.First(data => data.Id == id).Likes, Is.EqualTo(0));
        }
    }
}