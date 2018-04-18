using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace MonoMod.Installer {
    public class DrawablePolygon : Drawable {

        public PointF[] Points;
        public Brush Brush;
        public Pen Pen;
        public bool PenClosed = true;

        public override void Draw(AnimationManager animMan, Graphics g) {
            if (Points == null)
                return;
            if (Brush != null)
                g.FillPolygon(Brush, Points);
            if (Pen != null) {
                if (PenClosed) {
                    g.DrawPolygon(Pen, Points);
                } else {
                    g.DrawLines(Pen, Points);
                }
            }
        }

        public override void Dispose() {
            Brush?.Dispose();
            Pen?.Dispose();
        }

        public override object Clone() {
            return new DrawablePolygon {
                Points = Points?.Clone() as PointF[],
                Brush = Brush?.Clone() as Brush,
                Pen = Pen?.Clone() as Pen,
                PenClosed = PenClosed
            };
        }

        public DrawablePolygon RemoveDupes() {
            if (Points == null || Points.Length < 2)
                return this;

            List<PointF> points = new List<PointF>(Points.Length);
            PointF p;
            PointF last = Points[0];
            points.Add(last);
            for (int i = 1; i < Points.Length; i++) {
                p = Points[i];
                if (p == last)
                    continue;
                last = p;
                points.Add(p);
            }

            Points = points.ToArray();
            return this;
        }

        public override void Reverse() {
            if (Points == null || Points.Length < 2)
                return;
            List<PointF> points = new List<PointF>(Points);
            points.Reverse();
            Points = points.ToArray();
        }

    }
}
