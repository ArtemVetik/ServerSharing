using Newtonsoft.Json;
using NUnit.Framework;
using ServerSharing.Data;
using System.Text;

namespace ServerSharing.Tests
{
    [TestFixture]
    public class Test_002_SelectUploadedTests
    {
        private readonly string _userOne = "user_1";
        private readonly string _userTwo = "user_2";

        [OneTimeSetUp]
        public async Task Setup()
        {
            await CloudFunction.Clear("records");
            await CloudFunction.Upload(_userOne, new UploadData()
            {
                Metadata = new RecordMetadata() { Name = "name1", Description = "empty1" },
                Image = new byte[0],
                Data = Encoding.UTF8.GetBytes("user1_data"),
            });

            await CloudFunction.Upload(_userOne, new UploadData()
            {
                Metadata = new RecordMetadata() { Name = "name2", Description = "empty2" },
                Image = new byte[0],
                Data = Encoding.UTF8.GetBytes("user2_data"),
            });

            await CloudFunction.Upload(_userTwo, new UploadData()
            {
                Metadata = new RecordMetadata() { Name = "name3", Description = "empty3" },
                Image = new byte[0],
                Data = Encoding.UTF8.GetBytes("user3_data"),
            });
        }

        [Test]
        public async Task Select_001_UserOne_ShouldCorrectCount()
        {
            var selectRequest = "{\"entry_type\":\"uploaded\",\"order_by\":[{\"sort\":\"date\",\"order\":\"desc\"}],\"limit\":20,\"offset\":0}";
            var response = await CloudFunction.Post(Request.Create("SELECT", _userOne, selectRequest));

            Assert.True(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");

            var selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);

            Assert.That(selectData.Count, Is.EqualTo(2));
            Assert.That(selectData.Any(data => data.Metadata.Name == "name1" && data.Metadata.Description == "empty1"));
            Assert.That(selectData.Any(data => data.Metadata.Name == "name2" && data.Metadata.Description == "empty2"));
        }

        [Test]
        public async Task Select_002_UserTwo_ShouldCorrectCount()
        {
            var selectRequest = "{\"entry_type\":\"uploaded\",\"order_by\":[{\"sort\":\"date\",\"order\":\"desc\"}],\"limit\":20,\"offset\":0}";
            var response = await CloudFunction.Post(Request.Create("SELECT", _userTwo, selectRequest));

            Assert.True(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");

            var selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);

            Assert.That(selectData.Count, Is.EqualTo(1));
            Assert.That(selectData.Any(data => data.Metadata.Name == "name3" && data.Metadata.Description == "empty3"));
        }

        [Test]
        public async Task Select_003_UnknownUser_SouldEmptyData()
        {
            var selectRequest = "{\"entry_type\":\"uploaded\",\"order_by\":[{\"sort\":\"date\",\"order\":\"desc\"}],\"limit\":20,\"offset\":0}";
            var response = await CloudFunction.Post(Request.Create("SELECT", "unknown", selectRequest));

            Assert.True(response.IsSuccess, $"{response.StatusCode}, {response.ReasonPhrase}");

            var selectData = JsonConvert.DeserializeObject<List<SelectResponseData>>(response.Body);

            Assert.That(selectData.Count, Is.EqualTo(0));
        }
    }
}