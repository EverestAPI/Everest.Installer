using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MonoMod.Installer {
    public static class AnimationManagerExt {

        public static AnimationManager GetAnimationManager(this Control c) {
            c = c.FindForm() ?? c;
            AnimationManager manager;
            if (!AnimationManager.Managers.TryGetValue(c, out manager))
                return AnimationManager.Managers[c] = new AnimationManager(c);
            return manager;
        }

        public static Animation Animate(this Control c, Action<Animation, float> a, float dur = 0.3f, bool loop = false, bool smooth = true, bool repaint = true, Func<float, float> easing = null, bool run = true) {
            Animation anim = new DAnimation() {
                Control = c,
                DOnAnimate = a,
                Duration = dur,
                Loop = loop,
                Smooth = smooth,
                Repaint = repaint,
                Easing = easing ?? Easings.SineEaseInOut
            };
            if (run)
                anim.Run();
            return anim;
        }
        public static Animation Animate(this Control c, Animation a, bool run = true) {
            a.Control = c;
            if (run)
                a.Run();
            return a;
        }

        public static Animation AnimationDelay(this Control c, float delay, bool run = true) {
            return c.Animate((a, t) => { /* Wait. */ }, dur: delay, smooth: false, run: run);
        }

        public static Animation SlideIn(this Control c, float dur = 0.75f, int from = 0, int to = 1, float delay = 0f, bool run = true) {
            if (from == 0) {
                if (c.Left != to)
                    from = c.Left;
                else
                    from = to + c.Width;
            }
            int offs = from - to;
            if (run)
                c.Left = from;
            Animation anim = c.Animate((a, t) => {
                a.Control.Left = (int) (to + offs * (1f - t));
                if (t == 0f)
                    c.Visible = true;
            }, dur: dur, easing: Easings.ExponentialEaseOut, smooth: true, run: false);
            if (delay <= 0f) {
                if (run)
                    anim.Run();
                return anim;
            }
            return c.Animate(new AnimationSequence() {
                Sequence = {
                    c.AnimationDelay(delay, run: false),
                    anim,
                }
            }, run: run);
        }

        public static Animation SlideOut(this Control c, float dur = 0.75f, int from = 1, int to = -461, float delay = 0f, bool run = true) {
            int offs = from - to;
            if (run)
                c.Left = from;
            Animation anim = c.Animate((a, t) => {
                a.Control.Left = (int) (to + offs * (1f - t));
                if (t == 1f)
                    c.Visible = false;
            }, dur: dur, easing: Easings.ExponentialEaseOut, smooth: true, run: false);
            if (delay <= 0f) {
                if (run)
                    anim.Run();
                return anim;
            }
            return c.Animate(new AnimationSequence() {
                Sequence = {
                    c.AnimationDelay(delay, run: false),
                    anim,
                }
            }, run: run);
        }

        public static void PrepareAnimations(this Button b, Color cBorderFocused) {
            if (b == null)
                return;

            Animation fadeCurrent = null;

            Action<Color, Color> fade = (cBorderTo, cBackTo) => {
                if (fadeCurrent != null)
                    fadeCurrent.Status = Animation.EStatus.Finished;
                Color cBorderFrom = b.FlatAppearance.BorderColor;
                Color cBackFrom = b.BackColor;
                fadeCurrent = b.Animate((a, t) => {
                    b.SuspendLayout();

                    b.FlatAppearance.BorderColor = cBorderFrom.Lerp(cBorderTo, t);
                    b.BackColor = b.FlatAppearance.MouseOverBackColor = cBackFrom.Lerp(cBackTo, t);

                    b.ResumeLayout(false);
                    b.PerformLayout();
                }, dur: 0.15f, smooth: false);
            };

            Color cBorderNeutral = b.FlatAppearance.BorderColor;
            Color cBackNeutral = b.BackColor;
            Color cBackHovered = b.FlatAppearance.MouseOverBackColor;

            b.FlatAppearance.MouseOverBackColor = b.BackColor;

            bool mouseInside = false;

            b.MouseEnter += (s, e) => {
                fade(cBorderFocused, cBackHovered);
                mouseInside = true;
            };
            b.GotFocus += (s, e) => {
                fade(cBorderFocused, cBackHovered);
            };
            b.MouseLeave += (s, e) => {
                if (!b.Focused)
                    fade(cBorderNeutral, cBackNeutral);
                mouseInside = false;
            };
            b.LostFocus += (s, e) => {
                if (!mouseInside)
                    fade(cBorderNeutral, cBackNeutral);
            };
        }

    }

    public class AnimationManager {

        public readonly static Dictionary<Control, AnimationManager> Managers = new Dictionary<Control, AnimationManager>();

        public readonly static bool IsMono = Type.GetType("Mono.Runtime") != null;
        public readonly static bool SupportsFast = Type.GetType("Mono.Runtime") != null && Environment.OSVersion.Platform != PlatformID.Win32NT;

        private Stopwatch _Stopwatch;
        public float Time { get; private set; }
        public float DeltaTime { get; private set; }
        public float FrameTimeThrottled = 1f / 15f;
        // Blame Vicyorus#5202 on Discord when CPUs start melting.
        public float FrameTime = 0f; // 1f / 60f;
        public float FrameTimeSmooth = 0f; // 1f / 60f;
        public float FrameTimeBattery = 1f / 30f;

        public float CurrentFrameTime { get; private set; }

        public bool IsThrottled = false;
        public bool IsLayered = false;
        public int Invalidated = 0;
        public int InvalidatedRoot = 0;

        public bool AutoRepaintOnMouseMove = true;
        public bool AutoRepaint = false;
        public bool AutoRepaintChildren = false;
        public bool Repaint = false;
        public Control AnimationRoot;

        public List<Animation> Animations = new List<Animation>();

        private IAsyncResult _CurrentRootInvalidate;

        public AnimationManager(Control c) {
            AnimationRoot = c;
            Managers[c] = this;

            c.Invalidated += (s, e) => InvalidatedRoot++;

            _StartThread();
        }

        private Thread _Thread;
        private void _StartThread() {
            if (_Thread != null)
                return;

            if (FrameTimeSmooth <= 0f) {
                int rate = DisplayRefreshRate;
                FrameTimeSmooth = rate > 0 ? 1f / rate : 0f;
            }
            if (FrameTime <= 0f)
                FrameTime = FrameTimeSmooth;

            _Thread = new Thread(_ThreadLoop);
            _Thread.Name = "AnimationManager Thread";
            _Thread.IsBackground = true;
            _Thread.Start();
        }
        private void _ThreadLoop() {
            if (_Stopwatch == null)
                _Stopwatch = new Stopwatch();
            _Stopwatch.Reset();
            _Stopwatch.Start();

            List<Control> controls = new List<Control>();

            float frameTimeF = FrameTimeSmooth;
            long frameTime;
            long frameStartPrev;
            long frameStart = 0;
            long frameLeft;

            float timePrev = _Stopwatch.ElapsedMilliseconds * 0.001f;

            Point cursorPrev = Cursor.Position;
            Point cursor;

            bool repaint = false;

            while (_Thread != null) {
                if (AutoRepaintOnMouseMove) {
                    cursor = Cursor.Position;
                    if (cursorPrev != cursor) {
                        repaint = true;
                        cursorPrev = cursor;
                    }
                }

                if (!IsMono && SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Offline)
                    frameTimeF = FrameTimeBattery;

                if (IsThrottled)
                    frameTimeF = FrameTimeThrottled;

                frameTime = (long) (frameTimeF * 1000L);
                while ((frameLeft = frameTime - (_Stopwatch.ElapsedMilliseconds - frameStart)) > 0)
                    Thread.Sleep((int) frameLeft / 3);
                frameStartPrev = frameStart;
                frameStart = _Stopwatch.ElapsedMilliseconds;
                CurrentFrameTime = (frameStart - frameStartPrev) * 0.001f;

                controls.Clear();

                Time = _Stopwatch.ElapsedMilliseconds * 0.001f;
                DeltaTime = Time - timePrev;
                timePrev = Time;
                frameTimeF = FrameTime;

                for (int i = Animations.Count - 1; i > -1; --i) {
                    Animation anim = Animations[i];
                    if (anim == null)
                        continue;
                    if (anim.Control == null || anim.Control.IsDisposed)
                        continue;
                    /*
                    if (anim.Control != AnimationRoot && !controls.Contains(anim.Control)) {
                        InvokeSuspend(anim.Control);
                        controls.Add(anim.Control);
                    }
                    /**/
                    if (anim.Status == Animation.EStatus.Uninitialized)
                        anim.Init();
                    if (anim.Status == Animation.EStatus.Running)
                        anim.Update();
                    if (anim.Status == Animation.EStatus.Finished) {
                        anim.End();
                        lock (Animations)
                            Animations.RemoveAt(i);
                    }
                    if (anim.Smooth)
                        frameTimeF = FrameTimeSmooth;
                    repaint |= anim.Repaint;
                }
                
                /*
                for (int i = controls.Count - 1; i > -1; --i) {
                    Control c = controls[i];
                    if (c == null || c.IsDisposed)
                        continue;
                    if (c != AnimationRoot) {
                        Invalidated--;
                        InvokeResume(c);
                    }
                }
                /**/

                if (!AutoRepaint && !Repaint && !repaint)
                    continue;
                Repaint = false;
                repaint = false;

                /*
                if (SupportsFast) {
                    InvokeInvalidate(AnimationRoot, true);
                    continue;
                }
                */

                /**//*
                while (Invalidated < 0 || InvalidatedRoot < 0)
                    Thread.SpinWait(1);
                /**/
                if (/*Invalidated > 0 ||*/ InvalidatedRoot > 0) {
                    Invalidated = 0;
                    InvalidatedRoot = 0;
                    // continue;
                }

                if (_CurrentRootInvalidate != null && !_CurrentRootInvalidate.IsCompleted)
                    continue;

                _CurrentRootInvalidate = InvokeInvalidate(AnimationRoot, AutoRepaintChildren);

            }

        }

        public IAsyncResult InvokeInvalidate(Control c, bool invalidateChildren) {
            if (c == null || c.IsDisposed)
                return null;

            if (_Invalidate == null)
                _Invalidate = Invalidate;
            try {
                return c.BeginInvoke(_Invalidate, c, invalidateChildren);
            } catch {
                return null;
            }
        }

        private Action<Control, bool> _Invalidate;
        public void Invalidate(Control c, bool invalidateChildren) {
            if (c is IAnimationInvalidateable) {
                ((IAnimationInvalidateable) c).AnimationInvalidate();
                return;
            }

            c.Invalidate(invalidateChildren);
        }

        private int Suspended;

        private const int WM_SETREDRAW = 0x000B;
        private readonly static IntPtr TRUE = new IntPtr(1);

        public void InvokeSuspend(Control c) {
            if (c == null || c.IsDisposed)
                return;

            if (_Suspend == null)
                _Suspend = Suspend;
            try {
                c.BeginInvoke(_Suspend, c);
            } catch {
            }
        }

        private Action<Control> _Suspend;
        public void Suspend(Control target) {
            target.SuspendLayout();
            Message msg = Message.Create(target.Handle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
            NativeWindow window = NativeWindow.FromHandle(target.Handle);
            window.DefWndProc(ref msg);

            Suspended++;
        }

        public void InvokeResume(Control c) {
            if (c == null || c.IsDisposed)
                return;

            if (_Resume == null)
                _Resume = Resume;
            try {
                c.BeginInvoke(_Resume, c);
            } catch {
            }
        }

        private Action<Control> _Resume;
        public void Resume(Control target) {
            Message msg = Message.Create(target.Handle, WM_SETREDRAW, TRUE, IntPtr.Zero);
            NativeWindow window = NativeWindow.FromHandle(target.Handle);
            window.DefWndProc(ref msg);

            Suspended--;

            target.ResumeLayout(true);
            // target.Invalidate();
        }

        private const int VREFRESH = 116;
        [DllImport("gdi32.dll")]
        private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        public int DisplayRefreshRate {
            get {
                if (!IsMono && Environment.OSVersion.Platform == PlatformID.Win32NT) {
                    using (Graphics g = Graphics.FromHwnd(IntPtr.Zero)) {
                        IntPtr desktop = g.GetHdc();
                        int rate = GetDeviceCaps(desktop, VREFRESH);
                        if (rate <= 1)
                            return 0;
                        return rate;
                    }
                }

                return 0;
            }
        }

    }

    // Following classes adapted from SGUI: https://github.com/0x0ade/SGUI

    public abstract class Animation {

        public AnimationManager Manager;
        public Control Control;

        public float TimeStart;
        public EStatus Status = EStatus.Uninitialized;

        public float Duration;

        public bool AutoStart = true;
        public bool Loop = false;

        public bool Smooth = false;
        public bool Repaint = true;

        public Func<float, float> Easing = Easings.SineEaseInOut;

        public Animation()
            : this(0.3f) {
        }

        public Animation(float duration) {
            Duration = duration;
        }

        public virtual void Init() {
            if (Status != EStatus.Uninitialized)
                return;

            if (Manager == null)
                Manager = Control?.GetAnimationManager();
            if (Manager == null)
                return;
            if (Control == null)
                Control = Manager.AnimationRoot;

            OnInit();

            Control.Invalidated += (s, e) => Manager.Invalidated++;

            if (AutoStart) {
                Start();
            } else {
                Status = EStatus.Finished;
            }
        }

        public virtual void OnInit() {
        }

        public virtual void Update() {
            if (Status != EStatus.Running)
                return;

            float t = 0f;
            if (Duration > 0f) {
                Loop:
                t = (Manager.Time - TimeStart) / Duration;
                if (t >= 1f) {
                    End();
                    if (!Loop) return;
                    Start();
                    TimeStart += (t - 1f) * Duration;
                    goto Loop;
                }
            }

            InvokeAnimate(Easing(t));
        }

        public void Start() {
            OnStart();

            TimeStart = Manager.Time;
            Status = EStatus.Running;

            InvokeAnimate(0f);
        }

        public abstract void OnStart();

        private Action<float> _Animate;
        public abstract void Animate(float t);

        public void End() {
            Status = EStatus.Finished;
            OnEnd();
        }

        public virtual void OnEnd() {
            InvokeAnimate(1f);
        }

        public void InvokeAnimate(float t) {
            if (Control == null || Control.IsDisposed) {
                Status = EStatus.Finished;
                return;
            }

            if (_Animate == null)
                _Animate = Animate;
            try {
                Control.BeginInvoke(_Animate, t);
            } catch {
                Status = EStatus.Finished;
            }
        }

        public enum EStatus {
            Uninitialized,
            Running,
            Finished
        }

        public void Run() {
            Init();
            if (Manager == null)
                return;
            lock (Manager.Animations)
                Manager.Animations.Add(this);
        }

    }


    public class DAnimation : Animation {

        public Action<Animation> DOnInit;
        public override void OnInit() {
            DOnInit?.Invoke(this);
        }

        public Action<Animation> DOnStart;
        public override void OnStart() {
            DOnStart?.Invoke(this);
        }

        public Action<Animation, float> DOnAnimate;
        public override void Animate(float t) {
            DOnAnimate?.Invoke(this, t);
        }

        public override void OnEnd() {
            InvokeAnimate(1f);
        }

    }

    public class AnimationSequence : Animation {

        public List<Animation> Sequence = new List<Animation>();

        protected float[] _Offsets;

        protected int _CurrentIndex;
        public int CurrentIndex {
            get {
                return _CurrentIndex;
            }
        }
        public Animation Current {
            get {
                return _CurrentIndex < 0 || Sequence.Count <= _CurrentIndex ? null : Sequence[_CurrentIndex];
            }
        }
        public float CurrentOffset {
            get {
                return _CurrentIndex < 0 || Sequence.Count <= _CurrentIndex ? 0f : _Offsets[_CurrentIndex];
            }
        }
        public EStatus CurrentStatus {
            get {
                return _CurrentIndex < 0 || Sequence.Count <= _CurrentIndex ? EStatus.Finished : Sequence[_CurrentIndex].Status;
            }
        }

        public AnimationSequence()
            : base(0f) {
        }

        public override void OnStart() {
            Duration = 0f;
            _CurrentIndex = 0;

            _Offsets = new float[Sequence.Count];
            for (int i = 0; i < Sequence.Count; i++) {
                Animation anim = Sequence[i];
                if (anim == null) continue;
                _Offsets[i] = Duration;
                Duration += anim.Duration;
                anim.Loop = false;
                anim.AutoStart = false;
                anim.Control = anim.Control ?? Control;
                anim.Init();
            }

            Current.Start();
        }

        public override void Animate(float t) {
            if (CurrentStatus == EStatus.Finished) {
                Current?.End();
                _CurrentIndex++;
                Current?.Start();
            }

            if (Current == null) return;

            Smooth = Current.Smooth;

            t = ((t * Duration) - CurrentOffset) / Current.Duration;
            Current.Animate(Current.Easing(t));
            if (t >= 1f) Current.End();
        }

        public override void OnEnd() {
            base.OnEnd();
            Current?.End();
            _CurrentIndex = 0;
        }

    }

    public interface IAnimationInvalidateable {
        void AnimationInvalidate();
    }

    /// <summary>
    /// Taken from https://github.com/warrenm/AHEasing/blob/master/AHEasing/easing.c,
    /// which is licensed under the "Do What The Fuck You Want To Public License, Version 2".
    /// </summary>
    public static class Easings {

        private const float TAU = (float) Math.PI * 2f;

        /// <summary>
        /// Modeled after the line y = x
        /// </summary>
        public static float Linear(float f) { return f; }

        /// <summary>
        /// Modeled after the parabola y = x^2
        /// </summary>
        public static float QuadraticEaseIn(float f) { return f * f; }

        /// <summary>
        /// Modeled after the parabola y = -x^2 + 2x
        public static float QuadraticEaseOut(float f) { return -(f * (f - 2f)); }

        /// <summary>
        /// Modeled after the piecewise quadratic
        /// y = (1/2)((2x)^2)             ; [0, 0.5)
        /// y = -(1/2)((2x-1)*(2x-3) - 1) ; [0.5, 1]
        /// </summary>
        public static float QuadraticEaseInOut(float f) {
            if (f < 0.5f) {
                return 8f * f * f * f * f;
            } else {
                f = (f - 1f);
                return -8f * f * f * f * f + 1f;
            }
        }

        /// <summary>
        /// Modeled after the cubic y = x^3
        /// </summary>
        public static float CubicEaseIn(float f) { return f * f * f; }

        /// <summary>
        /// Modeled after the cubic y = (x - 1)^3 + 1
        /// </summary>
        public static float CubicEaseOut(float f) {
            f = (f - 1f);
            return f * f * f + 1f;
        }

        /// <summary>
        /// Modeled after the piecewise cubic
        /// y = (1/2)((2x)^3)       ; [0, 0.5)
        /// y = (1/2)((2x-2)^3 + 2) ; [0.5, 1]
        /// </summary>
        public static float CubicEaseInOut(float f) {
            if (f < 0.5f) {
                return 4f * f * f * f;
            } else {
                f = ((2f * f) - 2f);
                return 0.5f * f * f * f + 1f;
            }
        }

        /// <summary>
        /// Modeled after the quartic x^4
        /// </summary>
        public static float QuarticEaseIn(float f) { return f * f * f * f; }

        /// <summary>
        /// Modeled after the quartic y = 1 - (x - 1)^4
        /// </summary>
        public static float QuarticEaseOut(float f) {
            float g = (f - 1f);
            return g * g * g * (1f - f) + 1f;
        }

        /// <summary>
        /// Modeled after the piecewise quartic
        /// y = (1/2)((2x)^4)        ; [0, 0.5)
        /// y = -(1/2)((2x-2)^4 - 2) ; [0.5, 1]
        /// </summary>
        public static float QuarticEaseInOut(float f) {
            if (f < 0.5f) {
                return 8f * f * f * f * f;
            } else {
                f = (f - 1f);
                return -8f * f * f * f * f + 1f;
            }
        }

        /// <summary>
        /// Modeled after the quintic y = x^5
        /// </summary>
        public static float QuinticEaseIn(float f) { return f * f * f * f * f; }

        /// <summary>
        /// Modeled after the quintic y = (x - 1)^5 + 1
        /// </summary>
        public static float QuinticEaseOut(float f) {
            f = (f - 1);
            return f * f * f * f * f + 1;
        }

        /// <summary>
        /// Modeled after the piecewise quintic
        /// y = (1/2)((2x)^5)       ; [0, 0.5)
        /// y = (1/2)((2x-2)^5 + 2) ; [0.5, 1]
        /// </summary>
        public static float QuinticEaseInOut(float f) {
            if (f < 0.5f) {
                return 16f * f * f * f * f * f;
            } else {
                f = ((2f * f) - 2f);
                return 0.5f * f * f * f * f * f + 1f;
            }
        }

        /// <summary>
        /// Modeled after quarter-cycle of sine wave
        /// </summary>
        public static float SineEaseIn(float f) { return (float) Math.Sin((f - 1) * TAU) + 1; }

        /// <summary>
        /// Modeled after quarter-cycle of sine wave (different phase)
        /// </summary>
        public static float SineEaseOut(float f) { return (float) Math.Sin(f * TAU); }

        /// <summary>
        /// Modeled after half sine wave
        /// </summary>
        public static float SineEaseInOut(float f) { return 0.5f * (1f - (float) Math.Cos(f * (float) Math.PI)); }

        /// <summary>
        /// Modeled after shifted quadrant IV of unit circle
        /// </summary>
        public static float CircularEaseIn(float f) { return 1f - (float) Math.Sqrt(1f - (f * f)); }

        /// <summary>
        /// Modeled after shifted quadrant II of unit circle
        /// </summary>
        public static float CircularEaseOut(float f) { return (float) Math.Sqrt((2f - f) * f); }

        /// <summary>
        /// Modeled after the piecewise circular function
        /// y = (1/2)(1 - sqrt(1 - 4x^2))           ; [0, 0.5)
        /// y = (1/2)(sqrt(-(2x - 3)*(2x - 1)) + 1) ; [0.5, 1]
        /// </summary>
        public static float CircularEaseInOut(float f) {
            if (f < 0.5f) {
                return 0.5f * (1f - (float) Math.Sqrt(1f - 4f * (f * f)));
            } else {
                return 0.5f * ((float) Math.Sqrt(-((2f * f) - 3f) * ((2f * f) - 1f)) + 1f);
            }
        }

        /// <summary>
        /// Modeled after the exponential function y = 2^(10(x - 1))
        /// </summary>
        public static float ExponentialEaseIn(float f) { return (f <= 0f) ? 0f : (float) Math.Pow(2f, 10f * (f - 1f)); }

        /// <summary>
        /// Modeled after the exponential function y = -2^(-10x) + 1
        /// </summary>
        public static float ExponentialEaseOut(float f) { return (f >= 1f) ? f : 1f - (float) Math.Pow(2f, -10f * f); }

        /// <summary>
        /// Modeled after the piecewise exponential
        /// y = (1/2)2^(10(2x - 1))         ; [0,0.5)
        /// y = -(1/2)*2^(-10(2x - 1))) + 1 ; [0.5,1]
        /// </summary>
        public static float ExponentialEaseInOut(float f) {
            if (f <= 0f || 1f <= f) return f;

            if (f < 0.5f) {
                return 0.5f * (float) Math.Pow(2f, (20f * f) - 10f);
            } else {
                return -0.5f * (float) Math.Pow(2f, (-20f * f) + 10f) + 1f;
            }
        }

        /// <summary>
        /// Modeled after the damped sine wave y = sin(13pi/2*x)*pow(2, 10 * (x - 1))
        /// </summary>
        public static float ElasticEaseIn(float f) { return (float) Math.Sin(13f * TAU * f) * (float) Math.Pow(2f, 10f * (f - 1f)); }

        /// <summary>
        /// Modeled after the damped sine wave y = sin(-13pi/2*(x + 1))*pow(2, -10x) + 1
        /// </summary>
        public static float ElasticEaseOut(float f) { return (float) Math.Sin(-13f * TAU * (f + 1f)) * (float) Math.Pow(2f, -10f * f) + 1f; }

        /// <summary>
        /// Modeled after the piecewise exponentially-damped sine wave:
        /// y = (1/2)*sin(13pi/2*(2*x))*pow(2, 10 * ((2*x) - 1))      ; [0,0.5)
        /// y = (1/2)*(sin(-13pi/2*((2x-1)+1))*pow(2,-10(2*x-1)) + 2) ; [0.5, 1]
        /// </summary>
        public static float ElasticEaseInOut(float f) {
            if (f < 0.5f) {
                return 0.5f * (float) Math.Sin(13f * TAU * (2f * f)) * (float) Math.Pow(2f, 10f * ((2f * f) - 1f));
            } else {
                return 0.5f * ((float) Math.Sin(-13f * TAU * ((2f * f - 1f) + 1f)) * (float) Math.Pow(2f, -10f * (2f * f - 1f)) + 2f);
            }
        }

        /// <summary>
        /// Modeled after the overshooting cubic y = x^3-x*sin(x*pi)
        /// </summary>
        public static float BackEaseIn(float f) { return f * f * f - f * (float) Math.Sin(f * (float) Math.PI); }

        /// <summary>
        /// Modeled after overshooting cubic y = 1-((1-x)^3-(1-x)*sin((1-x)*pi))
        /// </summary>
        public static float BackEaseOut(float f) {
            f = (1 - f);
            return 1f - (f * f * f - f * (float) Math.Sin(f * (float) Math.PI));
        }

        /// <summary>
        /// Modeled after the piecewise overshooting cubic function:
        /// y = (1/2)*((2x)^3-(2x)*sin(2*x*pi))           ; [0, 0.5)
        /// y = (1/2)*(1-((1-x)^3-(1-x)*sin((1-x)*pi))+1) ; [0.5, 1]
        /// </summary>
        public static float BackEaseInOut(float f) {
            if (f < 0.5f) {
                f = 2f * f;
                return 0.5f * (f * f * f - f * (float) Math.Sin(f * (float) Math.PI));
            } else {
                f = (1f - (2f * f - 1f));
                return 0.5f * (1f - (f * f * f - f * (float) Math.Sin(f * (float) Math.PI))) + 0.5f;
            }
        }

        public static float BounceEaseIn(float f) { return 1f - BounceEaseOut(1f - f); }

        public static float BounceEaseOut(float f) {
            if (f < 4f / 11f) {
                return (121f * f * f) / 16f;
            } else if (f < 8f / 11f) {
                return (363f / 40f * f * f) - (99f / 10f * f) + 17f / 5f;
            } else if (f < 9f / 10f) {
                return (4356f / 361f * f * f) - (35442f / 1805f * f) + 16061f / 1805f;
            } else {
                return (54f / 5f * f * f) - (513f / 25.0f * f) + 268f / 25f;
            }
        }

        public static float BounceEaseInOut(float f) {
            if (f < 0.5f) {
                return 0.5f * BounceEaseIn(f * 2f);
            } else {
                return 0.5f * BounceEaseOut(f * 2f - 1f) + 0.5f;
            }
        }

    }
}
