using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.RuntimeDetour;
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

namespace MonoMod.Installer.Everest {
    public partial class EverestInfo : GameModInfo {

        public override void Install(Action<float> progress) {
            string root = CurrentGamePath;
            Environment.CurrentDirectory = root;
            Console.WriteLine("Starting MiniInstaller");

            AppDomainSetup nestInfo = new AppDomainSetup();
            // nestInfo.ApplicationBase = Path.GetDirectoryName(root);
            nestInfo.ApplicationBase = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            AppDomain nest = AppDomain.CreateDomain(
                AppDomain.CurrentDomain.FriendlyName + " - MiniInstaller",
                AppDomain.CurrentDomain.Evidence,
                nestInfo,
                AppDomain.CurrentDomain.PermissionSet
            );

            // Opens separate console window.
            /*
            int exit = nest.ExecuteAssembly(Path.Combine(root, "MiniInstaller.exe"), new string[] { });
            if (exit != 0)
                throw new Exception($"Expected return code 0, received {exit}");
            */

            ((MiniInstallerProxy) nest.CreateInstanceAndUnwrap(
                typeof(MiniInstallerProxy).Assembly.FullName,
                typeof(MiniInstallerProxy).FullName
            )).Boot(new MiniInstallerBridge {
                Encoding = Console.Out.Encoding,
                Root = root
            });

            AppDomain.Unload(nest);

        }

        class MiniInstallerProxy : MarshalByRefObject {
            public void Boot(MiniInstallerBridge bridge) {
                Assembly installerAssembly = Assembly.LoadFrom(Path.Combine(bridge.Root, "MiniInstaller.exe"));
                Type installerType = installerAssembly.GetType("MiniInstaller.Program");

                // Fix MonoMod dying when running with a debugger attached because it's running without a console.
                Hook hookReadKey = new Hook(
                    typeof(Console).GetMethod("ReadKey", BindingFlags.Public | BindingFlags.Static, null, new Type[] { }, null),
                    new Func<ConsoleKeyInfo>(() => {
                        return new ConsoleKeyInfo('\n', ConsoleKey.Enter, false, false, false);
                    })
                );

                // Fix old versions of MiniInstaller loading HookGen without RuntimeDetour.
                bool loadedRuntimeDetour = false;
                Hook hookLazyLoadAssembly = new Hook(
                    installerType.GetMethod("LazyLoadAssembly", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(string) }, null),
                    new Func<Func<string, Assembly>, string, Assembly>((orig, path) => {
                        if (path.EndsWith("MonoMod.RuntimeDetour.dll"))
                            loadedRuntimeDetour = true;
                        else if (path.EndsWith("MonoMod.RuntimeDetour.HookGen.exe") && !loadedRuntimeDetour) {
                            Console.WriteLine("HACKFIX: Loading MonoMod.RuntimeDetour.dll before MonoMod.RuntimeDetour.HookGen.exe");
                            orig(path.Substring(0, path.Length - 4 - 8) + ".dll");
                        }
                        return orig(path);
                    })
                );

                TextReader origReader = Console.In;
                using (TextWriter fileWriter = new MiniInstallerBridgeWriter(bridge))
                using (LogWriter logWriter = new LogWriter {
                    STDOUT = Console.Out,
                    File = fileWriter
                })
                using (TextReader fakeReader = new MiniInstallerFakeInReader()) {
                    Console.SetOut(logWriter);
                    Console.SetIn(fakeReader);

                    object exitObject = installerAssembly.EntryPoint.Invoke(null, new object[] { new string[] { } });
                    if (exitObject != null && exitObject is int && ((int) exitObject) != 0)
                        throw new Exception($"Return code != 0, but {exitObject}");

                    Console.SetOut(logWriter.STDOUT);
                    logWriter.STDOUT = null;
                    Console.SetIn(origReader);
                }

                hookReadKey.Undo();
                hookLazyLoadAssembly.Undo();
            }
        }

        class MiniInstallerBridge : MarshalByRefObject {
            public Encoding Encoding { get; set; }
            public string Root { get; set; }

            public void Write(string value) => Console.Out.Write(value);
            public void WriteLine(string value) => Console.Out.WriteLine(value);
            public void Write(char value) => Console.Out.Write(value);
            public void Write(char[] buffer, int index, int count) => Console.Out.Write(buffer, index, count);
            public void Flush() => Console.Out.Flush();
            public void Close() {
                // Console.Out.Close();
            }
        }

        class MiniInstallerBridgeWriter : TextWriter {
            private MiniInstallerBridge Bridge;
            public MiniInstallerBridgeWriter(MiniInstallerBridge bridge) {
                Bridge = bridge;
            }
            public override Encoding Encoding => Bridge.Encoding;
            public override void Write(string value) => Bridge.Write(value);
            public override void WriteLine(string value) => Bridge.WriteLine(value);
            public override void Write(char value) => Bridge.Write(value);
            public override void Write(char[] buffer, int index, int count) => Bridge.Write(buffer, index, count);
            public override void Flush() => Bridge.Flush();
            public override void Close() => Bridge.Close();
        }

        class MiniInstallerFakeInReader : TextReader {
            public override int Peek() => 1;
            public override int Read() => '\n';
            public override string ReadToEnd() => "\n";
        }

    }
}
