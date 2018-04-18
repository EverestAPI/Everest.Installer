using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace MonoMod.Installer {
    internal sealed class CachedInfo : GameModInfo {

        internal GameModInfo _From;

        internal string _GameName;
        public override string GameName => _GameName;

        internal string _ModName;
        public override string ModName => _ModName;

        internal string _ModInstallerName;
        public override string ModInstallerName => _ModInstallerName;

        internal Image _HeaderImage;
        public override Image HeaderImage => _HeaderImage;

        internal Image _BackgroundImage;
        public override Image BackgroundImage => _BackgroundImage;

        internal string _ExecutableDir;
        public override string ExecutableDir => _ExecutableDir;

        internal string _ExecutableName;
        public override string ExecutableName => _ExecutableName;

        internal string[] _Assemblies;
        public override string[] Assemblies => _Assemblies;

        internal ModBackup[] _Backups;
        public override ModBackup[] Backups => _Backups;

        internal Dictionary<string, string> _GameIDs;
        public override Dictionary<string, string> GameIDs => _GameIDs;

        internal string _CacheDir;
        public override string CacheDir => _CacheDir;

        internal List<ModVersion> _ModVersions;
        public override List<ModVersion> ModVersions {
            get {
                if (_ModVersions != null)
                    return _ModVersions;
                return _ModVersions = new List<ModVersion>(_From.ModVersions);
            }
        }

        public CachedInfo(GameModInfo from) {
            _From = from;
            _GameName = from.GameName;
            _ModName = from.ModName;
            _ModInstallerName = from.ModInstallerName;
            _HeaderImage = from.HeaderImage;
            _BackgroundImage = from.BackgroundImage;
            _ExecutableDir = from.ExecutableDir;
            _ExecutableName = from.ExecutableName;
            _Assemblies = from.Assemblies;
            _Backups = from.Backups;
            _GameIDs = from.GameIDs;
            _CacheDir = from.CacheDir;
        }

    }
}
