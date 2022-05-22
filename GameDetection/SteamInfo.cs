namespace GameDetection
{
    public class SteamInfo
    {
        public SteamInfo()
        {
            SteamInstalled = false;
            InstallPath = string.Empty;
            Libraries = new List<SteamLibrary>();
        }

        public bool SteamInstalled { get; internal set; }
        public string? InstallPath { get; internal set; }
        public List<SteamLibrary>? Libraries { get; internal set; }
    }
}