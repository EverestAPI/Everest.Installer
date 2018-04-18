using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MonoMod.Installer {
    public static class FormsExt {

        public static Bitmap Scaled(this Image img, float scale) {
            return new Bitmap(img, (int) (img.Width * scale), (int) (img.Height * scale));
        }

        public static Bitmap ScaledBlur(this Image img, float scale, PixelFormat? format = null) {
            Bitmap bmp = new Bitmap((int) (img.Width * scale), (int) (img.Height * scale), format ?? img.PixelFormat);
            using (Graphics g = Graphics.FromImage(bmp))
                g.DrawImage(img, 0, 0, bmp.Width, bmp.Height);
            return bmp;
        }

        public static void DrawBackgroundImage(this Graphics g, Image img, int width, int height, int imgWidth = 0, int imgHeight = 0, ImageLayout layout = ImageLayout.Center, float offsX = 0f, float offsY = 0f) {
            if (imgWidth == 0)
                imgWidth = img.Width;
            if (imgHeight == 0)
                imgHeight = img.Height;

            if (layout == ImageLayout.Center) {
                g.DrawImage(img,
                    width * 0.5f - imgWidth * 0.5f + offsX,
                    height * 0.5f - imgHeight * 0.5f + offsY,
                    imgWidth,
                    imgHeight
                );
            } else if (layout == ImageLayout.Zoom) {
                g.DrawImageUnscaled(img,
                    (int) (width * 0.5f - imgWidth * 0.5f + offsX),
                    (int) (height * 0.5f - imgHeight * 0.5f + offsY)
                );
            } else if (layout == ImageLayout.Stretch) {
                g.DrawImage(img,
                    -offsX,
                    -offsY,
                    width + offsX,
                    height + offsY
                );
            }

        }

        public static Color Lerp(this Color a, Color b, float t) {
            return Color.FromArgb(
                (int) (a.A + (b.A - a.A) * t),
                (int) (a.R + (b.R - a.R) * t),
                (int) (a.G + (b.G - a.G) * t),
                (int) (a.B + (b.B - a.B) * t)
            );
        }

        public static Control GetDeepChildAtPoint(this Control c, Point p) {
            Control f;
            do {
                f = c.GetChildAtPoint(p);
                if (f == null)
                    return c;
                p = new Point(
                    p.X - f.Left,
                    p.Y - f.Top
                );
                c = f;
            } while (true);

        }

        public static void ForEachDeep(this Control c, Action<Control> a) {
            a(c);
            foreach (Control cc in c.Controls)
                cc.ForEachDeep(a);
        }
        public static void ForEachDeep(this Control.ControlCollection c, Action<Control> a) {
            foreach (Control cc in c)
                cc.ForEachDeep(a);
        }

        public static void ForEachDeep(this Control c, Func<Control, bool> b, Action<Control> a) {
            if (b(c))
                a(c);
            foreach (Control cc in c.Controls)
                cc.ForEachDeep(b, a);
        }
        public static void ForEachDeep(this Control.ControlCollection c, Func<Control, bool> b, Action<Control> a) {
            foreach (Control cc in c)
                cc.ForEachDeep(b, a);
        }

    }
}
