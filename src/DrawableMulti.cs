using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace MonoMod.Installer {
    public class DrawableMulti : Drawable {

        public List<Drawable> Shapes = new List<Drawable>();

        public override bool IsActive {
            get {
                if (Shapes == null)
                    return false;
                for (int i = 0; i < Shapes.Count; i++) {
                    if (Shapes[i]?.IsActive ?? false)
                        return true;
                }
                return false;
            }
        }

        public override void Update(AnimationManager animMan) {
            if (Shapes == null)
                return;
            for (int i = 0; i < Shapes.Count; i++) {
                Shapes[i]?.Update(animMan);
            }
        }

        public override void Draw(AnimationManager animMan, Graphics g) {
            if (Shapes == null)
                return;
            for (int i = 0; i < Shapes.Count; i++) {
                Shapes[i]?.Draw(animMan, g);
            }
        }

        public override void Dispose() {
            if (Shapes == null)
                return;
            for (int i = 0; i < Shapes.Count; i++) {
                Shapes[i]?.Dispose();
            }
        }

        public override object Clone() {
            DrawableMulti clone = new DrawableMulti();
            if (Shapes == null)
                return clone;
            clone.Shapes = new List<Drawable>(Shapes.Count);
            for (int i = 0; i < Shapes.Count; i++) {
                clone.Shapes.Add(Shapes[i]?.Clone() as Drawable);
            }
            return clone;
        }

        public override void Reverse() {
            if (Shapes == null)
                return;
            for (int i = 0; i < Shapes.Count; i++) {
                Shapes[i]?.Reverse();
            }
        }

        public override void Out() {
            if (Shapes == null)
                return;
            for (int i = 0; i < Shapes.Count; i++) {
                Shapes[i]?.Out();
            }
        }

    }
}
