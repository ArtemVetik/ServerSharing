namespace ServerSharing
{
    public static class Tables
    {
        private static readonly string Directory;

        public static readonly string Records;
        public static readonly string Downloads;
        public static readonly string Likes;
        public static readonly string Ratings;
        public static readonly string Images;
        public static readonly string Data;

        static Tables()
        {
            Directory = Environment.GetEnvironmentVariable("TablePath");

            Records = Directory + "/records";
            Downloads = Directory + "/downloads";
            Likes = Directory + "/likes";
            Ratings = Directory + "/rating";
            Images = Directory + "/images";
            Data = Directory + "/data";
        }
    }
}