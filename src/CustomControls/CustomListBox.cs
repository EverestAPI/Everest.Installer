using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MonoMod.Installer {
    public class CustomListBox : ListBox {

        protected override CreateParams CreateParams {
            get {
                CreateParams cp = base.CreateParams;
                // cp.ExStyle |= 0x00000020; // WS_EX_TRANSPARENT
                cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED
                return cp;
            }
        }

        [Category("Appearance")]
        public Color HighlightBackColor { get; set; } = SystemColors.Highlight;
        [Category("Appearance")]
        public Color HighlightForeColor { get; set; } = SystemColors.HighlightText;

        public CustomListBox()
            : base() {
            SetStyle(ControlStyles.UserPaint, false);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            DoubleBuffered = true;

            DrawMode = DrawMode.OwnerDrawVariable;
            MeasureItem += _MeasureItem;
            DrawItem += _DrawItem;
        }

        private void _MeasureItem(object sender, MeasureItemEventArgs e) {
            e.ItemHeight = Font.Height;
        }

        private void _DrawItem(object sender, DrawItemEventArgs e) {
            if (e.Index < 0 || Items.Count <= e.Index)
                return;

            if (!e.Bounds.IntersectsWith(Bounds))
                return;

            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                e = new DrawItemEventArgs(
                    e.Graphics,
                    e.Font,
                    e.Bounds,
                    e.Index,
                    e.State ^ DrawItemState.Selected,
                    HighlightForeColor,
                    HighlightBackColor
                );

            e.DrawBackground();

            using (Brush fg = new SolidBrush(e.ForeColor))
                e.Graphics.DrawString(
                    Items[e.Index].ToString(),
                    e.Font,
                    fg,
                    e.Bounds,
                    StringFormat.GenericDefault
                );

            e.DrawFocusRectangle();
        }

    }
}
