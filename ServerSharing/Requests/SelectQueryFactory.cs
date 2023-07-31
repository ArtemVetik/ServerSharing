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
                var sortColumn = selectParameters.Sort switch
                {
                    Sort.Date => "records.date",
                    Sort.Downloads => "downloads.count",
                    Sort.Likes => "likes.count",
                    _ => throw new NotImplementedException()
                };

                var query = $@"  DECLARE $limit AS Uint64;
                                DECLARE $offset AS Uint64;
                                {EntryTypes.Create(selectParameters.EntryType, _userId)} 
                                ORDER BY {sortColumn} {selectParameters.Order}
                                LIMIT $limit OFFSET $offset;";

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

        private SelectRequestParameters ParseSelectParameters(string body)
        {
            try
            {
                var selectData = JsonConvert.DeserializeObject<SelectRequestParameters>(body);

                if (Enum.IsDefined(typeof(EntryType), selectData.EntryType) == false)
                    throw new ArgumentException($"Request is missing {nameof(selectData.EntryType)} parameter");

                if (Enum.IsDefined(typeof(Sort), selectData.Sort) == false)
                    throw new ArgumentException($"Request is missing {nameof(selectData.Sort)} parameter");

                if (Enum.IsDefined(typeof(Order), selectData.Order) == false)
                    throw new ArgumentException($"Request is missing {nameof(selectData.Order)} parameter");

                return selectData;
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException("Request body has an invalid format", exception);
            }
        }
    }
}