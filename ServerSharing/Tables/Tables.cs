namespace ServerSharing
{
    public static class Tables
    {
        public static readonly string Records;
        public static readonly string Downloads;
        public static readonly string Likes;
        public static readonly string Ratings;

        static Tables()
        {
            Records = Environment.GetEnvironmentVariable("RecordsTable");
            Downloads = Environment.GetEnvironmentVariable("DownloadsTable");
            Likes = Environment.GetEnvironmentVariable("LikesTable");
            Ratings = Environment.GetEnvironmentVariable("RateTable");
        }
    }
}