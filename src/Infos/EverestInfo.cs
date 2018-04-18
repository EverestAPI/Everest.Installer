using Mono.Cecil;
using Mono.Cecil.Cil;
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

        public override string CurrentExecutablePath {
            get {
                return base.CurrentExecutablePath;
            }

            set {
                base.CurrentExecutablePath = value;
                value = base.CurrentExecutablePath;

                if (string.IsNullOrEmpty(value) || !File.Exists(value)) {
                    CurrentStatus = "";
                    CurrentInstalledModVersion = null;
                    return;
                }

                try {
                    using (ModuleDefinition game = ModuleDefinition.ReadModule(CurrentExecutablePath)) {
                        TypeDefinition t_Celeste = game.GetType("Celeste.Celeste");
                        if (t_Celeste == null) {
                            CurrentStatus = "Not Celeste!";
                            base.CurrentExecutablePath = null;
                            CurrentInstalledModVersion = null;
                            return;
                        }

                        string versionString = null;
                        int[] versionInts = null;
                        // Find Celeste .ctor (luckily only has one)
                        MethodDefinition c_Celeste =
                            t_Celeste.FindMethod("System.Void orig_ctor_Celeste()") ??
                            t_Celeste.FindMethod("System.Void .ctor()");
                        if (c_Celeste != null && c_Celeste.HasBody) {
                            Mono.Collections.Generic.Collection<Instruction> instrs = c_Celeste.Body.Instructions;
                            for (int instri = 0; instri < instrs.Count; instri++) {
                                Instruction instr = instrs[instri];
                                MethodReference c_Version = instr.Operand as MethodReference;
                                if (instr.OpCode != OpCodes.Newobj || c_Version?.DeclaringType?.FullName != "System.Version")
                                    continue;

                                // We're constructing a System.Version - check if all parameters are of type int.
                                bool c_Version_intsOnly = true;
                                foreach (ParameterReference param in c_Version.Parameters)
                                    if (param.ParameterType.MetadataType != MetadataType.Int32) {
                                        c_Version_intsOnly = false;
                                        break;
                                    }

                                if (c_Version_intsOnly) {
                                    // Assume that ldc.i4* instructions are right before the newobj.
                                    versionInts = new int[c_Version.Parameters.Count];
                                    for (int i = -versionInts.Length; i < 0; i++)
                                        versionInts[i + versionInts.Length] = instrs[i + instri].GetInt();
                                }

                                if (c_Version.Parameters.Count == 1 && c_Version.Parameters[0].ParameterType.MetadataType == MetadataType.String) {
                                    // Assume that a ldstr is right before the newobj.
                                    versionString = instrs[instri - 1].Operand as string;
                                }

                                // Don't check any other instructions.
                                break;
                            }
                        }

                        // Construct the version from our gathered data.
                        Version version = new Version();
                        if (versionString != null) {
                            version = new Version(versionString);
                        }
                        if (versionInts == null || versionInts.Length == 0) {
                            // ???
                        } else if (versionInts.Length == 2) {
                            version = new Version(versionInts[0], versionInts[1]);
                        } else if (versionInts.Length == 3) {
                            version = new Version(versionInts[0], versionInts[1], versionInts[2]);
                        } else if (versionInts.Length == 4) {
                            version = new Version(versionInts[0], versionInts[1], versionInts[2], versionInts[3]);
                        }

                        string status = $"Celeste {version} {(game.AssemblyReferences.Any(r => r.Name == "FNA") ? "FNA" : "XNA")}";

                        CurrentInstalledModVersion = null;
                        TypeDefinition t_Everest = game.GetType("Celeste.Mod.Everest");
                        if (t_Everest != null) {
                            // The first operation in .cctor is ldstr with the version string.
                            string versionMod = (string) t_Everest.FindMethod("System.Void .cctor()").Body.Instructions[0].operand;
                            int versionSplitIndex = versionMod.IndexOf('-');
                            if (versionSplitIndex == -1) {
                                CurrentInstalledModVersion = new Version(versionMod);
                            } else {
                                CurrentInstalledModVersion = new Version(versionMod.Substring(0, versionSplitIndex));
                            }
                            status = $"{status} + Everest {versionMod}";
                        }

                        CurrentStatus = status;
                    }
                } catch (Exception e) {
                    CurrentStatus = "Error - check log";
                    base.CurrentExecutablePath = null;
                    CurrentInstalledModVersion = null;
                    Console.WriteLine("Error determining status!");
                    Console.WriteLine(e.ToString());
                }
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
