using Newtonsoft.Json;
using Ydb.Sdk.Client;
using Ydb.Sdk.Table;
using Ydb.Sdk.Value;

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
            var selectParameters = ParseSelectParameters(_body);

            return await _client.SessionExec(async session =>
            {
                var query = $@" DECLARE $limit AS Uint64;
                                DECLARE $offset AS Uint64;
                                {EntryTypes.Create(selectParameters.EntryType, _userId)} 
                                ORDER BY {CreateOrderBy(selectParameters.OrderBy)}
                                LIMIT $limit OFFSET $offset;";

                Console.WriteLine(query);

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
        }

        private SelectRequestBody ParseSelectParameters(string body)
        {
            try
            {
                var selectData = JsonConvert.DeserializeObject<SelectRequestBody>(body);

                if (Enum.IsDefined(typeof(EntryType), selectData.EntryType) == false)
                    throw new ArgumentException($"Request is missing {nameof(selectData.EntryType)} parameter");

                foreach (var orderBy in selectData.OrderBy)
                {
                    if (Enum.IsDefined(typeof(Sort), orderBy.Sort) == false)
                        throw new ArgumentException($"Request is missing {nameof(orderBy.Sort)} parameter");

                    if (Enum.IsDefined(typeof(Order), orderBy.Order) == false)
                        throw new ArgumentException($"Request is missing {nameof(orderBy.Order)} parameter");
                }

                return selectData;
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException("Request body has an invalid format", exception);
            }
        }

        private string CreateOrderBy(SelectRequestBody.SelectOrderBy[] orderBy)
        {
            var result = string.Empty;

            for (int i = 0; i < orderBy.Length; i++)
            {
                var sortColumn = orderBy[i].Sort switch
                {
                    Sort.Date => "records.date",
                    Sort.Downloads => "downloads.count",
                    Sort.Likes => "likes.count",
                    Sort.RaingCount => "ratings.count",
                    Sort.RaingAverage => "ratings.avg",
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