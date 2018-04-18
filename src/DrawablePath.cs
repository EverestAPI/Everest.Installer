using Svg;
using Svg.Pathing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace MonoMod.Installer {
    public class DrawablePath : Drawable {

        public GraphicsPath Path;
        public Brush Brush;
        public Pen Pen;

        public override void Draw(AnimationManager animMan, Graphics g) {
            if (Path == null)
                return;
            if (Brush != null)
                g.FillPath(Brush, Path);
            if (Pen != null)
                g.DrawPath(Pen, Path);
        }

        public override void Dispose() {
            Path?.Dispose();
            Brush?.Dispose();
            Pen?.Dispose();
        }

        public override object Clone() {
            return new DrawablePath {
                Path = Path?.Clone() as GraphicsPath,
                Brush = Brush?.Clone() as Brush,
                Pen = Pen?.Clone() as Pen
            };
        }

        public static DrawablePath FromDotgridSVG(byte[] data, Brush brush = null, Pen pen = null, bool flatten = true) {
            XmlDocument xml = new XmlDocument();
            using (MemoryStream ms = new MemoryStream(data))
            using (StreamReader reader = new StreamReader(ms, Encoding.UTF8)) {
                xml.Load(reader);
            }

            SvgDocument svg = SvgDocument.Open(xml);
            SvgPath layer = svg.GetElementById<SvgPath>("layer_1");

            if (pen != null) {
                pen.Width = layer.StrokeWidth;
                switch (layer.StrokeLineCap) {
                    case SvgStrokeLineCap.Butt:
                        // This is even the case when it should be square.
                        pen.StartCap = LineCap.Square;
                        pen.DashCap = DashCap.Flat;
                        pen.EndCap = LineCap.Square;
                        break;
                    case SvgStrokeLineCap.Round:
                        pen.StartCap = LineCap.Round;
                        pen.DashCap = DashCap.Round;
                        pen.EndCap = LineCap.Round;
                        break;
                    case SvgStrokeLineCap.Square:
                        pen.StartCap = LineCap.Square;
                        pen.DashCap = DashCap.Flat;
                        pen.EndCap = LineCap.Square;
                        break;
                }
            }

            GraphicsPath path = svg.Path;
            Matrix fix = new Matrix();
            fix.Translate(-svg.Width / 2f, -svg.Height / 2f);
            path.Transform(fix);
            if (flatten)
                path.Flatten();

            return new DrawablePath {
                Path = path,
                Brush = brush,
                Pen = pen
            };
        }

        public DrawablePolygon ToPolygon(bool disposePath = true) {
            DrawablePolygon shape = new DrawablePolygon {
                Points = Path?.PathPoints,
                Brush = Brush,
                Pen = Pen,
                PenClosed = false
            };
            if (disposePath)
                Path?.Dispose();
            return shape;
        }

        public DrawableMulti ToMultiPath(bool disposePath = true) {
            DrawableMulti multi = new DrawableMulti();

            if (Path == null)
                return null;

            using (GraphicsPathIterator iterator = new GraphicsPathIterator(Path)) {
                GraphicsPath subpath = new GraphicsPath();
                bool closed;
                while (iterator.NextSubpath(subpath, out closed) != 0) {
                    DrawablePath shape = new DrawablePath {
                        Path = subpath,
                        Brush = Brush,
                        Pen = Pen
                    };
                    multi.Shapes.Add(shape);
                    subpath = new GraphicsPath();
                }
                subpath.Dispose();
            }

            if (disposePath)
                Path?.Dispose();

            return multi;
        }

        public DrawableMulti ToMultiPolygon(bool disposePath = true) {
            DrawableMulti multi = new DrawableMulti();

            if (Path == null)
                return null;

            using (GraphicsPathIterator iterator = new GraphicsPathIterator(Path)) {
                GraphicsPath subpath = new GraphicsPath();
                bool closed;
                while (iterator.NextSubpath(subpath, out closed) != 0) {
                    DrawablePolygon shape = new DrawablePolygon {
                        Points = subpath.PathPoints,
                        Brush = Brush,
                        Pen = Pen,
                        PenClosed = closed
                    };
                    multi.Shapes.Add(shape);
                }
                subpath.Dispose();
            }

            if (disposePath)
                Path?.Dispose();

            return multi;
        }

        public delegate Drawable MultiGen(int i, GraphicsPath path, Brush brush, Pen pen, bool closed);
        public DrawableMulti ToMulti(MultiGen cb, bool disposePath = true) {
            DrawableMulti multi = new DrawableMulti();

            if (Path == null)
                return null;

            using (GraphicsPathIterator iterator = new GraphicsPathIterator(Path)) {
                GraphicsPath subpath = new GraphicsPath();
                bool closed;
                int i = 0;
                while (iterator.NextSubpath(subpath, out closed) != 0) {
                    Drawable shape = cb(i++, subpath, Brush, Pen, closed);
                    if (shape != null)
                        multi.Shapes.Add(shape);
                }
                subpath.Dispose();
            }

            if (disposePath)
                Path?.Dispose();

            return multi;
        }

        public PointF[] ToPoints(bool dispose = true) {
            PointF[] points = Path?.PathPoints;
            if (dispose)
                Dispose();
            return points;
        }

    }
}
