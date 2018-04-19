using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MonoMod.Installer {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            if (File.Exists("installer-log.txt"))
                File.Delete("installer-log.txt");
            using (Stream fileStream = File.OpenWrite("installer-log.txt"))
            using (StreamWriter fileWriter = new StreamWriter(fileStream, Console.OutputEncoding))
            using (LogWriter logWriter = new LogWriter {
                STDOUT = Console.Out,
                File = fileWriter
            }) {
                Console.SetOut(logWriter);

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm(new Everest.EverestInfo()));

                Console.SetOut(logWriter.STDOUT);
                logWriter.STDOUT = null;
            }
        }
    }
}
