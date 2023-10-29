using ServerSharing.Data;

namespace ServerSharing
{
    internal partial class SelectQueryFactory
    {
        private class SelectQueryContainer
        {
            public static string Create(EntryType type)
            {
                return type switch
                {
                    EntryType.All => All,
                    EntryType.Downloaded => Downloaded,
                    EntryType.Uploaded => Uploaded,
                    EntryType.Liked => Liked,
                    _ => throw new NotSupportedException()
                };
            }

            private static string All => 
                    $@" SELECT records.*, if (records.user_id = $user_id, true, false) AS my_record
                        FROM `{Tables.Records}` AS records";

            private static string Downloaded =>
                    $@" SELECT records.*, if (records.user_id = $user_id, true, false) AS my_record
                        FROM (SELECT id
                            FROM `{Tables.Downloads}`
                            WHERE user_id = $user_id) AS my_downloads
                        INNER JOIN `{Tables.Records}` AS records ON records.id = my_downloads.id";

            private static string Uploaded =>
                    $@" SELECT records.*, if (records.user_id = $user_id, true, false) AS my_record
                        FROM `{Tables.Records}` VIEW idx_user_id AS records
                        WHERE user_id = $user_id";

            private static string Liked =>
                    $@" SELECT records.*, if (records.user_id = $user_id, true, false) AS my_record
                        FROM (SELECT id
                            FROM `{Tables.Likes}`
                            WHERE user_id = $user_id) AS my_likes
                        INNER JOIN `{Tables.Records}` AS records ON records.id = my_likes.id";
        }
    }
}