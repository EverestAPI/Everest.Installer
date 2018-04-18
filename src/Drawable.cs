using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace MonoMod.Installer {
    public abstract class Drawable : IDisposable, ICloneable {

        public virtual bool IsActive { get; protected set; } = true;

        public virtual void Update(AnimationManager animMan) {
        }

        public abstract void Draw(AnimationManager animMan, Graphics g);

        public abstract void Dispose();

        public abstract object Clone();

        public virtual void Reverse() => IsActive = !IsActive;
        public virtual void Out() => IsActive = false;

    }
}
