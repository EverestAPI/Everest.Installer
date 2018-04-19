using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;

namespace MonoMod.Installer {
    public class GameModder {

        public readonly GameModInfo Info;

        public event Action OnStart;
        public event Action OnFinish;
        public event Action<Exception> OnError;

        public event Action<Status, float> OnProgress = (status, progress) => { };

        public GameModder(GameModInfo info) {
            Info = info;
        }

        public void Install() {
            Console.WriteLine("Starting installation");
            OnStart?.Invoke();
            try {

                if (Info.CurrentInstalledModVersion != null) {
                    _Restore();
                    Console.WriteLine();
                }

                _Backup();
                Console.WriteLine();

                _DownloadAndUnpack();
                Console.WriteLine();

                _Install();
                Console.WriteLine();

            } catch (Exception e) {
                Console.WriteLine(e);
                Console.WriteLine("Error! Please check installer-log.txt");
                OnError?.Invoke(e);
                if (Debugger.IsAttached)
                    throw;
                return;
            }
            Console.WriteLine("Finished installing!");
            Console.WriteLine();
            OnFinish?.Invoke();
        }

        public void Uninstall() {
            Console.WriteLine("Starting uninstall");
            OnStart?.Invoke();
            try {

                _Restore();

            } catch (Exception e) {
                Console.WriteLine(e);
                Console.WriteLine("Error! Please check installer-log.txt");
                OnError?.Invoke(e);
                if (Debugger.IsAttached)
                    throw;
                return;
            }
            Console.WriteLine("Finished uninstalling!");
            OnFinish?.Invoke();
        }

        private void _DownloadAndUnpack() {
            string root = Info.CurrentGamePath;
            Console.WriteLine($"STEP: DOWNLOAD & UNPACK");

            byte[] zipData = _Download(Info.CurrentInstallingModVersion.URL);

            OnProgress?.Invoke(Status.Unpack, 0f);

            using (MemoryStream ms = new MemoryStream(zipData))
            using (ZipArchive zip = new ZipArchive(ms)) {
                int i = 0;
                foreach (ZipArchiveEntry entry in zip.Entries) {
                    OnProgress?.Invoke(Status.Unpack, (i + 1) / (float) zip.Entries.Count);
                    string to = Path.Combine(root, entry.Name);
                    string toParent = Path.GetDirectoryName(to);
                    Console.WriteLine($"{entry.Name} -> {to}");
                    if (!Directory.Exists(toParent))
                        Directory.CreateDirectory(toParent);
                    if (File.Exists(to))
                        File.Delete(to);
                    using (FileStream fs = File.OpenWrite(to))
                    using (Stream compressed = entry.Open())
                        compressed.CopyTo(fs);
                    i++;
                }
            }

            OnProgress?.Invoke(Status.Unpack, 1f);
        }

        private void _Install() {
            Console.WriteLine("STEP: INSTALL");
            OnProgress?.Invoke(Status.Install, 0f);

            Info.Install(progress => OnProgress?.Invoke(Status.Install, progress));

            OnProgress?.Invoke(Status.Install, 1f);
        }

        private void _Backup() {
            string root = Info.CurrentGamePath;
            Console.WriteLine($"STEP: BACKUP @ {root}");
            OnProgress?.Invoke(Status.Backup, 0f);

            GameModInfo.ModBackup[] backups = Info.Backups;
            for (int i = 0; i < backups.Length; i++) {
                OnProgress?.Invoke(Status.Backup, (i + 1) / (float) backups.Length);

                GameModInfo.ModBackup backup = backups[i];
                string from = Path.Combine(root, backup.From);
                string to = Path.Combine(root, backup.To);

                if (!File.Exists(from)) {
                    Console.WriteLine($"File not found, skipping: {backup.From}");
                    continue;
                }

                Console.WriteLine($"{backup.From} -> {backup.To}");
                string dir = Path.GetDirectoryName(to);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                File.Copy(from, to, true);
            }

            OnProgress?.Invoke(Status.Backup, 1f);
        }

