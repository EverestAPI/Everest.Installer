using Microsoft.Win32;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MonoMod.Installer {
    public class EpicFinder : GameFinder {

        // "" -> "
        public readonly static Regex DisplayNameRegex = new Regex(@"\s*""DisplayName"": ""(.*)"",", RegexOptions.Compiled);
        public readonly static Regex InstallLocationRegex = new Regex(@"\s*""InstallLocation"": ""(.*)"",", RegexOptions.Compiled);

        public override string ID => "epic";

        public override string FindGameDir(string gameid) {
            if ((PlatformHelper.Current & Platform.Windows) != Platform.Windows)
                return null;

            string regKey =
                (PlatformHelper.Current & Platform.X64) == Platform.X64 ?
                @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Epic Games\EpicGamesLauncher" :
                @"HKEY_LOCAL_MACHINE\SOFTWARE\Epic Games\EpicGamesLauncher";
            // Win32 is case-insensitive.
            string appdata = (string) Registry.GetValue(regKey, "AppDataPath", null);
            if (!Directory.Exists(appdata))
                return null;

            // path ends up pointing to f.e. C:\ProgramData\Epic\EpicGamesLauncher\Data\
            // Manifest .item (pretty much .json) files are stored in the Manifests subdir.
            // The filenames sadly aren't consistent across machines / accounts,
            // meaning that we'll need to crawl through them to find the right one.

            string manifests = Path.Combine(appdata, "Manifests");
            if (!Directory.Exists(manifests))
                return null;

            foreach (string manifest in Directory.GetFiles(manifests)) {
                if (!manifest.EndsWith(".item", StringComparison.InvariantCultureIgnoreCase))
                    continue;

                string displayName = null;
                string installLocation = null;

                using (StreamReader reader = new StreamReader(manifest))
                    while (!reader.EndOfStream) {
                        string line = reader.ReadLine().Trim();
                        Match match;
                        
                        match = DisplayNameRegex.Match(line);
                        if (match.Success)
                            displayName = Regex.Unescape(match.Groups[1].Value);

                        match = InstallLocationRegex.Match(line);
                        if (match.Success)
                            installLocation = Regex.Unescape(match.Groups[1].Value);
                    }

                if (!string.IsNullOrEmpty(displayName) && gameid.Equals(displayName, StringComparison.InvariantCultureIgnoreCase) &&
                    !string.IsNullOrEmpty(installLocation) && Directory.Exists(installLocation))
                    return installLocation;
            }

            return null;
        }

    }
}
