using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MonoMod.Installer.CustomControls {
    public partial class ModalForm {

        public static ModalForm Create(Form owner, string text, string caption = null, Image icon = null, params string[] buttons) {
            if (buttons == null || buttons.Length == 0)
                buttons = new string[] { "OK" };

            ModalForm modal = new ModalForm(text, caption, icon, buttons);
            owner.Controls.Add(modal);
            return modal;
        }

    }
}
