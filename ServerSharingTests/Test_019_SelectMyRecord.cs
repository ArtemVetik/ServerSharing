using Newtonsoft.Json;
using NUnit.Framework;
using ServerSharing.Data;

namespace ServerSharingTests
{
    [TestFixture]
    public class Test_019_SelectMyRecord
    {
        [SetUp]
        public async Task Setup()
        {
            await CloudFunction.Clear("records");
        }

        [Test]
        public async Task SelectAll_AllRecordsIsMine_ShouldReturnTrueForAll()
        {
            await CloudFunction.Upload("user", new UploadData() { Data = new byte[] { 1 }, Image = new byte[] { 2 } });
            await CloudFunction.Upload("user", new UploadData() { Data = new byte[] { 3 }, Image = new byte[] { 4 } });
            await CloudFunction.Upload("user", new UploadData() { Data = new byte[] { 5 }, Image = new byte[] { 6 } });

            var selectRequest = new SelectRequestBody()
            {
                EntryType = EntryType.All,
                OrderBy = new SelectRequestBody.SelectOrderBy[] { new SelectRequestBody.SelectOrderBy() { Order = Order.Desc, Sort = Sort.Date } },
                Limit = 10,
                Offset = 0
            };

            var response = await CloudFunction.Post(Request.Create("SELECT", "user", JsonConvert.SerializeObject(selectRequest)));

            Assert.That(response.IsSuccess, Is.True);

            var selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);

            Assert.That(selectData.Count(data => data.MyRecord == true), Is.EqualTo(3));
        }

        [Test]
        public async Task SelectAll_FewUsers_ShouldReturnTrueAndFalse()
        {
            var id1 = await CloudFunction.Upload("user1", new UploadData() { Data = new byte[] { 0 }, Image = new byte[] { 1 } });
            var id2 = await CloudFunction.Upload("user2", new UploadData() { Data = new byte[] { 0 }, Image = new byte[] { 1 } });

            var selectRequest = new SelectRequestBody()
            {
                EntryType = EntryType.All,
                OrderBy = new SelectRequestBody.SelectOrderBy[] { new SelectRequestBody.SelectOrderBy() { Order = Order.Desc, Sort = Sort.Date } },
                Limit = 10,
                Offset = 0
            };
            var response = await CloudFunction.Post(Request.Create("SELECT", "user1", JsonConvert.SerializeObject(selectRequest)));

            Assert.That(response.IsSuccess, Is.True);

            var selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);

            Assert.That(selectData.First(data => data.Id == id1).MyRecord, Is.EqualTo(true));
            Assert.That(selectData.First(data => data.Id == id2).MyRecord, Is.EqualTo(false));
        }

        [Test]
        public async Task SelectAll_NotUploaded_ShouldReturnFalseForAll()
        {
            var id1 = await CloudFunction.Upload("user1", new UploadData() { Data = new byte[] { 0 }, Image = new byte[] { 1 } });
            var id2 = await CloudFunction.Upload("user2", new UploadData() { Data = new byte[] { 0 }, Image = new byte[] { 1 } });

            var selectRequest = new SelectRequestBody()
            {
                EntryType = EntryType.All,
                OrderBy = new SelectRequestBody.SelectOrderBy[] { new SelectRequestBody.SelectOrderBy() { Order = Order.Desc, Sort = Sort.Date } },
                Limit = 10,
                Offset = 0
            };
            var response = await CloudFunction.Post(Request.Create("SELECT", "user3", JsonConvert.SerializeObject(selectRequest)));

            Assert.That(response.IsSuccess, Is.True);

            var selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);

            Assert.That(selectData.Count(data => data.MyRecord == true), Is.EqualTo(0));
        }
    }
}