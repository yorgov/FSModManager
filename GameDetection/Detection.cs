using Gameloop.Vdf;
using Gameloop.Vdf.JsonConverter;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;

namespace GameDetection
{
    public static class Detection
    {
        private const string HKEY_LM_SteamKey = @"SOFTWARE\WOW6432Node\Valve\Steam";
        private const long FS22_APP_ID = 1248130;
        private const long FS19_APP_ID = 787860;
        private static SteamInfo steamInfo;

        public static IEnumerable<GameInfo> DetectGames()
        {
            steamInfo = new SteamInfo();
            var steamReg = Registry.LocalMachine.OpenSubKey(HKEY_LM_SteamKey);
            if (steamReg != null)
            {
                steamInfo.SteamInstalled = true;
                steamInfo.InstallPath = Convert.ToString(steamReg.GetValue("InstallPath"));
                FillInLibraryFolders(steamInfo);
            }
            else steamInfo.SteamInstalled = false;

            var fs19 = new GameInfo
            {
                SteamAppId = FS19_APP_ID,
                Name = "Farming Simulator 19"
            };
            var fs22 = new GameInfo
            {
                SteamAppId = FS22_APP_ID,
                Name = "Farming Simulator 22"
            };

            return DetectGames(fs19, fs22);
        }

        private static GameInfo[] DetectGames(params GameInfo[] games)
        {
            var result = new List<GameInfo>();
            foreach (var item in games)
            {
                DetectSteamInstall(item.SteamAppId, item);
                DetectRegInstall(item.Name, item);
                GetExecutableInfo(item);
                SetUpModsFolder(item.Name, item);
                result.Add(item);
            }
            return result.ToArray();
        }

        private static void FillInLibraryFolders(SteamInfo steamInfo)
        {
            if (steamInfo.InstallPath == null) throw new ArgumentNullException();
            var vdfFileContent = File.ReadAllText(Path.Combine(steamInfo.InstallPath, "config", "libraryfolders.vdf"));
            var libraries = ExtractLibraries(vdfFileContent);
            if (libraries != null)
            {
                foreach (var library in libraries)
                {
                    if (library == null) continue;
                    var steamLibrary = new SteamLibrary();
                    var pathValue = library.SelectToken("$.path");
                    if (pathValue == null) continue;
                    steamLibrary.Path = pathValue.ToString();
                    foreach (JProperty app in library.SelectToken("$.apps").Children())
                    {
                        if (app == null) continue;
                        steamLibrary.Apps.Add(Convert.ToInt64(app.Name));
                    }
                    steamInfo.Libraries.Add(steamLibrary);
                }
            }
        }

        private static IEnumerable<JToken>? ExtractLibraries(string vdfFileContent)
        {
            if (string.IsNullOrWhiteSpace(vdfFileContent)) return null;
            var first = VdfConvert.Deserialize(vdfFileContent).ToJson().First;
            if (first == null) return null;
            var elements = first.Children().Skip(1);
            if (elements == null || !elements.Any()) return null;
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
            IEnumerable<JToken> libraries = elements.Where(d => d.First != null).Select(d => d.First);
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
            if (libraries == null || !libraries.Any()) return null;
            return libraries;
        }

        private static void GetExecutableInfo(GameInfo gameInfo)
        {
            var di = new DirectoryInfo(gameInfo.InstallLocation);
            var foundFiles = di.GetFiles("FarmingSimulator*.exe");
            if (foundFiles.Any())
            {
                gameInfo.Executable = foundFiles[0];
            }
        }

        private static void SetUpModsFolder(string name, GameInfo gameInfo)
        {
            var docuPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var di = new DirectoryInfo(Path.Combine(docuPath, "My Games", Path.GetFileNameWithoutExtension(gameInfo.Executable.Name).Replace(" ", string.Empty), "mods"));
            if (!di.Exists)
            {
                di.Create();
            }
            gameInfo.ModsFolder = di.FullName;
        }

        private static void DetectRegInstall(string displayName, GameInfo gameInfo)
        {
            var reg = Registry.LocalMachine
                .OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall");
            if (reg == null) throw new InvalidOperationException();
            foreach (var app in reg.GetSubKeyNames())
            {
                var appreg = reg.OpenSubKey(app);
                if (appreg == null) continue;
                var regDisplayName = Convert.ToString(appreg.GetValue("DisplayName"));
                if (regDisplayName == null || regDisplayName != displayName) continue;
                var installLocation = Convert.ToString(appreg.GetValue("InstallLocation"));
                gameInfo.InstallMethod |= InstallMethod.DirectDownload;
                gameInfo.Installed = true;
                if (!string.IsNullOrWhiteSpace(installLocation))
                {
                    gameInfo.InstallLocation = installLocation;
                }
            }
        }

        private static void DetectSteamInstall(long appId, GameInfo gameInfo)
        {
            if (!steamInfo.SteamInstalled) return;
            if (steamInfo.Libraries == null) return;
            var lib = steamInfo.Libraries.FirstOrDefault(f => f.Apps.Contains(appId));
            if (lib == null) return;
            var acf = Path.Combine(lib.Path, "steamapps", $"appmanifest_{appId}.acf");
            var acfStruct = new AcfReader(acf).ACFFileToStruct();
            gameInfo.InstallMethod |= InstallMethod.Steam;
            gameInfo.InstallLocation = Path.Combine(lib.Path, "steamapps", "common", acfStruct.SubACF.First().Value.SubItems["installdir"]);
            gameInfo.Installed = true;
        }
    }
}