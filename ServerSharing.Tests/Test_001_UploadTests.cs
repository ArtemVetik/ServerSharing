using Newtonsoft.Json;
using NUnit.Framework;
using ServerSharing.Data;
using System.Text;

namespace ServerSharing.Tests
{
    [TestFixture]
    public class Test_001_UploadTests
    {
        private readonly string _userName = "test_user";

        private UploadData _uploadData;

        [OneTimeSetUp]
        public async Task ClearRecords()
        {
            await CloudFunction.Clear("records");

            _uploadData = new UploadData()
            {
                Metadata = new RecordMetadata()
                {
                    Name = "Joe",
                    Description = "My description",
                },
                Image = new byte[] { },
                Data = Encoding.UTF8.GetBytes("some_data"),
            };
        }

        [Test]
        public async Task Upload_001_CorrectJson_ShouldSuccess()
        {
            var response = await CloudFunction.Post(Request.Create("UPLOAD", _userName, JsonConvert.SerializeObject(_uploadData)));

            Assert.True(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");

            var records = await SelectAll();
            Assert.That(records.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task Upload_002_EmptyJson_ShouldError()
        {
            var response = await CloudFunction.Post(Request.Create("UPLOAD", _userName, "{}"));
            Assert.False(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");
        }

        [Test]
        public async Task Upload_003_WrongJsonRecord_ShouldNotSuccess()
        {
            var response = await CloudFunction.Post(Request.Create("UPLOAD", _userName, "abracadabra"));
            Assert.False(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");

            var records = await SelectAll();
            Assert.That(records.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task Upload_004_SameRecord_IdMustBeDifferent()
        {
            var response = await CloudFunction.Post(Request.Create("UPLOAD", _userName, JsonConvert.SerializeObject(_uploadData)));

            Assert.True(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");

            var records = await SelectAll();

            Assert.That(records.Count, Is.EqualTo(2));
            Assert.That(records[0].Id, Is.Not.EqualTo(records[1].Id));
        }

        [Test]
        public async Task Upload_005_OtherUserSameRecord_AllIdMustBeDifferent()
        {
            var response = await CloudFunction.Post(Request.Create("UPLOAD", _userName + "_2", JsonConvert.SerializeObject(_uploadData)));

            Assert.True(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");

            var records = await SelectAll();

            Assert.That(records.Count, Is.EqualTo(3));

            for (int i = 0; i < records.Count; i++)
                for (int j = i + 1; j < records.Count; j++)
                    Assert.That(records[i].Id, Is.Not.EqualTo(records[j].Id));
        }

        private async Task<List<SelectResponseData>> SelectAll()
        {
            var response = await CloudFunction.Post(Request.Create("SELECT", _userName, JsonConvert.SerializeObject(new SelectRequestBody()
            {
                EntryType = EntryType.All,
                OrderBy = new SelectRequestBody.SelectOrderBy[] { new SelectRequestBody.SelectOrderBy() { Sort = Sort.Date, Order = Order.Desc } },
                Limit = 100,
                Offset = 0,
            })));

            if (response.IsSuccess == false)
                throw new InvalidOperationException($"Select error: {response}");

            return JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);
        }
    }
}