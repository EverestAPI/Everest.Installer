using Microsoft.Win32;
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
            Console.WriteLine($"Installing: {Info.CurrentInstallingModVersion.Name}");
            Console.WriteLine($"To: {Info.CurrentGamePath}");
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

                // Sneaky step: Copy the installer.
                string installerPath = null;
                try {
                    string installerTmpPath = Assembly.GetEntryAssembly().Location;
                    installerPath = Path.Combine(Info.CurrentGamePath, Path.GetFileName(installerTmpPath));
                    File.Copy(installerTmpPath, installerPath, true);
                } catch {
                }
                // Sneaky step: Set up URI handler.
                if (!string.IsNullOrEmpty(Info.ModURIProtocol) &&
                    !string.IsNullOrEmpty(Info.ModsDir)) {
                    try {
                        RegistryKey regClasses = Registry
                            .CurrentUser
                            ?.OpenSubKey("Software", true)
                            ?.OpenSubKey("Classes", true);
                        RegistryKey regProtocol = regClasses?.CreateSubKey(Info.ModURIProtocol);
                        if (regProtocol != null) {
                            regProtocol.SetValue("", $"URL:{Info.ModURIProtocol}");
                            regProtocol.SetValue("URL Protocol", "");
                            regProtocol.CreateSubKey(@"shell\open\command").SetValue("", $"\"{installerPath}\" %1");
                        }
                    } catch {
                    }
                }

                _Install();
                Console.WriteLine();

            } catch (Exception e) {
                Console.WriteLine($"Failed installing {Info.CurrentInstallingModVersion.Name}");
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

                // Sneaky step: Remove URI handler.
                if (!string.IsNullOrEmpty(Info.ModURIProtocol) &&
                    !string.IsNullOrEmpty(Info.ModsDir)) {
                    try {
                        RegistryKey regClasses = Registry
                            .CurrentUser
                            ?.OpenSubKey("Software", true)
                            ?.OpenSubKey("Classes", true);
                        regClasses?.DeleteSubKey(Info.ModURIProtocol, false);
                    } catch {
                    }
                }

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

        public void DownloadMod(string url) {
            Console.WriteLine("Starting mod download");

            Uri uri = new Uri(url);

            string modRoot = Info.ModsDir;
            if (!Directory.Exists(modRoot))
                modRoot = Path.Combine(Info.CurrentGamePath, modRoot);
            if (!Directory.Exists(modRoot))
                Directory.CreateDirectory(modRoot);

            string modPath = Path.Combine(modRoot, Path.GetFileName(uri.AbsolutePath));

            OnStart?.Invoke();
            try {

                byte[] zipData = _Download(url);

                Console.WriteLine("Verifying");

                using (MemoryStream ms = new MemoryStream(zipData))
                using (ZipArchive zip = new ZipArchive(ms)) {
                    string name;
                    if (!Info.VerifyMod(zip, out name)) {
                        Console.WriteLine("Error: Invalid mod.");
                        OnError?.Invoke(null);
                        return;
                    }
                    if (!string.IsNullOrEmpty(name)) {
                        modPath = Path.Combine(modRoot, name + ".zip");
                    }
                }

                Console.WriteLine("Writing data to file");
                if (File.Exists(modPath))
                    File.Delete(modPath);
                File.WriteAllBytes(modPath, zipData);

            } catch (Exception e) {
                Console.WriteLine($"Failed downloading {url}");
                Console.WriteLine(e);
                Console.WriteLine("Error! Please check installer-log.txt");
                OnError?.Invoke(e);
                if (Debugger.IsAttached)
                    throw;
                return;
            }
            Console.WriteLine($"{Path.GetFileName(modPath)} downloaded!");
            Console.WriteLine();
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

                    if (!entry.Name.EndsWith("/") && entry.Length != 0) {
                        string entryName = entry.Name;
                        if (entryName.StartsWith("main/"))
                            entryName = entryName.Substring(5);

                        string to = Path.Combine(root, entryName);
                        string toParent = Path.GetDirectoryName(to);
                        Console.WriteLine($"{entry.Name} -> {to}");

                        if (!Directory.Exists(toParent))
                            Directory.CreateDirectory(toParent);

                        if (File.Exists(to))
                            File.Delete(to);

                        using (FileStream fs = File.OpenWrite(to))
                        using (Stream compressed = entry.Open())
                            compressed.CopyTo(fs);
                    }

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

            Console.WriteLine($"Downloading {url}");

            using (MemoryStream copy = new MemoryStream()) {
                DateTime timeStart = DateTime.Now;
                using (WebClient wc = new WebClient()) {
                    using (Stream input = wc.OpenRead(url)) {
                        long length;
                        if (input.CanSeek) {
                            // Mono
                            length = input.Length;
                        } else {
                            // .NET
                            try {
                                HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
                                request.UserAgent = $"MonoMod.Installer {Assembly.GetEntryAssembly().GetName().Version}";
                                request.Method = "HEAD";
                                using (HttpWebResponse response = (HttpWebResponse) request.GetResponse()) {
                                    length = response.ContentLength;
                                }
                            } catch (Exception) {
                                length = 0;
                            }
                        }
                        Console.WriteLine($"{length} bytes");

                        long progressSize = length;
                        int progressScale = 1;
                        while (progressSize > int.MaxValue) {
                            progressScale *= 10;
                            progressSize = length / progressScale;
                        }

                        OnProgress?.Invoke(Status.Download, 0f);

                        DateTime timeLast = timeStart;

                        byte[] buffer = new byte[4096];
                        DateTime timeLastSpeed = timeStart;
                        int read = 1;
                        int readForSpeed = 0;
                        int pos = 0;
                        int speed = 0;
                        int count = 0;
                        TimeSpan td;
                        while (read > 0) {
                            count = length > 0 ? (int) Math.Min(buffer.Length, length - pos) : buffer.Length;
                            read = input.Read(buffer, 0, count);
                            copy.Write(buffer, 0, read);
                            pos += read;
                            readForSpeed += read;

                            td = (DateTime.Now - timeLast);
                            if (td.TotalMilliseconds > 100) {
                                speed = (int) ((readForSpeed / 1024D) / td.TotalSeconds);
                                readForSpeed = 0;
                                timeLast = DateTime.Now;
                            }

                            if (length > 0) {
                                OnProgress?.Invoke(Status.Download, (float) ((pos / progressScale) / (double) progressSize));
                                LogWriter.OnWriteLine?.Invoke(
                                    $"Downloading: {((int) Math.Floor(100D * (pos / (double) length)))}% @ {speed} KiB/s"
                                );
                            } else {
                                OnProgress?.Invoke(Status.Download, 1f);
                                LogWriter.OnWriteLine?.Invoke(
                                    $"Downloading: {((int) Math.Floor(pos / 1000D))}KiB @ {speed} KiB/s"
                                );
                            }
                        }

                    }
                }

                OnProgress?.Invoke(Status.Download, 1f);

                byte[] data = copy.ToArray();
                string logSize = (data.Length / 1024D).ToString(CultureInfo.InvariantCulture);
                logSize = logSize.Substring(0, Math.Min(logSize.IndexOf('.') + 3, logSize.Length));
                string logTime = (DateTime.Now - timeStart).TotalSeconds.ToString(CultureInfo.InvariantCulture);
                logTime = logTime.Substring(0, Math.Min(logTime.IndexOf('.') + 3, logTime.Length));
                Console.WriteLine($"Downloaded {logSize} KiB in {logTime} seconds.");

                return data;
            }
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
