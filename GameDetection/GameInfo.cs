namespace GameDetection
{
    public class GameInfo
    {
        public long SteamAppId { get; set; } = 0;
        public string Name { get; internal set; } = string.Empty;
        public InstallMethod InstallMethod { get; internal set; }
        public bool Installed { get; internal set; }
        public string InstallLocation { get; internal set; } = string.Empty;
        public string ModsFolder { get; set; } = string.Empty;
        public FileInfo? Executable { get; set; }
    }
}