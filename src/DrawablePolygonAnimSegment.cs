using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace MonoMod.Installer {
    public class DrawablePolygonAnimSegment : Drawable {

        public DrawablePolygon Polygon;

        public float Progress = 1f;
        public float Time;
        public bool Shrink = false;

        public float? Fade = 1f;
        public float[] Timing;

        public float TimeDelay = 0f;
        public float TimeScale = 1f;

        private float _Progress;
        private PointF[] _Points;

        public override bool IsActive => !Shrink || (Shrink && Progress >= 1f);

        public override void Update(AnimationManager animMan) {
            Polygon?.Update(animMan);
            if (Polygon?.Points == null)
                return;

            if (Timing != null) {
                float t = Math.Max(0f, Math.Min((Time - Timing[0] * TimeScale - TimeDelay) / Timing[1] / TimeScale, 1f));
                Progress = (float) Math.Sin(t * Math.PI / 2f);
                if (Shrink)
                    Progress = 1f - Progress;
                Time += animMan.DeltaTime;
            }

            int lengthShort = (int) Math.Floor(1 + (Polygon.Points.Length - 1) * Progress);
            int lengthLong = Math.Min(lengthShort + 1, Polygon.Points.Length);
            int lengthArr = lengthLong;
            if (lengthLong <= 1)
                lengthArr = 2;
            if (_Points == null || _Points.Length != lengthArr) {
                _Points = new PointF[lengthArr];
            }

            if (_Progress != Progress) {
                _Progress = Progress;
                animMan.Repaint = true;
            }

            if (lengthLong <= 1) {
                PointF p = Polygon.Points[0];
                _Points[0] = p;
                _Points[1] = new PointF(
                    p.X + 0.001f,
                    p.Y
                );

                return;
            }

            Array.Copy(Polygon.Points, _Points, lengthShort);

            float endStep = (1 + (Polygon.Points.Length - 1) * Progress) - lengthShort;
            PointF a = Polygon.Points[lengthShort - 1];
            PointF b = Polygon.Points[lengthLong - 1];
            _Points[lengthLong - 1] = new PointF(
                a.X + (b.X - a.X) * endStep + 0.001f,
                a.Y + (b.Y - a.Y) * endStep
            );
        }

        public override void Draw(AnimationManager animMan, Graphics g) {
            if (Polygon?.Points == null)
                return;
            if (_Points == null)
                Update(animMan);
            if (_Points == null || _Points.Length <= 1)
                return;
            if (Polygon.Brush != null) {
                SolidBrush brush = Polygon.Brush as SolidBrush;
                if (Fade != null && brush != null) {
                    brush.Color = Color.FromArgb(
                        Math.Min(255, (int) Math.Round(255f * Fade.Value * Progress * 3f)),
                        brush.Color.R,
                        brush.Color.G,
                        brush.Color.B
                    );
                }
                g.FillPolygon(Polygon.Brush, _Points);
            }
            if (Polygon.Pen != null) {
                if (Fade != null) {
                    Polygon.Pen.Color = Color.FromArgb(
                        Math.Min(255, (int) Math.Round(255f * Fade.Value * Progress * 3f)),
                        Polygon.Pen.Color.R,
                        Polygon.Pen.Color.G,
                        Polygon.Pen.Color.B
                    );
                }
                if (Polygon.PenClosed) {
                    g.DrawPolygon(Polygon.Pen, _Points);
                } else {
                    g.DrawLines(Polygon.Pen, _Points);
                }
            }
        }

        public override void Dispose() {
            Polygon?.Dispose();
        }

        public override object Clone() {
            return new DrawablePolygonAnimSegment {
                Polygon = Polygon?.Clone() as DrawablePolygon,

                Progress = Progress,
                Time = 0f,
                Shrink = Shrink,

                Fade = Fade,
                Timing = Timing?.Clone() as float[],

                TimeDelay = TimeDelay,
                TimeScale = TimeScale
            };
        }

        public DrawablePolygonAnimSegment Reverse() {
            Polygon.Reverse();
            Shrink = !Shrink;
            return this;
        }

    }
}
