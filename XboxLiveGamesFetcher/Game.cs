namespace XboxLiveGamesFetcher
{
    public class Game
    {
        public string Name { get; set; }
        public string UrlInfos { get; set; }
        public string UrlCover { get; set; }
        public string TimePlayed { get; set; }

        public Game() { }

        public Game(string name, string urlInfos)
        {
            Name = name;
            UrlInfos = urlInfos;
        }
    }
}
