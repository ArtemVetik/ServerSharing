﻿using Newtonsoft.Json;
using NUnit.Framework;
using ServerSharing.Data;

namespace ServerSharingTests
{
    [TestFixture]
    public class Test_020_SelectMetaData
    {
        [SetUp]
        public async Task Setup()
        {
            await CloudFunction.Clear("records");
        }

        [Test]
        public async Task SelectAll_NotEmptyMeta_ShouldReturnMeta()
        {
            var metadata = new RecordMetadata()
            {
                Name = "Admin",
                Description = "My Description",
                CompatibilityVersion = "1.0.3",
            };

            await CloudFunction.Upload("user1", new UploadData() { Metadata = metadata, Data = new byte[] { 1 }, Image = new byte[] { 2 }, });

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

            Assert.That(selectData.First().Metadata.Name, Is.EqualTo(metadata.Name));
            Assert.That(selectData.First().Metadata.Description, Is.EqualTo(metadata.Description));
            Assert.That(selectData.First().Metadata.CompatibilityVersion, Is.EqualTo(metadata.CompatibilityVersion));
        }

        [Test]
        public async Task SelectAll_EmptyMeta_ShouldReturnEmptyMeta()
        {
            await CloudFunction.Upload("user1", new UploadData() { Data = new byte[] { 1 }, Image = new byte[] { 2 }, });

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

            Assert.That(selectData.First().Metadata, Is.EqualTo(null));
        }
    }
}