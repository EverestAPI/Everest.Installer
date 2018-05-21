using Microsoft.Win32;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MonoMod.Installer {
    public class LocalFinder : GameFinder {

        public override string ID => "local";
        public override int Priority => -1000;

        public override string FindGameDir(string gameid) {
            string path = Environment.CurrentDirectory;
            if (File.Exists(Path.Combine(path, gameid)))
                return path;
            return null;
        }

    }
}
