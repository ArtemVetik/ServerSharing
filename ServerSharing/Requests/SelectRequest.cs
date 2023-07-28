using System.Text;
using System.Text.Json;
using Ydb.Sdk.Table;
using Ydb.Sdk.Value;

namespace ServerSharing
{
    public class SelectRequest : BaseRequest
    {
        public SelectRequest(TableClient tableClient, Request request)
            : base(tableClient, request)
        { }

        protected async override Task<Response> Handle(TableClient client, Request request)
        {
            var selectParameters = ParseSelectParameters(request.body, out string column);

            var response = await client.SessionExec(async session =>
            {
                var query = $@"
                        DECLARE $limit AS Uint64;
                        DECLARE $offset AS Uint64;

                        $downloads = (SELECT id, COUNT(id) as count
                        FROM `{Tables.Downloads}`
                        GROUP BY id
                        ORDER BY count);

                        $likes = (SELECT id, COUNT(id) as count
                        FROM `{Tables.Likes}`
                        GROUP BY id
                        ORDER BY count);

                        SELECT records.id, records.record, records.date, downloads.count, likes.count
                        FROM `{Tables.Records}` as records
                        LEFT JOIN $downloads as downloads ON downloads.id == records.id
                        LEFT JOIN $likes as likes ON likes.id == records.id" +
                        (selectParameters.Self ? $"\nWHERE records.user_id == \"{request.user_id}\"\n" : "\n") +
                        @$"GROUP BY records.id, records.record, records.date, downloads.count, likes.count
                        ORDER BY {column} {selectParameters.Order}
                        LIMIT $limit OFFSET $offset;
                ";

                return await session.ExecuteDataQuery(
                    query: query,
                    txControl: TxControl.BeginSerializableRW().Commit(),
                    parameters: new Dictionary<string, YdbValue>
                    {
                        { "$limit", YdbValue.MakeUint64(selectParameters.Limit) },
                        { "$offset", YdbValue.MakeUint64(selectParameters.Offset) }
                    }
                );
            });

            if (response.Status.IsSuccess == false)
                return new Response((uint)response.Status.StatusCode, response.Status.StatusCode.ToString(), string.Empty);

            var queryResponse = (ExecuteDataQueryResponse)response;
            var resultSet = queryResponse.Result.ResultSets[0];

            var responseData = new List<SelectResponseData>();

            foreach (var row in resultSet.Rows)
            {
                responseData.Add(new SelectResponseData()
                {
                    Id = Encoding.UTF8.GetString(row["records.id"].GetString()),
                    Record = Encoding.UTF8.GetString(row["records.record"].GetOptionalString()),
                    Datetime = row["records.date"].GetOptionalDatetime() ?? DateTime.MinValue,
                    Downloads = row["downloads.count"].GetOptionalUint64() ?? 0,
                    Likes = row["downloads.count"].GetOptionalUint64() ?? 0,
                });
            }

            return new Response((uint)response.Status.StatusCode, response.Status.StatusCode.ToString(), JsonSerializer.Serialize(responseData));
        }

        private SelectRequestParameters ParseSelectParameters(string body, out string column)
        {
            try
            {
                var selectData = JsonSerializer.Deserialize(body, typeof(SelectRequestParameters)) as SelectRequestParameters;

                column = selectData.Column switch
                {
                    "date" => "date",
                    "downloads" => "downloads.count",
                    "likes" => "likes.count",
                    _ => throw new ArgumentException("Request is missing column parameter")
                };

                if (selectData.Order != "desc" && selectData.Order != "asc")
                    throw new ArgumentException("Request is missing order parameter");

                return selectData;
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException("Request body has an invalid format", exception);
            }
        }
    }
}