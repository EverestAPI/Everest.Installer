using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MonoMod.Installer {
    public static class GameFinderManager {

        private readonly static Type[] _EmptyTypeArray = new Type[0];
        private readonly static object[] _EmptyObjectArray = new object[0];

        public readonly static List<GameFinder> Finders = new List<GameFinder>();

        static GameFinderManager() {
            Type[] types = Assembly.GetExecutingAssembly().GetTypes();
            for (int i = 0; i < types.Length; i++) {
                Type type = types[i];
                if (!typeof(GameFinder).IsAssignableFrom(type) || type.IsAbstract)
                    continue;
                Finders.Add((GameFinder) type.GetConstructor(_EmptyTypeArray).Invoke(_EmptyObjectArray));
            }
        }

        public static string Find(GameModInfo info) {
            Dictionary<string, string> gameids = info.GameIDs;
            string exedir = info.ExecutableDir;
            string exename = info.ExecutableName;

            for (int i = 0; i < Finders.Count; i++) {
                GameFinder finder = Finders[i];
                string s;
                if (!gameids.TryGetValue(finder.ID, out s))
                    continue;
                if ((s = finder.FindGameDir(s)) != null) {
                    if (!string.IsNullOrEmpty(exedir))
                        s = Path.Combine(s, exedir);
                    if (!string.IsNullOrEmpty(exename))
                        s = Path.Combine(s, exename);
                    if (File.Exists(s) || Directory.Exists(s))
                        return s;
                }
            }

            return null;
        }

    }
}
