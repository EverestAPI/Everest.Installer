using Microsoft.Win32;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace MonoMod.Installer {
    public class LocalFinder : GameFinder {

        public override string ID => "local";
        public override int Priority => -1000;

        public override string FindGameDir(string gameid) {
            string path;
            if (File.Exists(Path.Combine(path = Environment.CurrentDirectory, gameid)))
                return path;
            if (File.Exists(Path.Combine(path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), gameid)))
                return path;
            return null;
        }

    }
}
