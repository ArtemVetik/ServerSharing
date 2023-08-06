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
                    $downloads = (SELECT {Downloads.Id}, COUNT({Downloads.Id}) as count
                    FROM `{Downloads.TablePath}` {Downloads.Name}
                    GROUP BY {Downloads.Id}
                    ORDER BY count);

                    $likes = (SELECT {Likes.Id}, COUNT({Likes.Id}) as count
                    FROM `{Likes.TablePath}` {Likes.Name}
                    GROUP BY {Likes.Id}
                    ORDER BY count);

                    $rating = (SELECT {RatingTable.Id}, COUNT({RatingTable.Id}) as count, AVG({RatingTable.Rating}) as avg
                    FROM `{RatingTable.TablePath}` {RatingTable.Name}
                    GROUP BY {RatingTable.Id});

                    SELECT {Records.Id}, {Records.Body}, {Records.Date}, {Downloads.Name}.count, {Likes.Name}.count, {RatingTable.Name}.count, {RatingTable.Name}.avg
                    FROM `{Records.TablePath}` as {Records.Name}
                    LEFT JOIN $downloads as {Downloads.Name} ON {Downloads.Id} == {Records.Id}
                    LEFT JOIN $likes as {Likes.Name} ON {Likes.Id} == {Records.Id}
                    LEFT JOIN $rating as {RatingTable.Name} ON {RatingTable.Id} == {Records.Id}
                ";
            }

            private static string Downloaded(string userId)
            {
                return $@"
                    $downloads = (SELECT {Downloads.UserId}, {Downloads.Id}, COUNT({Downloads.Id}) as count
                    FROM `{Downloads.TablePath}` {Downloads.Name}
                    where {Downloads.UserId} = ""{userId}""
                    GROUP BY {Downloads.UserId}, {Downloads.Id});

                    $likes = (SELECT {Likes.UserId}, {Likes.Id}, COUNT({Likes.Id}) as count
                    FROM `{Likes.TablePath}` {Likes.Name}
                    GROUP BY {Likes.UserId}, {Likes.Id});

                    $rating = (SELECT {RatingTable.Id}, COUNT({RatingTable.Id}) as count, AVG({RatingTable.Rating}) as avg
                    FROM `{RatingTable.TablePath}` {RatingTable.Name}
                    GROUP BY {RatingTable.Id});

                    SELECT {Records.Id}, {Records.Body}, {Records.Date}, {Downloads.Name}.count, {Likes.Name}.count, {RatingTable.Name}.count, {RatingTable.Name}.avg
                    FROM `{Records.TablePath}` as {Records.Name}
                    INNER JOIN $downloads as {Downloads.Name} ON {Downloads.Id} == {Records.Id}
                    LEFT JOIN $likes as {Likes.Name} ON {Likes.Id} == {Records.Id}
                    LEFT JOIN $rating as {RatingTable.Name} ON {RatingTable.Id} == {Records.Id}
                ";
            }

            private static string Uploaded(string userId)
            {
                return $@"
                    $downloads = (SELECT {Downloads.Id}, COUNT({Downloads.Id}) as count
                    FROM `{Downloads.TablePath}` {Downloads.Name}
                    GROUP BY {Downloads.Id}
                    ORDER BY count);

                    $likes = (SELECT {Likes.Id}, COUNT({Likes.Id}) as count
                    FROM `{Likes.TablePath}` {Likes.Name}
                    GROUP BY {Likes.Id}
                    ORDER BY count);

                    $rating = (SELECT {RatingTable.Id}, COUNT({RatingTable.Id}) as count, AVG({RatingTable.Rating}) as avg
                    FROM `{RatingTable.TablePath}` {RatingTable.Name}
                    GROUP BY {RatingTable.Id});

                    SELECT {Records.Id}, {Records.Body}, {Records.Date}, {Downloads.Name}.count, {Likes.Name}.count, {RatingTable.Name}.count, {RatingTable.Name}.avg
                    FROM `{Records.TablePath}` as {Records.Name}
                    LEFT JOIN $downloads as {Downloads.Name} ON {Downloads.Id} == {Records.Id}
                    LEFT JOIN $likes as {Likes.Name} ON {Likes.Id} == {Records.Id}
                    LEFT JOIN $rating as {RatingTable.Name} ON {RatingTable.Id} == {Records.Id}
                    WHERE {Records.UserId} == ""{userId}""
                ";
            }

            private static string Liked(string userId)
            {
                return $@"
                    $downloads = (SELECT {Downloads.UserId}, {Downloads.Id}, COUNT({Downloads.Id}) as count
                    FROM `{Downloads.TablePath}` {Downloads.Name}
                    GROUP BY {Downloads.UserId}, {Downloads.Id});

                    $likes = (SELECT {Likes.UserId}, {Likes.Id}, COUNT({Likes.Id}) as count
                    FROM `{Likes.TablePath}` {Likes.Name}
                    where {Likes.UserId} = ""{userId}""
                    GROUP BY {Likes.UserId}, {Likes.Id});

                    $rating = (SELECT {RatingTable.Id}, COUNT({RatingTable.Id}) as count, AVG({RatingTable.Rating}) as avg
                    FROM `{RatingTable.TablePath}` {RatingTable.Name}
                    GROUP BY {RatingTable.Id});

                    SELECT {Records.Id}, {Records.Body}, {Records.Date}, {Downloads.Name}.count, {Likes.Name}.count, {RatingTable.Name}.count, {RatingTable.Name}.avg
                    FROM `{Records.TablePath}` as {Records.Name}
                    INNER JOIN $likes as {Likes.Name} ON {Likes.Id} == {Records.Id}
                    LEFT JOIN $downloads as {Downloads.Name} ON {Downloads.Id} == {Records.Id}
                    LEFT JOIN $rating as {RatingTable.Name} ON {RatingTable.Id} == {Records.Id}
                ";
            }
        }
    }
}