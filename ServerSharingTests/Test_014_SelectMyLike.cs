using Newtonsoft.Json;
using NUnit.Framework;
using ServerSharing.Data;

namespace ServerSharingTests
{
    [TestFixture]
    public class Test_014_SelectMyLike
    {
        [SetUp]
        public async Task Setup()
        {
            await CloudFunction.Clear("records");
            await CloudFunction.Clear("downloads");
            await CloudFunction.Clear("likes");
        }

        [Test]
        public async Task SelectAll_WrongUser_ShouldReturnFalse()
        {
            var id1 = await CloudFunction.Upload("user1", new UploadData() { Data = new byte[] { 0 }, Image = new byte[] { 1 } });
            var id2 = await CloudFunction.Upload("user2", new UploadData() { Data = new byte[] { 2 }, Image = new byte[] { 3 } });

            await CloudFunction.Like("like1", id1);
            await CloudFunction.Like("like2", id2);

            var selectRequest = new SelectRequestBody()
            {
                EntryType = EntryType.All,
                OrderBy = new SelectRequestBody.SelectOrderBy[] { new SelectRequestBody.SelectOrderBy() { Order = Order.Desc, Sort = Sort.Date } },
                Limit = 10,
                Offset = 0
            };
            var response = await CloudFunction.Post(Request.Create("SELECT", "test_user", JsonConvert.SerializeObject(selectRequest)));

            Assert.That(response.IsSuccess, Is.True);

            var selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);

            Assert.That(selectData.First(data => data.Id == id1).MyLike, Is.EqualTo(false));
            Assert.That(selectData.First(data => data.Id == id2).MyLike, Is.EqualTo(false));
        }

        [Test]
        public async Task SelectAll_LikeAndNot_ShouldReturnTrueAndFalse()
        {
            var id1 = await CloudFunction.Upload("user1", new UploadData() { Data = new byte[] { 0 }, Image = new byte[] { 1 } });
            var id2 = await CloudFunction.Upload("user2", new UploadData() { Data = new byte[] { 2 }, Image = new byte[] { 3 } });

            await CloudFunction.Like("self_id", id1);

            var selectRequest = new SelectRequestBody()
            {
                EntryType = EntryType.All,
                OrderBy = new SelectRequestBody.SelectOrderBy[] { new SelectRequestBody.SelectOrderBy() { Order = Order.Desc, Sort = Sort.Date } },
                Limit = 10,
                Offset = 0
            };
            var response = await CloudFunction.Post(Request.Create("SELECT", "self_id", JsonConvert.SerializeObject(selectRequest)));

            Assert.That(response.IsSuccess, Is.True);

            var selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);

            Assert.That(selectData.First(data => data.Id == id1).MyLike, Is.EqualTo(true));
            Assert.That(selectData.First(data => data.Id == id2).MyLike, Is.EqualTo(false));
        }

        [Test]
        public async Task SelectUploaded_LikeAndNot_ShouldReturnTrueAndFalse()
        {
            var id1 = await CloudFunction.Upload("user1", new UploadData() { Data = new byte[] { 0 }, Image = new byte[] { 1 } });
            var id2 = await CloudFunction.Upload("user1", new UploadData() { Data = new byte[] { 2 }, Image = new byte[] { 3 } });

            await CloudFunction.Like("user1", id1);

            var selectRequest = new SelectRequestBody()
            {
                EntryType = EntryType.Uploaded,
                OrderBy = new SelectRequestBody.SelectOrderBy[] { new SelectRequestBody.SelectOrderBy() { Order = Order.Desc, Sort = Sort.Date } },
                Limit = 10,
                Offset = 0
            };
            var response = await CloudFunction.Post(Request.Create("SELECT", "user1", JsonConvert.SerializeObject(selectRequest)));

            Assert.That(response.IsSuccess, Is.True);

            var selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);

            Assert.That(selectData.First(data => data.Id == id1).MyLike, Is.EqualTo(true));
            Assert.That(selectData.First(data => data.Id == id2).MyLike, Is.EqualTo(false));
        }

        [Test]
        public async Task SelectLiked_FewRecords_ShouldBeAllTrue()
        {
            var id1 = await CloudFunction.Upload("user1", new UploadData() { Data = new byte[] { 0 }, Image = new byte[] { 1 } });
            var id2 = await CloudFunction.Upload("user2", new UploadData() { Data = new byte[] { 2 }, Image = new byte[] { 3 } });
            var id3 = await CloudFunction.Upload("user3", new UploadData() { Data = new byte[] { 2 }, Image = new byte[] { 3 } });

            await CloudFunction.Like("user1", id1);
            await CloudFunction.Like("user1", id2);
            await CloudFunction.Like("user1", id3);

            var selectRequest = new SelectRequestBody()
            {
                EntryType = EntryType.Liked,
                OrderBy = new SelectRequestBody.SelectOrderBy[] { new SelectRequestBody.SelectOrderBy() { Order = Order.Desc, Sort = Sort.Date } },
                Limit = 10,
                Offset = 0
            };
            var response = await CloudFunction.Post(Request.Create("SELECT", "user1", JsonConvert.SerializeObject(selectRequest)));

            Assert.That(response.IsSuccess, Is.True);

            var selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);

            foreach (var item in selectData)
                Assert.That(item.MyLike, Is.True);
        }

        [Test]
        public async Task SelectDownloaded_LikeAndNot_ShouldReturnTrueAndFalse()
        {
            var id1 = await CloudFunction.Upload("user1", new UploadData() { Data = new byte[] { 0 }, Image = new byte[] { 1 } });
            var id2 = await CloudFunction.Upload("user2", new UploadData() { Data = new byte[] { 2 }, Image = new byte[] { 3 } });

            await CloudFunction.Download("user1", id1);
            await CloudFunction.Download("user1", id2);
            await CloudFunction.Like("user1", id2);

            var selectRequest = new SelectRequestBody()
            {
                EntryType = EntryType.Downloaded,
                OrderBy = new SelectRequestBody.SelectOrderBy[] { new SelectRequestBody.SelectOrderBy() { Order = Order.Desc, Sort = Sort.Date } },
                Limit = 10,
                Offset = 0
            };
            var response = await CloudFunction.Post(Request.Create("SELECT", "user1", JsonConvert.SerializeObject(selectRequest)));

            Assert.That(response.IsSuccess, Is.True);

            var selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);

            Assert.That(selectData.First(data => data.Id == id1).MyLike, Is.EqualTo(false));
            Assert.That(selectData.First(data => data.Id == id2).MyLike, Is.EqualTo(true));
        }
    }
}