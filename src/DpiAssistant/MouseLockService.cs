using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DpiAssistant.Core;

namespace DpiAssistant
{
    internal sealed class MouseLockService : IDisposable
    {
        private readonly object _sync = new object();
        private readonly NativeMethods.LowLevelMouseProc _hookCallback;
        private RawMouseInputWindow _rawInputWindow;
        private IntPtr _hookId = IntPtr.Zero;
        private volatile bool _enabled;
        private bool _disposed;
        private RelativeLineLock _lineLock;
        private Point? _lastProgrammaticPoint;

        public MouseLockService()
        {
            _hookCallback = HookCallback;
        }

        public event EventHandler EnabledChanged;

        public bool Enabled
        {
            get
            {
                lock (_sync)
                {
                    return _enabled;
                }
            }
        }

        public void Start()
        {
            ThrowIfDisposed();

            if (_rawInputWindow != null)
            {
                return;
            }

            _rawInputWindow = new RawMouseInputWindow(IsEnabledForInputProcessing);
            _rawInputWindow.MouseMoved += OnRawMouseMoved;
            _hookId = InstallHook(_hookCallback);
        }

        public void Toggle()
        {
            if (Enabled)
            {
                Disable();
            }
            else
            {
                Enable();
            }
        }

        public void Enable()
        {
            bool changed;

            lock (_sync)
            {
                changed = !_enabled;
                _lineLock = CreateLineLock();
                _lastProgrammaticPoint = null;
                _enabled = true;
            }

            if (changed)
            {
                OnEnabledChanged();
            }
        }

        public void Disable()
        {
            bool changed;

            lock (_sync)
            {
                changed = _enabled;
                _enabled = false;
                _lastProgrammaticPoint = null;
            }

            if (changed)
            {
                OnEnabledChanged();
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            Disable();

            if (_rawInputWindow != null)
            {
                _rawInputWindow.MouseMoved -= OnRawMouseMoved;
                _rawInputWindow.Dispose();
                _rawInputWindow = null;
            }

            if (_hookId != IntPtr.Zero)
            {
                NativeMethods.UnhookWindowsHookEx(_hookId);
                _hookId = IntPtr.Zero;
            }

            GC.SuppressFinalize(this);
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                int message = wParam.ToInt32();

                if (MouseHookProcessingPolicy.ShouldInspectMouseMove(nCode, message, _enabled))
                {
                    NativeMethods.MSLLHOOKSTRUCT hookData =
                        (NativeMethods.MSLLHOOKSTRUCT)Marshal.PtrToStructure(
                            lParam,
                            typeof(NativeMethods.MSLLHOOKSTRUCT));

                    Point current = hookData.pt.ToPoint();
                    bool enabled;
                    bool expectedProgrammaticMove;

                    lock (_sync)
                    {
                        enabled = _enabled;
                        expectedProgrammaticMove = _lastProgrammaticPoint.HasValue
                            && current == _lastProgrammaticPoint.Value;

                        if (expectedProgrammaticMove)
                        {
                            _lastProgrammaticPoint = null;
                        }
                    }

                    bool isInjectedMove = (hookData.flags & NativeMethods.LLMHF_INJECTED) != 0;

                    if (MouseMoveSuppressionPolicy.ShouldSuppress(
                        enabled,
                        isInjectedMove,
                        expectedProgrammaticMove))
                    {
                        return new IntPtr(1);
                    }
                }
            }
            catch
            {
            }

            return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        private bool IsEnabledForInputProcessing()
        {
            return _enabled;
        }

        private void OnRawMouseMoved(object sender, int deltaX, int deltaY)
        {
            Point? target = null;

            lock (_sync)
            {
                if (!_enabled)
                {
                    return;
                }

                if (_lineLock == null)
                {
                    _lineLock = CreateLineLock();
                }

                target = _lineLock.ApplyDelta(deltaX, deltaY);
            }

            if (target.HasValue)
            {
                Point targetPoint = target.Value;

                if (Cursor.Position == targetPoint)
                {
                    return;
                }

                lock (_sync)
                {
                    _lastProgrammaticPoint = targetPoint;
                }

                if (!NativeMethods.SetCursorPos(targetPoint.X, targetPoint.Y))
                {
                    lock (_sync)
                    {
                        _lastProgrammaticPoint = null;
                    }
                }
            }
        }

        private void OnEnabledChanged()
        {
            EventHandler handler = EnabledChanged;

            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        private static IntPtr InstallHook(NativeMethods.LowLevelMouseProc callback)
        {
            using (Process currentProcess = Process.GetCurrentProcess())
            {
                ProcessModule currentModule = currentProcess.MainModule;
                IntPtr hookId = NativeMethods.SetWindowsHookEx(
                    NativeMethods.WH_MOUSE_LL,
                    callback,
                    NativeMethods.GetModuleHandle(currentModule.ModuleName),
                    0);

                if (hookId == IntPtr.Zero)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to install the global mouse hook.");
                }

                return hookId;
            }
        }

        private static RelativeLineLock CreateLineLock()
        {
            return new RelativeLineLock(Cursor.Position, SystemInformation.VirtualScreen);
        }
    }
}
