using Ydb.Sdk.Client;
using Ydb.Sdk.Table;
using Ydb.Sdk.Value;
using ServerSharing.Data;
using System.Text;

namespace ServerSharing
{
    internal partial class SelectQueryFactory
    {
        private readonly TableClient _client;
        private readonly string _userId;
        private readonly string _body;

        public SelectQueryFactory(TableClient client, string userId, string body)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _userId = userId ?? throw new ArgumentNullException(nameof(userId));
            _body = body ?? throw new ArgumentNullException(nameof(body));
        }

        public async Task<IResponse> CreateQuery()
        {
            var selectParameters = SelectExtentions.ParseSelectRequestBody(_body);

            return await _client.SessionExec(async session =>
            {
                var query = $@" DECLARE $user_id AS String;
                                DECLARE $limit AS Uint64;
                                DECLARE $offset AS Uint64;

                                $data = ({SelectQueryContainer.Create(selectParameters.EntryType)} 
                                ORDER BY {CreateOrderBy(selectParameters.OrderBy)}
                                LIMIT $limit OFFSET $offset);
                                
                                $downloads = (SELECT id, true AS my_download
                                FROM `{Tables.Downloads}`
                                WHERE user_id = $user_id);

                                $likes = (SELECT id, true AS my_like
                                FROM `{Tables.Likes}`
                                WHERE user_id = $user_id);

                                $rating = (SELECT id, rating as my_rating
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
                        { "$user_id", YdbValue.MakeString(Encoding.UTF8.GetBytes(_userId)) },
                        { "$limit", YdbValue.MakeUint64(selectParameters.Limit) },
                        { "$offset", YdbValue.MakeUint64(selectParameters.Offset) }
                    }
                );
            });
        }

        private static string CreateOrderBy(SelectRequestBody.SelectOrderBy[] orderBy)
        {
            var result = string.Empty;

            for (int i = 0; i < orderBy.Length; i++)
            {
                var sortColumn = orderBy[i].Sort switch
                {
                    Sort.Date => "date",
                    Sort.Downloads => "download_count",
                    Sort.Likes => "like_count",
                    Sort.RaingCount => "rating_count",
                    Sort.RaingAverage => "rating_avg",
                    _ => throw new NotImplementedException()
                };

                result += $"{sortColumn} {orderBy[i].Order}";

                if (i < orderBy.Length - 1)
                    result += ", ";
            }

            return result;
        }
    }
}