        private void _Restore() {
            string root = Info.CurrentGamePath;
            Console.WriteLine($"STEP: RESTORE @ {root}");
            OnProgress?.Invoke(Status.Restore, 0f);

            GameModInfo.ModBackup[] backups = Info.Backups;
            for (int i = 0; i < backups.Length; i++) {
                OnProgress?.Invoke(Status.Restore, (i + 1) / (float) backups.Length);

                GameModInfo.ModBackup backup = backups[i];
                string from = Path.Combine(root, backup.From);
                string to = Path.Combine(root, backup.To);

                if (!File.Exists(to)) {
                    Console.WriteLine($"File not found, skipping: {backup.From}");
                    continue;
                }

                Console.WriteLine($"{backup.From} <- {backup.To}");
                string dir = Path.GetDirectoryName(from);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                File.Copy(to, from, true);
            }

            OnProgress?.Invoke(Status.Restore, 1f);
        }

        private byte[] _Download(string url) {
            if (url.StartsWith("|local|")) {
                Console.WriteLine($"Reading local file {url}");
                return File.ReadAllBytes(url.Substring(7));
            }

            // The following blob of code comes from the old ETGMod.Installer.

            byte[] data = null;

            Console.WriteLine($"Downloading {url}");

            DateTime timeStart = DateTime.Now;
            using (WebClient wc = new WebClient()) {
                using (Stream s = wc.OpenRead(url)) {
                    long sLength;
                    if (s.CanSeek) {
                        // Mono
                        sLength = s.Length;
                    } else {
                        // .NET
                        HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
                        request.UserAgent = $"MonoMod.Installer {Assembly.GetEntryAssembly().GetName().Version}";
                        request.Method = "HEAD";
                        using (HttpWebResponse response = (HttpWebResponse) request.GetResponse()) {
                            sLength = response.ContentLength;
                        }
                    }
                    Console.WriteLine($"{sLength} bytes");
                    data = new byte[sLength];

                    long progressSize = sLength;
                    int progressScale = 1;
                    while (progressSize > int.MaxValue) {
                        progressScale *= 10;
                        progressSize = sLength / progressScale;
                    }

                    OnProgress?.Invoke(Status.Download, 0f);

                    DateTime timeLast = timeStart;

                    int read;
                    int readForSpeed = 0;
                    int pos = 0;
                    int speed = 0;
                    TimeSpan td;
                    while (pos < data.Length) {
                        read = s.Read(data, pos, Math.Min(2048, data.Length - pos));
                        pos += read;
                        readForSpeed += read;

                        td = (DateTime.Now - timeLast);
                        if (td.TotalMilliseconds > 100) {
                            speed = (int) ((readForSpeed / 1024D) / td.TotalSeconds);
                            readForSpeed = 0;
                            timeLast = DateTime.Now;
                        }

                        OnProgress?.Invoke(Status.Download, (float) ((pos / progressScale) / (double) progressSize));
                        Console.WriteLine(
                            "Downloading - " +
                            (int) (Math.Round(100D * ((pos / progressScale) / (double) progressSize))) + "%, " +
                            speed + " KiB/s"
                        );
                    }

                }
            }

            OnProgress?.Invoke(Status.Download, 1f);

            string logSize = (data.Length / 1024D).ToString(CultureInfo.InvariantCulture);
            logSize = logSize.Substring(0, Math.Min(logSize.IndexOf('.') + 3, logSize.Length));
            string logTime = (DateTime.Now - timeStart).TotalSeconds.ToString(CultureInfo.InvariantCulture);
            logTime = logTime.Substring(0, Math.Min(logTime.IndexOf('.') + 3, logTime.Length));
            Console.WriteLine($"Downloaded {logSize} KiB in {logTime} seconds.");

            return data;
        }

        public enum Status {
            Backup,
            Restore,
            Download,
            Unpack,
            Install,
        }

    }
}
