using Newtonsoft.Json;
using Ydb.Sdk.Table;
using ServerSharing.Data;
using Ydb.Sdk.Value;
using System.Text;

namespace ServerSharing
{
    public class InfoRequest : BaseRequest
    {
        public InfoRequest(TableClient tableClient, Request request)
            : base(tableClient, request)
        { }

        protected async override Task<Response> Handle(TableClient client, Request request)
        {
            var response = await client.SessionExec(async session =>
            {
                var query = $@"
                    DECLARE $id AS String;
                    DECLARE $user_id AS String;

                    $data = (SELECT records.*, if (records.user_id = $user_id, true, false) AS my_record
                            FROM `{Tables.Records}` AS records
                            WHERE records.id = $id);
                                
                    $downloads = (SELECT id, true AS my_download
                    FROM `{Tables.Downloads}`
                    WHERE user_id = $user_id);

                    $likes = (SELECT id, true AS my_like
                    FROM `{Tables.Likes}`
                    WHERE user_id = $user_id);

                    $rating = (SELECT id, rating AS my_rating
                    FROM `{Tables.Ratings}`
                    WHERE user_id = $user_id);

                    SELECT *
                    FROM $data AS data
                    LEFT JOIN $downloads AS downloads ON downloads.id = data.id
                    LEFT JOIN $likes AS likes ON likes.id = data.id
                    LEFT JOIN $rating AS rating ON rating.id = data.id;
                ";

                return await session.ExecuteDataQuery(
                    query: query,
                    txControl: TxControl.BeginSerializableRW().Commit(),
                    parameters: new Dictionary<string, YdbValue>
                    {
                        { "$id", YdbValue.MakeString(Encoding.UTF8.GetBytes(request.body)) },
                        { "$user_id", YdbValue.MakeString(Encoding.UTF8.GetBytes(request.user_id)) },
                    }
                );
            });

            if (response.Status.IsSuccess == false)
                return new Response((uint)response.Status.StatusCode, response.Status.StatusCode.ToString(), string.Empty);

            var queryResponse = (ExecuteDataQueryResponse)response;
            var resultSet = queryResponse.Result.ResultSets[0];

            if (resultSet.Rows.Count == 0)
                throw new InvalidOperationException("Record not found!");

            var responseData = resultSet.Rows[0].CreateResponseData();

            return new Response((uint)response.Status.StatusCode, response.Status.StatusCode.ToString(), JsonConvert.SerializeObject(responseData));
        }
    }
}