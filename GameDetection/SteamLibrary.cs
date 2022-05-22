namespace GameDetection
{
    public class SteamLibrary
    {
        public string Path { get; set; } = "";
        public List<long> Apps { get; set; } = new List<long>();
    }
}