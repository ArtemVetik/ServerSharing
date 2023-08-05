namespace ServerSharing
{
    public class Records
    {
        public const string Name = "records";
        public const string Id = "records.id";
        public const string UserId = "records.user_id";
        public const string Date = "records.date";
        public const string Body = "records.body";
        public const string SId = "id";
        public const string SUserId = "user_id";
        public const string SDate = "date";
        public const string SBody = "body";

        static Records()
        {
            TablePath = Environment.GetEnvironmentVariable("RecordsTable");
        }

        public static readonly string TablePath;
    }

    public class Downloads
    {
        public const string Name = "downloads";
        public const string UserId = "downloads.user_id";
        public const string Id = "downloads.id";
        public const string SUserId = "user_id";
        public const string SId = "id";

        static Downloads()
        {
            TablePath = Environment.GetEnvironmentVariable("DownloadsTable");
        }

        public static readonly string TablePath;
    }

    public class Likes
    {
        public const string Name = "likes";
        public const string UserId = "likes.user_id";
        public const string Id = "likes.id";
        public const string SUserId = "user_id";
        public const string SId = "id";

        static Likes()
        {
            TablePath = Environment.GetEnvironmentVariable("LikesTable");
        }

        public static readonly string TablePath;
    }
}