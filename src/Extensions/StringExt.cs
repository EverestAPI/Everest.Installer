using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MonoMod.Installer {
    public static class StringExt {

        public static string NormalizeFilepath(this string s) {
            return s.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        }

    }
}
