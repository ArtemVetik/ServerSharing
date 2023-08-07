using Newtonsoft.Json;
using ServerSharing;

namespace ServerSharingTests
{
    [TestFixture]
    public class Test_001_UploadTests
    {
        private readonly string _userName = "test_user";

        [OneTimeSetUp]
        public async Task ClearRecords()
        {
            await CloudFunction.Clear("records");
        }

        [Test, Order(1)]
        public async Task Upload_001_EmptyRecord_ShouldSuccess()
        {
            var response = await CloudFunction.Post(new Request("UPLOAD", _userName, "{}"));
            Assert.True(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");

            var records = await SelectAll();
            Assert.That(records.Count, Is.EqualTo(1));
        }

        [Test, Order(2)]
        public async Task Upload_002_WrongJsonRecord_ShouldNotSuccess()
        {
            var response = await CloudFunction.Post(new Request("UPLOAD", _userName, "abracadabra"));
            Assert.False(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");

            var records = await SelectAll();
            Assert.That(records.Count, Is.EqualTo(1));
        }

        [Test, Order(3)]
        public async Task Upload_003_SameRecord_IdMustBeDifferent()
        {
            var response = await CloudFunction.Post(new Request("UPLOAD", _userName, "{}"));
            Assert.True(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");

            var records = await SelectAll();

            Assert.That(records.Count, Is.EqualTo(2));
            Assert.That(records.Count, Is.EqualTo(records.Distinct().Count()));
        }

        [Test, Order(4)]
        public async Task Upload_004_OtherUserSameRecord_AllIdMustBeDifferent()
        {
            var response = await CloudFunction.Post(new Request("UPLOAD", _userName + "_2", "{}"));
            Assert.True(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");

            var records = await SelectAll();

            Assert.That(records.Count, Is.EqualTo(3));
            Assert.That(records.Count, Is.EqualTo(records.Distinct().Count()));
        }

        [Test, Order(5)]
        public async Task Upload_005_ComplexJsonRecord_ShouldCorrectAdd()
        {
            var jsonString = "{\"entry_type\":1,\"order_by\":[{\"sort\":4,\"order\":0},{\"sort\":3,\"order\":0}],\"limit\":20,\"offset\":0}";
            var response = await CloudFunction.Post(new Request("UPLOAD", _userName + "_2", jsonString));
            Assert.True(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");

            var records = await SelectAll();

            Assert.That(records.Count, Is.EqualTo(4));
            Assert.That(records.Count, Is.EqualTo(records.Distinct().Count()));
        }

        private async Task<List<SelectResponseData>> SelectAll()
        {
            var response = await CloudFunction.Post(new Request("SELECT", _userName, JsonConvert.SerializeObject(new SelectRequestBody()
            {
                EntryType = EntryType.All,
                OrderBy = new SelectRequestBody.SelectOrderBy[] { new SelectRequestBody.SelectOrderBy() { Sort = Sort.Date, Order = Order.Desc} },
                Limit = 100,
                Offset = 0,
            })));

            if (response.IsSuccess == false)
                throw new InvalidOperationException($"Select error: {response}");

            return JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);
        }
    }
}