using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace MonoMod.Installer.Everest {
    public class EverestInfo : GameModInfo {

        public readonly static Random RNG = new Random();

        public override string GameName {
            get {
                return "Celeste";
            }
        }

        public override string ModName {
            get {
                return "Everest";
            }
        }

        public override string ModInstallerName {
            get {
                return "Everest.Installer";
            }
        }

        public override Image HeaderImage {
            get {
                return Properties.Resources.header;
            }
        }

        public override Image BackgroundImage {
            get {
                return Properties.Resources.background;
            }
        }

        public override string ExecutableDir {
            get {
                if ((PlatformHelper.Current & Platform.MacOS) == Platform.MacOS)
                    // From ETG:
                    // /Users/$USER/Library/Application Support/Steam/SteamApps/common/Enter the Gungeon/EtG_OSX.app/Contents/MacOS/EtG_OSX
                    return "Celeste.app/Contents/MacOS".NormalizeFilepath();

                return null;
            }
        }

        public override string ExecutableName {
            get {
                string env = Environment.GetEnvironmentVariable("EVEREST_EXE");
                if (!string.IsNullOrEmpty(env))
                    return env;
                return "Celeste.exe";
            }
        }

        public override string[] Assemblies {
            get {
                if (!string.IsNullOrEmpty(ExecutableDir))
                    return new string[] {
                        Path.Combine(ExecutableDir, ExecutableName)
                    };

                return new string[] {
                    ExecutableName
                };
            }
        }

        public override ModBackup[] Backups {
            get {
                if (!string.IsNullOrEmpty(ExecutableDir))
                    return new ModBackup[] {
                        new ModBackup { From = Path.Combine(ExecutableDir, ExecutableName), To = Path.Combine(ExecutableDir, Path.Combine("orig", ExecutableName)) }
                    };

                return new ModBackup[] {
                    new ModBackup { From = ExecutableName, To = Path.Combine("orig", ExecutableName) }
                };
            }
        }

        public override Dictionary<string, string> GameIDs {
            get {
                return new Dictionary<string, string>() {
                    { "steam", "Celeste" }
                };
            }
        }

        public override List<ModVersion> ModVersions {
            get {
                string data = null;
                using (WebClient wc = new WebClient())
                    data = wc.DownloadString("https://ams3.digitaloceanspaces.com/lollyde/everest-travis/builds_index.txt");

                string[] lines = data.Split('\n');

                List<ModVersion> versions = new List<ModVersion>();
                for (int i = 0; i < lines.Length; i++) {
                    string line = lines[i].Trim('\r', '\n').Trim();
                    if (line.Length == 0 || line.StartsWith("#"))
                        continue;
                    versions.Add(ParseLine(line, "https://ams3.digitaloceanspaces.com"));
                }

                return versions;
            }
        }

        // Copy-paste of Everest's updater version parser.
        private static ModVersion ParseLine(string line, string root) {
            string[] split = line.Split(' ');
            if (split.Length < 2 || split.Length > 3)
                throw new Exception("Version list format incompatible!");

            string url = split[0];
            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                // The index contains a relative path.
                url = root + url;

            if (!url.EndsWith("/" + split[1]))
                throw new Exception("URL (first column) must end in filename (second column)!");

            string name = split[1];
            string branch = "master";

            if (name.EndsWith(".zip"))
                name = name.Substring(0, name.Length - 4);

            if (name.StartsWith("build-"))
                name = name.Substring(6);

            int indexOfBranch = name.IndexOf('-');
            if (indexOfBranch != -1) {
                branch = name.Substring(indexOfBranch + 1);
                name = name.Substring(0, indexOfBranch);
            }

            Version version;
            if (split.Length == 3)
                version = new Version(split[2]);
            else
                version = new Version(0, 0, int.Parse(Regex.Match(split[1], @"\d+").Value));

            if (branch != "master")
                name = $"{name} ({branch})";
            return new ModVersion { Name = name, URL = url, Version = version };
        }

    }
}
