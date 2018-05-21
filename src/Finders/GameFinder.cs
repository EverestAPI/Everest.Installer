using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoMod.Installer {
    public abstract class GameFinder {

        public abstract string ID { get; }
        public virtual int Priority => 0;

        public abstract string FindGameDir(string gameid);

    }
}
