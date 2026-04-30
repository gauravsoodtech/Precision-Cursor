using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PrecisionCursor
{
    internal sealed class KeyboardHookService : IDisposable
    {
        private const int VkEscape = 0x1B;
        private const int VkL = 0x4C;
        private const int VkControl = 0x11;
        private const int VkMenu = 0x12;
        private const int VkLeftControl = 0xA2;
        private const int VkRightControl = 0xA3;
        private const int VkLeftMenu = 0xA4;
        private const int VkRightMenu = 0xA5;

        private readonly Action _toggle;
        private readonly Action _forceDisable;
        private readonly NativeMethods.LowLevelKeyboardProc _hookCallback;
        private IntPtr _hookId = IntPtr.Zero;
        private bool _toggleChordDown;
        private bool _disposed;

        public KeyboardHookService(Action toggle, Action forceDisable)
        {
            if (toggle == null)
            {
                throw new ArgumentNullException("toggle");
            }

            if (forceDisable == null)
            {
                throw new ArgumentNullException("forceDisable");
            }

            _toggle = toggle;
            _forceDisable = forceDisable;
            _hookCallback = HookCallback;
        }

        public void Start()
        {
            ThrowIfDisposed();

            if (_hookId != IntPtr.Zero)
            {
                return;
            }

            _hookId = InstallHook(_hookCallback);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

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
                if (nCode >= 0)
                {
                    int message = wParam.ToInt32();
                    bool isKeyDown = message == NativeMethods.WM_KEYDOWN || message == NativeMethods.WM_SYSKEYDOWN;
                    bool isKeyUp = message == NativeMethods.WM_KEYUP || message == NativeMethods.WM_SYSKEYUP;

                    NativeMethods.KBDLLHOOKSTRUCT hookData =
                        (NativeMethods.KBDLLHOOKSTRUCT)Marshal.PtrToStructure(
                            lParam,
                            typeof(NativeMethods.KBDLLHOOKSTRUCT));

                    int vkCode = unchecked((int)hookData.vkCode);

                    if (isKeyDown && vkCode == VkEscape)
                    {
                        _forceDisable();
                    }

                    if (isKeyDown && vkCode == VkL && IsControlDown() && IsAltDown())
                    {
                        if (!_toggleChordDown)
                        {
                            _toggleChordDown = true;
                            _toggle();
                        }

                        return new IntPtr(1);
                    }

                    if (isKeyUp && IsToggleChordResetKey(vkCode))
                    {
                        _toggleChordDown = false;
                    }
                }
            }
            catch
            {
                _toggleChordDown = false;
            }

            return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        private static bool IsToggleChordResetKey(int vkCode)
        {
            return vkCode == VkL
                || vkCode == VkControl
                || vkCode == VkLeftControl
                || vkCode == VkRightControl
                || vkCode == VkMenu
                || vkCode == VkLeftMenu
                || vkCode == VkRightMenu;
        }

        private static bool IsControlDown()
        {
            return IsKeyDown(VkControl) || IsKeyDown(VkLeftControl) || IsKeyDown(VkRightControl);
        }

        private static bool IsAltDown()
        {
            return IsKeyDown(VkMenu) || IsKeyDown(VkLeftMenu) || IsKeyDown(VkRightMenu);
        }

        private static bool IsKeyDown(int virtualKey)
        {
            return (NativeMethods.GetAsyncKeyState(virtualKey) & unchecked((short)0x8000)) != 0;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        private static IntPtr InstallHook(NativeMethods.LowLevelKeyboardProc callback)
        {
            using (Process currentProcess = Process.GetCurrentProcess())
            {
                ProcessModule currentModule = currentProcess.MainModule;
                IntPtr hookId = NativeMethods.SetWindowsHookEx(
                    NativeMethods.WH_KEYBOARD_LL,
                    callback,
                    NativeMethods.GetModuleHandle(currentModule.ModuleName),
                    0);

                if (hookId == IntPtr.Zero)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to install the global keyboard hook.");
                }

                return hookId;
            }
        }
    }
}
