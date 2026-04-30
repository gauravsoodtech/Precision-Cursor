using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PrecisionCursor
{
    internal sealed class RawMouseInputWindow : NativeWindow, IDisposable
    {
        private bool _disposed;

        public RawMouseInputWindow()
        {
            CreateHandle(new CreateParams
            {
                Caption = "PrecisionCursorRawInputSink"
            });

            RegisterForRawMouseInput();
        }

        public event EventHandler<RawMouseDeltaEventArgs> MouseMoved;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            UnregisterRawMouseInput();
            DestroyHandle();
            GC.SuppressFinalize(this);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == NativeMethods.WM_INPUT)
            {
                ProcessRawInput(m.LParam);
            }

            base.WndProc(ref m);
        }

        private void ProcessRawInput(IntPtr rawInputHandle)
        {
            uint size = 0;
            uint headerSize = (uint)Marshal.SizeOf(typeof(NativeMethods.RAWINPUTHEADER));

            uint queryResult = NativeMethods.GetRawInputData(
                rawInputHandle,
                NativeMethods.RID_INPUT,
                IntPtr.Zero,
                ref size,
                headerSize);

            if (queryResult != 0 || size == 0)
            {
                return;
            }

            IntPtr buffer = Marshal.AllocHGlobal((int)size);

            try
            {
                uint readResult = NativeMethods.GetRawInputData(
                    rawInputHandle,
                    NativeMethods.RID_INPUT,
                    buffer,
                    ref size,
                    headerSize);

                if (readResult != size)
                {
                    return;
                }

                NativeMethods.RAWINPUT rawInput =
                    (NativeMethods.RAWINPUT)Marshal.PtrToStructure(
                        buffer,
                        typeof(NativeMethods.RAWINPUT));

                if (rawInput.header.dwType != NativeMethods.RIM_TYPEMOUSE)
                {
                    return;
                }

                int deltaX = rawInput.mouse.lLastX;
                int deltaY = rawInput.mouse.lLastY;

                if (deltaX == 0 && deltaY == 0)
                {
                    return;
                }

                EventHandler<RawMouseDeltaEventArgs> handler = MouseMoved;

                if (handler != null)
                {
                    handler(this, new RawMouseDeltaEventArgs(deltaX, deltaY));
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        private void RegisterForRawMouseInput()
        {
            NativeMethods.RAWINPUTDEVICE[] devices =
            {
                new NativeMethods.RAWINPUTDEVICE
                {
                    usUsagePage = 0x01,
                    usUsage = 0x02,
                    dwFlags = NativeMethods.RIDEV_INPUTSINK,
                    hwndTarget = Handle
                }
            };

            bool registered = NativeMethods.RegisterRawInputDevices(
                devices,
                (uint)devices.Length,
                (uint)Marshal.SizeOf(typeof(NativeMethods.RAWINPUTDEVICE)));

            if (!registered)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to register raw mouse input.");
            }
        }

        private static void UnregisterRawMouseInput()
        {
            NativeMethods.RAWINPUTDEVICE[] devices =
            {
                new NativeMethods.RAWINPUTDEVICE
                {
                    usUsagePage = 0x01,
                    usUsage = 0x02,
                    dwFlags = NativeMethods.RIDEV_REMOVE,
                    hwndTarget = IntPtr.Zero
                }
            };

            NativeMethods.RegisterRawInputDevices(
                devices,
                (uint)devices.Length,
                (uint)Marshal.SizeOf(typeof(NativeMethods.RAWINPUTDEVICE)));
        }
    }

    internal sealed class RawMouseDeltaEventArgs : EventArgs
    {
        public RawMouseDeltaEventArgs(int deltaX, int deltaY)
        {
            DeltaX = deltaX;
            DeltaY = deltaY;
        }

        public int DeltaX { get; private set; }

        public int DeltaY { get; private set; }
    }
}
