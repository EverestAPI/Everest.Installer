using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace MonoMod.Installer {
    public class GameModder {

        public readonly GameModInfo Info;

        public event Action OnStart;
        public event Action OnFinish;
        public event Action<Exception> OnError;

        public event Action<ProgressType, string, int, int> OnProgress = (type, text, current, max) => { };

        public event Action<string> OnLog = text => Console.WriteLine(text);

        public GameModder(GameModInfo info) {
            Info = info;
        }

        public void Install() {
            OnStart?.Invoke();
            
            try {
                _Install();
            } catch (Exception e) {
                OnError?.Invoke(e);
                if (Debugger.IsAttached)
                    throw;
                return;
            }

            OnFinish?.Invoke();
        }

        private void _Install() {
            

        }

        public void Uninstall() {
            OnStart?.Invoke();

            try {
                _Uninstall();
            } catch (Exception e) {
                OnError?.Invoke(e);
                if (Debugger.IsAttached)
                    throw;
                return;
            }

            OnFinish?.Invoke();
        }

        private void _Uninstall() {

        }

        public void Backup(string file) {
        }

        public void Restore() {
            /*
            string pathGame = Info.CurrentGamePath;
            string pathBackup = Path.Combine(pathGame, "orig");
            if (!Directory.Exists(pathBackup))
                return;

            string[] files = Directory.GetFiles(pathGame);
            OnProgress(ProgressType.Restore, "Removing leftover files", 0, files.Length);
            for (int i = 0; i < files.Length; i++) {
                string file = Path.GetFileName(files[i]);
                if (!file.Contains(".mm."))
                    continue;
                OnLog($"Removing: {file}");
                OnProgress(ProgressType.Restore, file, i, -1);
                File.Delete(files[i]);
            }

            OnLog("Reverting...");
            if (Info.CurrentInstalledModVersion != null)
                OnLog($"Found previous mod installation: {Info.CurrentInstalledModVersion}");

            OnProgress(ProgressType.Restore, "Reverting from backup", 0, files.Length);
            GameModInfo.ModBackup[] backups = Info.Backups;
            for (int i = 0; i < backups.Length; i++) {
                GameModInfo.ModBackup backup = backups[i];
                OnLog($"Reverting: {backup.To} -> {backup.From}");
                OnProgress(ProgressType.Restore, $"Reverting: {backup.To}", i, -1);

                string origPath = Path.Combine(pathGame, file);
                File.Delete(origPath);
                File.Move(files[i], origPath);
            }

            ins.LogLine("Reloading Assembly-CSharp.dll");
            ins.SetProgress("Reloading Assembly-CSharp.dll", files.Length);
            ins.MainMod = new MonoModder() {
                InputPath = ins.MainModIn
            };
            ins.MainMod.SetupETGModder();
#if DEBUG
            if (LogPath == null) {
                ins.MainMod.Read(); // Read main module first
                ins.MainMod.ReadMod(ins.MainModDir); // ... then mods
                ins.MainMod.MapDependencies(); // ... then all dependencies
            } else
                using (FileStream fileStream = File.Open(LogPath, FileMode.Append)) {
                    using (StreamWriter streamWriter = new StreamWriter(fileStream)) {
                        ins.MainMod.Logger = (string s) => ins.OnActivity();
                        ins.MainMod.Logger += (string s) => streamWriter.WriteLine(s);
                        // MonoMod.MonoModSymbolReader.MDBDEBUG = true;
#endif

                        ins.MainMod.Read(); // Read main module first
                        ins.MainMod.ReadMod(ins.MainModDir); // ... then mods
                        ins.MainMod.MapDependencies(); // ... then all dependencies
#if DEBUG
                        Mono.Cecil.TypeDefinition etgMod = ins.MainMod.Module.GetType("ETGMod");
                        if (etgMod != null) {
                            for (int i = 0; i < etgMod.Methods.Count; i++) {
                                Mono.Cecil.Cil.MethodBody body = etgMod.Methods[i].Body;
                            }
                        }
                    }
                }
            ins.MainMod.Logger = null;
#endif
            ins.EndProgress("Uninstalling complete.");
            */
        }

        public Stream Download(string url) {
            return null;
        }
        
        public void ModAssembly(string file) {
            OnProgress?.Invoke(ProgressType.Install, file, 0, 1);


            OnProgress?.Invoke(ProgressType.Install, file, 1, 1);
        }

        public enum ProgressType {
            Install,
            Restore,
            Download,
            Unzip
        }

    }
}
