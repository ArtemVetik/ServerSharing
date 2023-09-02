using Newtonsoft.Json;
using NUnit.Framework;
using ServerSharing.Data;

namespace ServerSharingTests
{
    [TestFixture]
    public class Test_018_SelectMyDownload
    {
        [SetUp]
        public async Task Setup()
        {
            await CloudFunction.Clear("records");
            await CloudFunction.Clear("downloads");
        }

        [Test]
        public async Task SelectAll_NotDownload_ShouldReturnFalse()
        {
            await CloudFunction.Upload("user", new UploadData() { Data = new byte[] { 0 }, Image = new byte[] { 1 } });

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

            Assert.That(selectData[0].MyDownload, Is.EqualTo(false));
        }

        [Test]
        public async Task SelectAll_Download_ShouldReturnTrue()
        {
            var id = await CloudFunction.Upload("user", new UploadData() { Data = new byte[] { 0 }, Image = new byte[] { 1 } });

            await CloudFunction.Download("test_user", id);

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

            Assert.That(selectData[0].MyDownload, Is.EqualTo(true));
        }
    }
}