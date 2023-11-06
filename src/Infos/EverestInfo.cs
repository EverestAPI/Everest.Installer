using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.IO.Compression;
using Newtonsoft.Json.Linq;

namespace MonoMod.Installer.Everest {
    public partial class EverestInfo : GameModInfo {

        public readonly static Random RNG = new Random();

        public override string GameName => "Celeste";
        public override string ModName => "Everest";
        public override string ModURIProtocol => "Everest";
        public override string ModInstallerName => "Everest.Installer";

        public override Image HeaderImage => Properties.Resources.header;
        public override Image BackgroundImage => Properties.Resources.background;

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
                string root = ExecutableDir;
                if (string.IsNullOrEmpty(root))
                    root = "";

                List<ModBackup> backups = new List<ModBackup>();

                string origDir = Path.GetFullPath(Path.Combine(CurrentGamePath, root, "orig"));
                if (Directory.Exists(origDir))
                    backups.AddRange(Directory.EnumerateFiles(origDir, "*", SearchOption.AllDirectories).Select(file =>
                        {
                            if (file.StartsWith(origDir))
                                file = file.Substring(origDir.Length + 1);
                            if (file.StartsWith("Content"))
                                return null;

                            return new ModBackup
                            {
                                From = Path.Combine(root, file),
                                To = Path.Combine(root, "orig", file)
                            };
                        }
                    ).Where(b => b != null));

                if (!File.Exists(Path.Combine(origDir, ExecutableName)))
                    backups.Add(new ModBackup { From = Path.Combine(root, ExecutableName), To = Path.Combine(root, "orig", ExecutableName) });

                return backups.ToArray();
            }
        }

        public override Dictionary<string, string> GameIDs {
            get {
                return new Dictionary<string, string>() {
                    { "local", "Celeste.exe" },
                    { "steam", "Celeste" },
                    { "epic", "Celeste" }
                };
            }
        }

        public override List<ModVersion> ModVersions {
            get {
                const int offset = 700;
                const string artifactFormat = "https://dev.azure.com/EverestAPI/Everest/_apis/build/builds/{0}/artifacts?artifactName=main&api-version=5.0&%24format=zip";
                const string index = "https://dev.azure.com/EverestAPI/Everest/_apis/build/builds?api-version=5.0";

                string dataRaw = null;
                using (WebClient wc = new WebClient())
                    dataRaw = wc.DownloadString(index);

                List<ModVersion> versions = new List<ModVersion>();

                JObject root = JObject.Parse(dataRaw);
                JArray list = root["value"] as JArray;
                foreach (JObject build in list) {
                    if (build["status"].ToObject<string>() != "completed" || build["result"].ToObject<string>() != "succeeded")
                        continue;

                    string reason = build["reason"].ToObject<string>();
                    if (reason != "manual" && reason != "individualCI")
                        continue;

                    int id = build["id"].ToObject<int>();
                    string branch = build["sourceBranch"].ToObject<string>().Replace("refs/heads/", "");
                    string url = string.Format(artifactFormat, id);

                    string name = (id + offset).ToString();
                    if (branch != "master")
                        name = $"{name} ({branch})";

                    versions.Add(new ModVersion() {
                        Name = name,
                        URL = url,
                        Version = new Version(1, 0, id)
                    });
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

                bool tryCoreDll = false;
                retry:;
                try {
                    using (ModuleDefinition game = ModuleDefinition.ReadModule(tryCoreDll ?
                        Path.ChangeExtension(CurrentExecutablePath, ".dll") :
                        CurrentExecutablePath
                    )) {
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

                        string status = $"Celeste {version}-{(game.AssemblyReferences.Any(r => r.Name == "FNA") ? "fna" : "xna")}";

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
                    if (!tryCoreDll)
                    {
                        tryCoreDll = true;
                        goto retry;
                    }

                    CurrentStatus = "Error - check log";
                    base.CurrentExecutablePath = null;
                    CurrentInstalledModVersion = null;
                    Console.WriteLine("Error determining status!");
                    Console.WriteLine(e.ToString());
                }
            }
        }

        public override bool VerifyMod(ZipArchive zip, out string name) {
            name = null;
            bool valid = false;

            foreach (ZipArchiveEntry entry in zip.Entries) {
                if (entry.FullName == "everest.yaml" ||
                    entry.FullName == "everest.yml") {
                    // Hack: Let's just read the first name.
                    using (Stream stream = entry.Open())
                    using (StreamReader reader = new StreamReader(stream))
                        while (!reader.EndOfStream) {
                            string line = reader.ReadLine().Trim();
                            if (line.Contains("Name:") && name == null) {
                                valid = true;
                                name = line.Substring(line.IndexOf("Name:") + 5).Trim();
                            }
                        }
                }
            }

            return valid;
        }

    }
}
