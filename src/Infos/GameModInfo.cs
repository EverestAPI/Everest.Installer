using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;

namespace MonoMod.Installer {
    public abstract class GameModInfo {

        public abstract string GameName { get; }

        public abstract string ModName { get; }
        public abstract string ModInstallerName { get; }

        public abstract Image HeaderImage { get; }
        public abstract Image BackgroundImage { get; }

        public abstract string ExecutableDir { get; }
        public abstract string ExecutableName { get; }
        public abstract string[] Assemblies { get; }
        public virtual ModBackup[] Backups {
            get {
                return new ModBackup[0];
            }
        }
        public abstract Dictionary<string, string> GameIDs { get; }

        public virtual string ModURIProtocol => "";
        public virtual string ModsDir => "Mods";

        public abstract List<ModVersion> ModVersions { get; }

        private string _CurrentExecutablePath;
        public virtual string CurrentExecutablePath {
            get {
                return _CurrentExecutablePath ?? "";
            }
            set {
                if (string.IsNullOrEmpty(value)) {
                    OnChangeCurrentExecutablePath?.Invoke(this, _CurrentExecutablePath = null);
                    return;
                }

                if (File.Exists(value) &&
                    value.ToLowerInvariant().EndsWith(ExecutableName.ToLowerInvariant())) {
                    OnChangeCurrentExecutablePath?.Invoke(this, _CurrentExecutablePath = value);
                }
            }
        }
        public virtual string CurrentGamePath {
            get {
                string path = CurrentExecutablePath;
                if (string.IsNullOrEmpty(CurrentExecutablePath) || !File.Exists(path))
                    return null;

                if (!string.IsNullOrEmpty(ExecutableName))
                    path = Path.GetDirectoryName(path);

                string exeDir = ExecutableDir;
                while (!string.IsNullOrEmpty(exeDir)) {
                    path = Path.GetDirectoryName(path);
                    exeDir = Path.GetDirectoryName(exeDir);
                }

                return path;
            }
        }

        private string _CurrentStatus;
        public virtual string CurrentStatus {
            get {
                return _CurrentStatus ?? "";
            }
            set {
                if (string.IsNullOrEmpty(value)) {
                    _CurrentStatus = null;
                    OnChangeCurrentStatus?.Invoke(this, null);
                    return;
                }

                OnChangeCurrentStatus?.Invoke(this, _CurrentStatus = value);
            }
        }

        public virtual ModVersion CurrentInstallingModVersion { get; set; }
        public virtual Version CurrentInstalledModVersion { get; set; }

        public event Action<GameModInfo, string> OnChangeCurrentExecutablePath;
        public event Action<GameModInfo, string> OnChangeCurrentStatus;

        public abstract void Install(Action<float> progress);

        public virtual bool VerifyMod(ZipArchive zip, out string name) {
            name = null;
            return false;
        }

        public class ModVersion {
            public string Name;
            public string URL;
            public Version Version;
            public override string ToString() {
                return Name;
            }
        }

        public class ModBackup {
            public string From;
            public string To;
        }

        public List<ModVersion> GetAndParseVersions_Legacy(string url) {
            string data = null;
            using (WebClient wc = new WebClient())
                data = wc.DownloadString(url);

            string[] lines = data.Split('\n');

            List<ModVersion> versions = new List<ModVersion>();
            for (int i = 0; i < lines.Length; i++) {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line))
                    continue;
                string[] split = line.Split('|');
                versions.Add(new ModVersion { Name = split[0], URL = split[1] });
            }

            return versions;
        }

    }
}
