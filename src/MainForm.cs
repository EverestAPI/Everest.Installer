using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MonoMod.Installer {
    public partial class MainForm : Form {

        protected override CreateParams CreateParams {
            get {
                CreateParams cp = base.CreateParams;
                // cp.ExStyle |= 0x00000020; // WS_EX_TRANSPARENT
                // cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED
                // cp.ExStyle |= 0x00080000; // WS_EX_LAYERED

                cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED
                // cp.ExStyle |= 0x00080000; // WS_EX_LAYERED

                return cp;
            }
        }

        public readonly AnimationManager AnimationManager;

        public readonly static Random RNG = new Random();

        private Image BackgroundNoise;

        // public readonly static Color ColorBorderFocused = Color.FromArgb(255, 21, 116, 180);
        // public readonly static Color ColorBorderFocused = Color.FromArgb(255, 0x3B, 0x2D, 0x4A);
        public readonly static Color ColorBorderFocused = Color.FromArgb(255, 0xC1, 0xB9, 0xF6);

        private SolidBrush _BackgroundBrush = new SolidBrush(Color.FromArgb(31, 0x3B, 0x2D, 0x4A));
        private SolidBrush _FPSBrush = new SolidBrush(Color.FromArgb(31, 0xC1, 0xB9, 0xF6));
        private SolidBrush _VersionBrush = new SolidBrush(Color.FromArgb(31, 0xC1, 0xB9, 0xF6));

        public readonly GameModInfo Info;
        public readonly GameModder Modder;

        public InstallerStatus Status { get; private set; } = InstallerStatus.Selecting;

        public bool ShowFPS =
#if DEBUG
            true;
#else
            false;
#endif

        public Size BackgroundSize;
        public float BackgroundSizeFactor;

        private Stopwatch _Stopwatch;

        private PrivateFontCollection _Fonts;
        private Font _Font;

        private Animation _IntroSlideAnimation;

        private OpenFileDialog _ExeBrowseDialog;
        private OpenFileDialog _ModBrowseDialog;

        public readonly string VersionString;

        private Thread _ModVersionsThread;

        public MainForm(GameModInfo info) {
            Info = info;
            Modder = new GameModder(Info);

            InitializeComponent();

            SuspendLayout();

            MinimumSize = MaximumSize = Size = new Size((int) (460 * AutoScaleFactor.Width), (int) (600 * AutoScaleFactor.Height));

            Text = Info.ModInstallerName;
            MainStep1Label.Text = string.Format(MainStep1Label.Text, Info.ExecutableName);
            BackgroundImage = Info.BackgroundImage;
            HeaderPicture.Image = Info.HeaderImage;

            BackgroundNoise = Properties.Resources.noise;

            Version v = Assembly.GetEntryAssembly().GetName().Version;
#if !DEBUG
            VersionString = $"v{v.Major}.{v.Minor}.{v.Build}";
#else
            VersionString = $"DEBUG #{v.Revision}";
#endif

            ProgressShapes.Init();

            ResumeLayout(false);

            AnimationManager = new AnimationManager(this);
        }

        private void MainForm_Load(object _s, EventArgs _e) {
            _Stopwatch = new Stopwatch();
            _Stopwatch.Reset();
            _Stopwatch.Start();

            SuspendLayout();

            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            DoubleBuffered = true;

            ResizeRedraw = !AnimationManager.IsMono;

            ResizeBegin += (s, e) => AnimationManager.IsThrottled = true;
            ResizeEnd += (s, e) => AnimationManager.IsThrottled = false;

            _Fonts = new PrivateFontCollection();
            unsafe
            {
                byte[] data = Properties.Resources.selawksl;
                fixed (byte* dataptr = &data[0])
                    _Fonts.AddMemoryFont(new IntPtr(dataptr), data.Length);
            }
            _Font = new Font(_Fonts.Families[0], 12f);
            Controls.ForEachDeep(c => {
                c.Font = _Font;
                (c as Button)?.PrepareAnimations(ColorBorderFocused);
            });

            if (Environment.OSVersion.Platform == PlatformID.Win32NT &&
                AnimationManager.IsMono) {
                Application.ApplicationExit += (s, e) => {
                    // At least hides the "mono.exe crashed" window.
                    throw new Exception("Thanks, Windows Mono!");
                };
            }

            float backgroundF = 2048f / BackgroundImage.Width;
            BackgroundSize = new Size(
                (int) (BackgroundImage.Width * backgroundF),
                (int) (BackgroundImage.Height * backgroundF)
            );

            backgroundF = 1024f / BackgroundImage.Width;
            BackgroundImage = BackgroundImage.ScaledBlur(backgroundF, PixelFormat.Format24bppRgb);

            foreach (Panel panel in Controls) {
                if (panel != HeaderPanel && panel != MainPanel)
                    panel.Visible = false;
            }

            ResumeLayout(false);
            PerformLayout();

            if (AnimationManager.IsMono) {
                MouseDown += _DragStart;
                MouseMove += _DragMove;
                MouseUp += _DragEnd;
            }

            this.Animate((a, t) => Opacity = t, dur: 0.4f, smooth: true);
            this.Animate((a, t) => BackgroundSizeFactor = 1f + 8f * (1f - t), dur: 2f, easing: Easings.QuinticEaseOut, smooth: true);

            HeaderPanel.SlideIn(delay: 0.05f);

            _IntroSlideAnimation = MainPanel.SlideIn(delay: 0.1f);

            this.Animate(UpdateBackground, loop: true, smooth: false, repaint: false);

            Info.OnChangeCurrentExecutablePath += (info, path) => {
                MainPathBox.Text = path ?? "";
                bool valid = !string.IsNullOrEmpty(path);
                MainInstallButton.Enabled = valid;
                MainUninstallButton.Enabled = valid;
            };
            Info.OnChangeCurrentStatus += (info, status) => {
                MainExeStatusLabel.Text = status ?? "";
            };
            Info.CurrentExecutablePath = GameFinderManager.Find(Info);

            _ExeBrowseDialog = new OpenFileDialog() {
                Title = $"Select {Info.ExecutableName}",
                AutoUpgradeEnabled = true,
                CheckFileExists = true,
                CheckPathExists = true,
                ValidateNames = true,
                Multiselect = false,
                ShowReadOnly = false,
                Filter = $"{Info.ExecutableName}|{Info.ExecutableName}",
                FilterIndex = 0
            };
            _ExeBrowseDialog.FileOk += (object s, CancelEventArgs e) => {
                Info.CurrentExecutablePath = _ExeBrowseDialog.FileNames[0];
            };

            _ModBrowseDialog = new OpenFileDialog() {
                Title = $"Select {Info.ModName}",
                AutoUpgradeEnabled = true,
                CheckFileExists = true,
                CheckPathExists = true,
                ValidateNames = true,
                Multiselect = false,
                ShowReadOnly = false,
                Filter = $"{Info.ModName} (*.zip)|*.zip",
                FilterIndex = 0
            };
            _ModBrowseDialog.FileOk += (object s, CancelEventArgs e) => {
                Info.CurrentInstallingModVersion = new GameModInfo.ModVersion { Name = "Custom .zip", URL = "|local|" + _ModBrowseDialog.FileNames[0] };
                _PreviousModVersionIndex = MainVersionList.SelectedIndex;
            };

            _ModVersionsThread = new Thread(_DownloadModVersions);
            _ModVersionsThread.IsBackground = true;
            _ModVersionsThread.Start();

            /*
            this.SetProgressState(TaskbarExt.TBPF.TBPF_NORMAL);
            this.SetProgressValue(5, 10);
            this.SetOverlayIcon(Icon);
            */
        }


        private PointF _BackgroundOffs;
        private long _FrameStart;
        private long _FrameStartPrev;
        private float _CurrentFrameTime;

        private InstallerStatus _ProgressStatus;
        private Drawable _ProgressShapePrevious;
        private Drawable _ProgressShapeCurrent;

        public void UpdateBackground(Animation a, float t) {
            Point cursor = PointToClient(Cursor.Position);
            _BackgroundOffs = new PointF(
                cursor.X - Width / 2f,
                cursor.Y - Height / 2f
            );

            if (_ProgressStatus != Status) {
                _ProgressShapePrevious?.Dispose();
                _ProgressShapePrevious = _ProgressShapeCurrent;
                _ProgressShapePrevious?.Reverse();
                switch (Status) {
                    default:
                        _ProgressShapeCurrent = null;
                        break;
                    case InstallerStatus.Downloading:
                        _ProgressShapeCurrent = ProgressShapes.Download;
                        break;
                    case InstallerStatus.Installing:
                        _ProgressShapeCurrent = ProgressShapes.Installing;
                        break;
                    case InstallerStatus.Uninstalling:
                        _ProgressShapeCurrent = ProgressShapes.Uninstalling;
                        break;
                    case InstallerStatus.Done:
                        _ProgressShapeCurrent = ProgressShapes.Done;
                        break;
                }
                _ProgressStatus = Status;
            }

            if (_ProgressShapePrevious != null && _ProgressShapePrevious.IsActive) {
                _ProgressShapePrevious.Update(AnimationManager);
            } else {
                if (_ProgressShapePrevious != null) {
                    _ProgressShapePrevious.Dispose();
                    _ProgressShapePrevious = null;
                }
                _ProgressShapeCurrent?.Update(AnimationManager);
            }

        }

        protected override void OnPaintBackground(PaintEventArgs e) {
            if (e.ClipRectangle.Size == Size) {
                _FrameStartPrev = _FrameStart;
                _FrameStart = _Stopwatch.ElapsedMilliseconds;
                _CurrentFrameTime = (_FrameStart - _FrameStartPrev) * 0.001f;
            }

            Graphics g = e.Graphics;

            g.CompositingQuality = CompositingQuality.HighSpeed;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.SmoothingMode = SmoothingMode.None;
 
            g.DrawBackgroundImage(
                BackgroundImage,
                Width,
                Height,
                (int) (BackgroundSize.Width * BackgroundSizeFactor),
                (int) (BackgroundSize.Height * BackgroundSizeFactor),
                ImageLayout.Center,
                _BackgroundOffs.X * -0.04f,
                _BackgroundOffs.Y * -0.04f
            );

            g.InterpolationMode = InterpolationMode.NearestNeighbor;

            g.DrawImage(BackgroundNoise, 0, 0, Width, Height);

            // g.FillRectangle(_BackgroundBrush, 1, 1, Width - 2, Height - 2);

            foreach (Panel panel in Controls)
                if (panel.Visible)
                    g.FillRectangle(_BackgroundBrush, panel.Left, panel.Top, panel.Width, panel.Height);

            SizeF versionSize = g.MeasureString(VersionString, _Font);
            g.DrawString(VersionString, _Font, _VersionBrush,
                HeaderPanel.Left + HeaderPanel.Width - versionSize.Width,
                MainPanel.Top + MainPanel.Height - versionSize.Height
            );

            if (ProgressPanel.Visible && _ProgressShapeCurrent != null) {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                Matrix transformPrev = g.Transform;

                g.TranslateTransform(
                    ProgressPanel.Left + Width * 0.5f + _BackgroundOffs.X * -0.02f,
                    Height * 0.5f + _BackgroundOffs.Y * -0.02f
                );

                _ProgressShapeCurrent.Draw(AnimationManager, g);

                g.Transform = transformPrev;
                g.SmoothingMode = SmoothingMode.HighSpeed;
            }

            if (ShowFPS) {
                g.DrawString((1f / AnimationManager.CurrentFrameTime).ToString("F3", System.Globalization.CultureInfo.InvariantCulture), _Font, _FPSBrush, 0, 0);
                g.DrawString((1f / _CurrentFrameTime).ToString("F3", System.Globalization.CultureInfo.InvariantCulture), _Font, _FPSBrush, 0, 14 * (AutoScaleFactor.Height));
            }

        }

        private void _DownloadModVersions() {
            MainVersionList.Invoke((Action) (() => {
                MainVersionList.Enabled = false;
                MainVersionList.BeginUpdate();
                MainVersionList.Items.Clear();
                MainVersionList.Items.Add("Downloading version list, please wait.");
                MainVersionList.EndUpdate();
            }));

            GameModInfo.ModVersion custom = new GameModInfo.ModVersion { Name = "Custom .zip", URL = "|custom|" };

            try {
                // This also caches the ModVersions in the CachedInfo.
                List<GameModInfo.ModVersion> versions = Info.ModVersions;
                versions.Sort((a, b) => {
                    return -a.Version.CompareTo(b.Version);
                });
                MainVersionList.Invoke((Action) (() => {
                    MainVersionList.Enabled = true;
                    MainVersionList.BeginUpdate();
                    MainVersionList.Items.Clear();
                    for (int i = 0; i < versions.Count; i++)
                        MainVersionList.Items.Add(versions[i]);
                    MainVersionList.Items.Add(custom);
                    MainVersionList.SelectedIndex = 0;
                    MainVersionList.EndUpdate();
                }));
            } catch {
                MainVersionList.Invoke((Action) (() => {
                    MainVersionList.Enabled = true;
                    MainVersionList.BeginUpdate();
                    MainVersionList.Items.Add(custom);
                    MainVersionList.EndUpdate();
                }));
            }
        }

        private const int WM_NCHITTEST = 0x0084;
        private readonly static IntPtr HTCAPTION = new IntPtr(0x02);
        private readonly static IntPtr HTBOTTOMRIGHT = new IntPtr(0x11);

        private const int WM_ERASEBKGND = 0x0014;
        private readonly static IntPtr FALSE = new IntPtr(0);
        private readonly static IntPtr TRUE = new IntPtr(1);

        private const int WM_SYSCOMMAND = 0x0112;
        private readonly static IntPtr SC_MINIMIZE = new IntPtr(0xF020);
        private readonly static IntPtr SC_RESTORE = new IntPtr(0xF120);

        protected override void WndProc(ref Message m) {
            if (m.Msg == WM_NCHITTEST && !AnimationManager.IsMono) {
                // This likes to kill the animation manager in Mono and sometimes fails on Linux.
                Point cursor = new Point(m.LParam.ToInt32() & 0xFFFF, m.LParam.ToInt32() >> 16);
                cursor = PointToClient(cursor);
                if (cursor.Y < 130) {
                    m.Result = HTCAPTION;
                    return;
                }
                /*
                if (cursor.X >= ClientSize.Width - 32 && cursor.Y >= ClientSize.Height - 32) {
                    m.Result = HTBOTTOMRIGHT;
                    return;
                }
                */
            }

            if (m.Msg == WM_SYSCOMMAND) {
                if (m.WParam == SC_MINIMIZE) {
                    FormBorderStyle = FormBorderStyle.FixedSingle;
                    WindowState = FormWindowState.Minimized;
                    return;
                } else if (m.WParam == SC_RESTORE) {
                    WindowState = FormWindowState.Normal;
                    FormBorderStyle = FormBorderStyle.None;
                    return;
                }
            }

            base.WndProc(ref m);
        }

        private bool _Dragging = false;
        private Point _DraggingStartPoint;

        private void _DragStart(object sender, MouseEventArgs e) {
            _Dragging = true;
            _DraggingStartPoint = e.Location;
            AnimationManager.IsThrottled = true;
        }

        private void _DragMove(object sender, MouseEventArgs e) {
            if (!_Dragging)
                return;

            Location = new Point(
                (Location.X - _DraggingStartPoint.X) + e.X,
                (Location.Y - _DraggingStartPoint.Y) + e.Y
            );

            // Refresh handled by AnimationManager
            // Refresh();
        }

        private void _DragEnd(object sender, MouseEventArgs e) {
            _Dragging = false;
            AnimationManager.IsThrottled = false;
        }


        private void MainBrowseButton_Click(object sender, EventArgs e) {
            _ExeBrowseDialog.ShowDialog(this);
        }

        private void InstallButton_Click(object sender, EventArgs e) {
            if (Status != InstallerStatus.Selecting)
                return;
            Status = InstallerStatus.Transitioning;
            MainPanel.SlideOut();
            ProgressPanel.SlideIn();

            Modder.Install();
        }

        private void MainUninstallButton_Click(object sender, EventArgs e) {
            if (Status != InstallerStatus.Selecting)
                return;
            Status = InstallerStatus.Transitioning;
            MainPanel.SlideOut();
            ProgressPanel.SlideIn();

            Modder.Uninstall();
        }

        private void MinimizeButton_Click(object sender, EventArgs e) {
            FormBorderStyle = FormBorderStyle.FixedSingle;
            WindowState = FormWindowState.Minimized;
        }

        private void CloseButton_Click(object sender, EventArgs e) {
            Close();
        }

        private int _PreviousModVersionIndex;
        private void MainVersionList_SelectedIndexChanged(object sender, EventArgs e) {
            if (_PreviousModVersionIndex == MainVersionList.SelectedIndex)
                return;

            GameModInfo.ModVersion version = MainVersionList.SelectedItem as GameModInfo.ModVersion;
            if (version == null)
                return;

            if (version.URL == "|custom|") {
                if (_ModBrowseDialog.ShowDialog(this) != DialogResult.OK)
                    MainVersionList.SelectedIndex = _PreviousModVersionIndex;
                return;
            }

            _PreviousModVersionIndex = MainVersionList.SelectedIndex;
            Info.CurrentInstallingModVersion = version;
        }

        public enum InstallerStatus {
            Selecting,
            Transitioning,
            Downloading,
            Installing,
            Uninstalling,
            Done
        }

    }
}
