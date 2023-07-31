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
                    _ => throw new NotSupportedException()
                };
            }

            private static string All()
            {
                return $@"
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
                    LEFT JOIN $likes as likes ON likes.id == records.id
                    GROUP BY records.id, records.record, records.date, downloads.count, likes.count
                ";
            }

            private static string Downloaded(string userId)
            {
                return $@"
                    $downloads = (SELECT user_id, id, COUNT(id) as count
                    FROM `{Tables.Downloads}`
                    where user_id = ""{userId}""
                    GROUP BY user_id, id);

                    $likes = (SELECT user_id, id, COUNT(id) as count
                    FROM `{Tables.Likes}`
                    GROUP BY user_id, id);

                    SELECT records.id, records.record, records.date, downloads.count, likes.count
                    FROM `{Tables.Records}` as records
                    INNER JOIN $downloads as downloads ON downloads.id == records.id
                    LEFT JOIN $likes as likes ON likes.id == records.id
                    GROUP BY records.id, records.record, records.date, downloads.count, likes.count
                ";
            }

            private static string Uploaded(string userId)
            {
                return $@"
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
                    LEFT JOIN $likes as likes ON likes.id == records.id
                    WHERE records.user_id == ""{userId}""
                    GROUP BY records.id, records.record, records.date, downloads.count, likes.count
                ";
            }
        }
    }
}