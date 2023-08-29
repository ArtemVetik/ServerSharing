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
                    EntryType.All => All(userId),
                    EntryType.Downloaded => Downloaded(userId),
                    EntryType.Uploaded => Uploaded(userId),
                    EntryType.Liked => Liked(userId),
                    _ => throw new NotSupportedException()
                };
            }

            private static string All(string userId)
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

                    SELECT records.id, records.body, records.date, downloads.count, likes.count, ratings.count, ratings.avg, if (myLikes.id is null, false, true) as myLike
                    FROM `{Tables.Records}` as records
                    LEFT JOIN $downloads as downloads on downloads.id == records.id
                    LEFT JOIN $likes as likes on likes.id == records.id
                    LEFT JOIN $rating as ratings on ratings.id == records.id
                    LEFT JOIN $myLikes as myLikes on myLikes.id == records.id
                ";
            }

            private static string Downloaded(string userId)
            {
                return $@"
                    $myDownloads = (SELECT downloads.user_id, downloads.id
                    FROM `{Tables.Downloads}` downloads
                    where downloads.user_id = ""{userId}""
                    GROUP BY downloads.user_id, downloads.id);

                    $downloads = (SELECT downloads.id, COUNT(downloads.id) as count
                    FROM `{Tables.Downloads}` downloads
                    GROUP BY downloads.id);

                    $likes = (SELECT likes.id, COUNT(likes.id) as count
                    FROM `{Tables.Likes}` likes
                    GROUP BY likes.id);

                    $myLikes = (SELECT *
                    FROM `{Tables.Likes}` likes
                    where likes.user_id == ""{userId}"");

                    $rating = (SELECT ratings.id, COUNT(ratings.id) as count, AVG(ratings.rating) as avg
                    FROM `{Tables.Ratings}` ratings
                    GROUP BY ratings.id);

                    SELECT records.id, records.body, records.date, downloads.count, likes.count, ratings.count, ratings.avg, if (myLikes.id is null, false, true) as myLike
                    FROM `{Tables.Records}` as records
                    INNER JOIN $myDownloads as myDownloads on myDownloads.id == records.id
                    LEFT JOIN $downloads as downloads on downloads.id == records.id
                    LEFT JOIN $likes as likes on likes.id == records.id
                    LEFT JOIN $myLikes as myLikes on myLikes.id == records.id
                    LEFT JOIN $rating as ratings on ratings.id == records.id
                ";
            }

            private static string Uploaded(string userId)
            {
                return $@"
                    $downloads = (SELECT downloads.id, COUNT(downloads.id) as count
                    FROM `{Tables.Downloads}` downloads
                    GROUP BY downloads.id);

                    $likes = (SELECT likes.id, COUNT(likes.id) as count
                    FROM `{Tables.Likes}` likes
                    GROUP BY likes.id);

                    $rating = (SELECT ratings.id, COUNT(ratings.id) as count, AVG(ratings.rating) as avg
                    FROM `{Tables.Ratings}` ratings
                    GROUP BY ratings.id);

                    $myLikes = (SELECT *
                    FROM `{Tables.Likes}` likes
                    where likes.user_id == ""{userId}"");

                    SELECT records.id, records.body, records.date, downloads.count, likes.count, ratings.count, ratings.avg, if (myLikes.id is null, false, true) as myLike
                    FROM `{Tables.Records}` as records
                    LEFT JOIN $downloads as downloads on downloads.id == records.id
                    LEFT JOIN $likes as likes on likes.id == records.id
                    LEFT JOIN $rating as ratings on ratings.id == records.id
                    LEFT JOIN $myLikes as myLikes on myLikes.id == records.id
                    WHERE records.user_id == ""{userId}""
                ";
            }

            private static string Liked(string userId)
            {
                return $@"
                    $downloads = (SELECT downloads.id, COUNT(downloads.id) as count
                    FROM `{Tables.Downloads}` downloads
                    GROUP BY downloads.id);

                    $myLikes = (SELECT likes.user_id, likes.id
                    FROM `{Tables.Likes}` likes
                    where likes.user_id == ""{userId}""
                    GROUP BY likes.user_id, likes.id);

                    $likes = (SELECT likes.id, COUNT(likes.id) as count
                    FROM `{Tables.Likes}` likes
                    GROUP BY likes.id);

                    $rating = (SELECT ratings.id, COUNT(ratings.id) as count, AVG(ratings.rating) as avg
                    FROM `{Tables.Ratings}` ratings
                    GROUP BY ratings.id);

                    SELECT records.id, records.body, records.date, downloads.count, likes.count, ratings.count, ratings.avg, true as myLike
                    FROM `{Tables.Records}` as records
                    INNER JOIN $myLikes as myLikes on myLikes.id == records.id
                    LEFT JOIN $likes as likes on likes.id == records.id
                    LEFT JOIN $downloads as downloads on downloads.id == records.id
                    LEFT JOIN $rating as ratings on ratings.id == records.id
                ";
            }
        }
    }
}