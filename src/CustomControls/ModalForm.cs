using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace MonoMod.Installer.CustomControls {
    public partial class ModalForm : CustomPanel {
        
        public static Color DefaultModalBackColor = Color.FromArgb(255, 45, 45, 45);
        public static Color DefaultModalForeColor = Color.FromArgb(255, 255, 255, 255);

        public static Color DefaultButtonColorBorderFocused = Color.FromArgb(255, 21, 116, 180);
        public Color ColorButtonBorderFocused = DefaultButtonColorBorderFocused;

        protected ModalForm()
            : this(null, null, null, null, "OK") {
        }

        public ModalForm(string text, string caption, Image icon, params string[] buttons)
            : base() {
            BackColor = DefaultModalBackColor;
            ForeColor = DefaultModalForeColor;

            SuspendLayout();

            Margin = new Padding(0, 64, 0, 64);
            Size = new Size(460, 128);

            ResumeLayout(false);
        }

        public Rectangle CalculateBounds(Rectangle fit)
            => new Rectangle(
                fit.X,
                (int) (fit.Y + fit.Height * 0.5 - Height * 0.5),
                fit.Width,
                Height
            );

        public Rectangle UpdateOwnerBounds(Rectangle owner) {
            Rectangle prev = CalculateBounds(owner);
            return new Rectangle(
                owner.Left + Bounds.Left - prev.Left,
                owner.Top + Bounds.Top - prev.Top,
                owner.Width,
                owner.Height
            );
        }

    }
}
