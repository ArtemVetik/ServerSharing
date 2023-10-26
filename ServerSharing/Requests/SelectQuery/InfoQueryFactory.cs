using Ydb.Sdk.Client;
using Ydb.Sdk.Table;

namespace ServerSharing
{
    internal partial class InfoQueryFactory
    {
        private readonly TableClient _client;
        private readonly string _userId;
        private readonly string _body;

        public InfoQueryFactory(TableClient client, string userId, string body)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _userId = userId ?? throw new ArgumentNullException(nameof(userId));
            _body = body ?? throw new ArgumentNullException(nameof(body));
        }

        public async Task<IResponse> Create()
        {
            return await _client.SessionExec(async session =>
            {
                var query = InfoQuery(_body, _userId);

                return await session.ExecuteDataQuery(
                    query: query,
                    txControl: TxControl.BeginSerializableRW().Commit()
                );
            });
        }

        private static string InfoQuery(string id, string userId)
        {
            return $@"
                    $downloads = (SELECT downloads.id, COUNT(downloads.id) as count
                    FROM `{Tables.Downloads}` downloads
                    GROUP BY downloads.id
                    ORDER BY count);

                    $likes = (SELECT likes.id, COUNT(likes.id) as count
                    FROM `{Tables.Likes}` likes
                    GROUP BY likes.id
                    ORDER BY count);

                    $myLikes = (SELECT *
                    FROM `{Tables.Likes}` likes
                    WHERE likes.user_id == ""{userId}"");

                    $rating = (SELECT ratings.id, COUNT(ratings.id) as count, AVG(ratings.rating) as avg
                    FROM `{Tables.Ratings}` ratings
                    GROUP BY ratings.id);
                    
                    $myRating = (SELECT *
                    FROM `{Tables.Ratings}` ratings
                    WHERE ratings.user_id == ""{userId}"");

                    $myDownloads = (SELECT *
                    FROM `{Tables.Downloads}` downloads
                    WHERE downloads.user_id == ""{userId}"");

                    SELECT records.id, records.body, records.date, downloads.count, likes.count, ratings.count, ratings.avg,
                        if (myLikes.id is null, false, true) as myLike,
                        if (myRating.id is null, null, myRating.rating) as myRating,
                        if (myDownloads.id is null, false, true) as myDownload,
                        if (records.user_id == ""{userId}"", true, false) as myRecord
                    FROM `{Tables.Records}` as records
                    LEFT JOIN $downloads as downloads on downloads.id == records.id
                    LEFT JOIN $likes as likes on likes.id == records.id
                    LEFT JOIN $rating as ratings on ratings.id == records.id
                    LEFT JOIN $myLikes as myLikes on myLikes.id == records.id
                    LEFT JOIN $myRating as myRating on myRating.id == records.id
                    LEFT JOIN $myDownloads as myDownloads on myDownloads.id == records.id
                    WHERE records.id == ""{id}""
                ";
        }
    }
}