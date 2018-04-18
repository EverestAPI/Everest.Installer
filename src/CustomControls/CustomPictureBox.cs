using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MonoMod.Installer {
    public class CustomPictureBox : PictureBox {

        protected override CreateParams CreateParams {
            get {
                CreateParams cp = base.CreateParams;
                // cp.ExStyle |= 0x00000020; // WS_EX_TRANSPARENT
                cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED
                return cp;
            }
        }

        public CustomPictureBox()
            : base() {
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            DoubleBuffered = true;
        }

        private const int WM_NCHITTEST = 0x0084;
        private readonly static IntPtr HTTRANSPARENT = new IntPtr(-1);

        protected override void WndProc(ref Message m) {
            if (m.Msg == WM_NCHITTEST && !DesignMode) {
                m.Result = HTTRANSPARENT;
                return;
            }

            base.WndProc(ref m);
        }

    }
}
