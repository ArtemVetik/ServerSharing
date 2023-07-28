namespace ServerSharing
{
    public static class Tables
    {
        static Tables()
        {
            Records = Environment.GetEnvironmentVariable("RecordsTable");
            Downloads = Environment.GetEnvironmentVariable("DownloadsTable");
            Likes = Environment.GetEnvironmentVariable("LikesTable");
        }

        public static string Records { get; private set; }
        public static string Downloads { get; private set; }
        public static string Likes { get; private set; }
    }
}