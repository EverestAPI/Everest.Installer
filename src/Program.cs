using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
            string log = Path.GetFullPath("installer-log.txt");
            if (args.Length >= 2 && args[0] == "--log") {
                StringBuilder logBuilt = new StringBuilder();
                for (int i = 1; i < args.Length; i++)
                    logBuilt.Append(args[i]).Append(" ");
                log = logBuilt.ToString().Trim();
            }

            string asmLoc = Assembly.GetEntryAssembly().Location;
            string asmNameWanted = Assembly.GetExecutingAssembly().GetName().Name + ".exe";
            if (Path.GetFileName(asmLoc) != asmNameWanted) {
                string asmTmp = Path.Combine(Path.GetTempPath(), asmNameWanted);
                File.Copy(asmLoc, asmTmp);
                Process.Start(asmTmp, "--log " + log);
                return;
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

                try {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new MainForm(info));
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
