using ServerSharing.Data;

namespace ServerSharing
{
    internal partial class SelectQueryFactory
    {
        private static class EntryTypes
        {
            internal static string Create(EntryType type, string userId)
            {
                return type switch
                {
                    EntryType.All => All(),
                    EntryType.Downloaded => Downloaded(userId),
                    EntryType.Uploaded => Uploaded(userId),
                    EntryType.Liked => Liked(userId),
                    _ => throw new NotSupportedException()
                };
            }

            private static string All()
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

                    $rating = (SELECT ratings.id, COUNT(ratings.id) as count, AVG(ratings.rating) as avg
                    FROM `{Tables.Ratings}` ratings
                    GROUP BY ratings.id);

                    SELECT records.id, records.body, records.date, downloads.count, likes.count, ratings.count, ratings.avg
                    FROM `{Tables.Records}` as records
                    LEFT JOIN $downloads as downloads ON downloads.id == records.id
                    LEFT JOIN $likes as likes ON likes.id == records.id
                    LEFT JOIN $rating as ratings ON ratings.id == records.id
                ";
            }

            private static string Downloaded(string userId)
            {
                return $@"
                    $downloads = (SELECT downloads.user_id, downloads.id, COUNT(downloads.id) as count
                    FROM `{Tables.Downloads}` downloads
                    where downloads.user_id = ""{userId}""
                    GROUP BY downloads.user_id, downloads.id);

                    $likes = (SELECT likes.user_id, likes.id, COUNT(likes.id) as count
                    FROM `{Tables.Likes}` likes
                    GROUP BY likes.user_id, likes.id);

                    $rating = (SELECT ratings.id, COUNT(ratings.id) as count, AVG(ratings.rating) as avg
                    FROM `{Tables.Ratings}` ratings
                    GROUP BY ratings.id);

                    SELECT records.id, records.body, records.date, downloads.count, likes.count, ratings.count, ratings.avg
                    FROM `{Tables.Records}` as records
                    INNER JOIN $downloads as downloads ON downloads.id == records.id
                    LEFT JOIN $likes as likes ON likes.id == records.id
                    LEFT JOIN $rating as ratings ON ratings.id == records.id
                ";
            }

            private static string Uploaded(string userId)
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

                    $rating = (SELECT ratings.id, COUNT(ratings.id) as count, AVG(ratings.rating) as avg
                    FROM `{Tables.Ratings}` ratings
                    GROUP BY ratings.id);

                    SELECT records.id, records.body, records.date, downloads.count, likes.count, ratings.count, ratings.avg
                    FROM `{Tables.Records}` as records
                    LEFT JOIN $downloads as downloads ON downloads.id == records.id
                    LEFT JOIN $likes as likes ON likes.id == records.id
                    LEFT JOIN $rating as ratings ON ratings.id == records.id
                    WHERE records.user_id == ""{userId}""
                ";
            }

            private static string Liked(string userId)
            {
                return $@"
                    $downloads = (SELECT downloads.user_id, downloads.id, COUNT(downloads.id) as count
                    FROM `{Tables.Downloads}` downloads
                    GROUP BY downloads.user_id, downloads.id);

                    $likes = (SELECT likes.user_id, likes.id, COUNT(likes.id) as count
                    FROM `{Tables.Likes}` likes
                    where likes.user_id = ""{userId}""
                    GROUP BY likes.user_id, likes.id);

                    $rating = (SELECT ratings.id, COUNT(ratings.id) as count, AVG(ratings.rating) as avg
                    FROM `{Tables.Ratings}` ratings
                    GROUP BY ratings.id);

                    SELECT records.id, records.body, records.date, downloads.count, likes.count, ratings.count, ratings.avg
                    FROM `{Tables.Records}` as records
                    INNER JOIN $likes as likes ON likes.id == records.id
                    LEFT JOIN $downloads as downloads ON downloads.id == records.id
                    LEFT JOIN $rating as ratings ON ratings.id == records.id
                ";
            }
        }
    }
}