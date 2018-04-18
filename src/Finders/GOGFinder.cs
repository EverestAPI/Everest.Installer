using Microsoft.Win32;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MonoMod.Installer {
    public class GOGFinder : GameFinder {

        public override string ID => "gog";

        public override string FindGameDir(string gameid) {
            if ((PlatformHelper.Current & Platform.Windows) != Platform.Windows)
                return null;
            string regKey =
                (PlatformHelper.Current & Platform.X64) == Platform.X64 ?
                $@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\GOG.com\Games\{gameid}" :
                $@"HKEY_LOCAL_MACHINE\SOFTWARE\GOG.com\Games\{gameid}";
            // Win32 is case-insensitive.
            string path = (string) Registry.GetValue(regKey, "exe", null);
            if (!File.Exists(path))
                return null;
            return Path.GetDirectoryName(path);
        }

    }
}
