using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
            Console.WriteLine("STARTING: Install");
            OnStart?.Invoke();
            try {

                if (Info.CurrentInstalledModVersion != null) {
                    _Restore();
                }

                _Backup();
            
                _Install();

            } catch (Exception e) {
                Console.WriteLine("ERROR: " + e);
                OnError?.Invoke(e);
                if (Debugger.IsAttached)
                    throw;
                return;
            }
            Console.WriteLine("FINISHED: Install");
            OnFinish?.Invoke();
        }

        public void Uninstall() {
            Console.WriteLine("STARTING: Uninstall");
            OnStart?.Invoke();
            try {

                _Restore();

            } catch (Exception e) {
                Console.WriteLine("ERROR: " + e);
                OnError?.Invoke(e);
                if (Debugger.IsAttached)
                    throw;
                return;
            }
            Console.WriteLine("FINISHED: Uninstall");
            OnFinish?.Invoke();
        }

        private void _Install() {
            OnProgress?.Invoke(Status.Install, 0f);

            OnProgress?.Invoke(Status.Install, 1f);
            Thread.Sleep(4000);

            OnProgress?.Invoke(Status.Install, 1f);
        }

        private void _Uninstall() {
            OnProgress?.Invoke(Status.Uninstall, 0f);

            OnProgress?.Invoke(Status.Uninstall, 1f);
            Thread.Sleep(4000);

            OnProgress?.Invoke(Status.Uninstall, 1f);
        }

        private void _Backup() {
            string root = Info.CurrentGamePath;
            Console.WriteLine($"BACKUP @ {root}");
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
            Console.WriteLine($"RESTORE @ {root}");
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

        private Stream _Download(string url) {
            return null;
        }
        
        private void _ModAssembly(string file) {


        }

        public enum Status {
            Download,
            Backup,
            Restore,
            Install,
            Uninstall
        }

    }
}
