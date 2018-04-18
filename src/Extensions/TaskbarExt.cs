﻿using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MonoMod.Installer {
    // Taken from http://stackoverflow.com/a/24187171
    public static class TaskbarExt {

        public enum TBPF {
            TBPF_NOPROGRESS = 0,
            TBPF_INDETERMINATE = 0x1,
            TBPF_NORMAL = 0x2,
            TBPF_ERROR = 0x4,
            TBPF_PAUSED = 0x8
        }

        public enum TBATF {
            TBATF_USEMDITHUMBNAIL = 0x1,
            TBATF_USEMDILIVEPREVIEW = 0x2
        }

        public enum THB : uint {
            THB_BITMAP = 0x1,
            THB_ICON = 0x2,
            THB_TOOLTIP = 0x4,
            THB_FLAGS = 0x8
        }

        public enum THBF : uint {
            THBF_ENABLED = 0,
            THBF_DISABLED = 0x1,
            THBF_DISMISSONCLICK = 0x2,
            THBF_NOBACKGROUND = 0x4,
            THBF_HIDDEN = 0x8
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Auto)]
        public struct THUMBBUTTON {
            public THB dwMask;
            public uint iId;
            public uint iBitmap;
            public IntPtr hIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] // MAX_PATH
            public string szTip;
            public THBF dwFlags;
        }

        [ComImport]
        [Guid("ea1afb91-9e28-4b86-90e9-9e9f8a5eefaf")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface ITaskbarList3 {
            // ITaskbarList
            void HrInit();
            void AddTab(IntPtr hwnd);
            void DeleteTab(IntPtr hwnd);
            void ActivateTab(IntPtr hwnd);
            void SetActiveAlt(IntPtr hwnd);

            // ITaskbarList2
            void MarkFullscreenWindow(
                IntPtr hwnd,
                [MarshalAs(UnmanagedType.Bool)] bool fFullscreen);

            // ITaskbarList3
            void SetProgressValue(IntPtr hwnd, ulong ullCompleted, ulong ullTotal);
            void SetProgressState(IntPtr hwnd, TBPF tbpFlags);
            void RegisterTab(IntPtr hwndTab, IntPtr hwndMDI);
            void UnregisterTab(IntPtr hwndTab);
            void SetTabOrder(IntPtr hwndTab, IntPtr hwndInsertBefore);
            void SetTabActive(IntPtr hwndTab, IntPtr hwndMDI, TBATF tbatFlags);

            void ThumbBarAddButtons(
                IntPtr hwnd,
                uint cButtons,
                [MarshalAs(UnmanagedType.LPArray)] THUMBBUTTON[] pButtons);

            void ThumbBarUpdateButtons(
                IntPtr hwnd,
                uint cButtons,
                [MarshalAs(UnmanagedType.LPArray)] THUMBBUTTON[] pButtons);

            void ThumbBarSetImageList(IntPtr hwnd, IntPtr himl);

            void SetOverlayIcon(
                IntPtr hwnd,
                IntPtr hIcon,
                [MarshalAs(UnmanagedType.LPWStr)] string pszDescription);

            void SetThumbnailTooltip(
                IntPtr hwnd,
                [MarshalAs(UnmanagedType.LPWStr)] string pszTip);

            void SetThumbnailClip(
                IntPtr hwnd,
                [MarshalAs(UnmanagedType.LPStruct)] Rectangle prcClip);
        }

        [ComImport()]
        [Guid("56fdf344-fd6d-11d0-958a-006097c9a090")]
        [ClassInterface(ClassInterfaceType.None)]
        private class TaskbarInstance {
        }

        private readonly static ITaskbarList3 _Instance = (ITaskbarList3) new TaskbarInstance();
        private readonly static bool _Supported = Type.GetType("Mono.Runtime") == null && Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version >= new Version(6, 1);

        public static void SetProgressState(this Form form, TBPF state) {
            if (_Supported)
                _Instance.SetProgressState(form.Handle, state);
        }

        public static void SetProgressValue(this Form form, ulong value, ulong max) {
            if (_Supported)
                _Instance.SetProgressValue(form.Handle, value, max);
        }

        public static void SetOverlayIcon(this Form form, Icon icon, string description = "") {
            if (_Supported)
                _Instance.SetOverlayIcon(form.Handle, icon.Handle, description);
        }

    }
}
