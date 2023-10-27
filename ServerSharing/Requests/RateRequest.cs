using Ydb.Sdk.Table;
using System.Text;
using Ydb.Sdk.Value;
using Newtonsoft.Json;
using ServerSharing.Data;

namespace ServerSharing
{
    public class RateRequest : BaseRequest
    {
        public RateRequest(TableClient tableClient, Request request)
            : base(tableClient, request)
        { }

        protected async override Task<Response> Handle(TableClient client, Request request)
        {
            RatingRequestBody body = default;

            try
            {
                body = JsonConvert.DeserializeObject<RatingRequestBody>(request.body);

                if (body.Rating < 1 || body.Rating > 5)
                    throw new ArgumentOutOfRangeException("The rating has an incorrect value: " + body.Rating);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException("Request body has an invalid format", exception);
            }

            var response = await client.SessionExec(async session =>
            {
                var query = $@"
                    DECLARE $user_id AS string;
                    DECLARE $id AS string;
                    DECLARE $rating AS int8;

                    $avg_rating = (SELECT CAST(AVG(ratings.rating) as float?)
                    from `{Tables.Ratings}` VIEW idx_id ratings
                    where ratings.id = $id);

                    INSERT INTO `{Tables.Ratings}` (user_id, id, rating)
                    VALUES ($user_id, $id, $rating);

                    UPDATE `{Tables.Ratings}`
                    SET rating_count = if (rating_count is null, 1u, rating_count + 1u)
                    WHERE id = $id;

                    UPDATE `{Tables.Records}`
                    SET rating_avg = if (rating_avg is null, 3, $avg_rating)
                    WHERE id = $id;
                ";

                return await session.ExecuteDataQuery(
                    query: query,
                    txControl: TxControl.BeginSerializableRW().Commit(),
                    parameters: new Dictionary<string, YdbValue>
                    {
                        { "$user_id", YdbValue.MakeString(Encoding.UTF8.GetBytes(request.user_id)) },
                        { "$id", YdbValue.MakeString(Encoding.UTF8.GetBytes(body.Id)) },
                        { "$rating", YdbValue.MakeInt8(body.Rating) },
                    }
                );
            });

            return new Response((uint)response.Status.StatusCode, response.Status.StatusCode.ToString(), string.Empty);
        }
    }
}