using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace MonoMod.Installer {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args) {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            const string logDefaultName = "installer-log.txt";
            string log = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), logDefaultName);

            Queue<string> argsQueue = new Queue<string>(args);
            Queue<string> argsLateQueue = new Queue<string>();
            while (argsQueue.Count > 0) {
                string arg = argsQueue.Dequeue();

                if (arg == "--log" && argsQueue.Count >= 1) {
                    StringBuilder logBuilt = new StringBuilder();
                    while (argsQueue.Count > 0)
                        logBuilt.Append(argsQueue.Dequeue()).Append(" ");
                    log = logBuilt.ToString().Trim();

                } else {
                    argsLateQueue.Enqueue(arg);
                }
            }
            argsQueue = argsLateQueue;

            // If the assembly name doesn't match, re-run the installer from a temporary location.
            string asmLoc = Assembly.GetEntryAssembly().Location;
            string asmNameWanted = Assembly.GetExecutingAssembly().GetName().Name + ".exe";
            if (Path.GetFileName(asmLoc) != asmNameWanted) {
                string asmTmp = Path.Combine(Path.GetTempPath(), asmNameWanted);
                File.Copy(asmLoc, asmTmp, true);
                Process.Start(asmTmp, "--log " + log);
                return;
            }

            // Check if log is writable, otherwise write log to user dir.
            try {
                using (Stream tmpStream = File.OpenWrite(log)) {
                }
            } catch {
                log = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), logDefaultName);
            }

            if (File.Exists(log))
                File.Delete(log);
            using (Stream fileStream = File.OpenWrite(log))
            using (StreamWriter fileWriter = new StreamWriter(fileStream, Console.OutputEncoding))
            using (LogWriter logWriter = new LogWriter {
                STDOUT = Console.Out,
                File = fileWriter
            }) {
                Console.SetOut(logWriter);

                GameModInfo info = new Everest.EverestInfo();
                string protocol = info.ModURIProtocol;
                if (!string.IsNullOrEmpty(protocol))
                    protocol = protocol + ":";

                Console.WriteLine($"{info.ModInstallerName} v{Assembly.GetEntryAssembly().GetName().Version}");

                try {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);

                    MainForm form = new MainForm(info);

                    while (argsQueue.Count > 0) {
                        string arg = argsQueue.Dequeue();

                        if (arg.ToLowerInvariant().StartsWith(protocol, StringComparison.InvariantCultureIgnoreCase)) {
                            arg = arg.Substring(info.ModURIProtocol.Length + 1);
                            
                            if (arg.StartsWith("http://") || arg.StartsWith("https://")) {
                                // Automatic mod .zip download.
                                form.AutoDownloadMod = arg.Split(',')[0];
                            }
                        }
                    }

                    Application.Run(form);
                } catch (Exception e) {
                    Console.WriteLine(e);
                    if (Debugger.IsAttached) {
                        throw;
                    } else {
                        MessageBox.Show($"{info.ModInstallerName} has encountered a critical error.\nPlease submit your installer-log.txt\nIt's located next to the installer .exe", info.ModInstallerName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                Console.SetOut(logWriter.STDOUT);
                logWriter.STDOUT = null;
            }
        }
    }
}
