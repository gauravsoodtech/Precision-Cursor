using System;
using System.Runtime.InteropServices;

namespace DpiAssistant
{
    internal sealed class RawInputBuffer : IDisposable
    {
        private IntPtr _buffer = IntPtr.Zero;
        private uint _capacity;
        private bool _disposed;

        internal IntPtr EnsureCapacity(uint size)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            if (size == 0)
            {
                return IntPtr.Zero;
            }

            if (_buffer != IntPtr.Zero && _capacity >= size)
            {
                return _buffer;
            }

            IntPtr replacement = Marshal.AllocHGlobal(checked((int)size));

            if (_buffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_buffer);
            }

            _buffer = replacement;
            _capacity = size;
            return _buffer;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            if (_buffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_buffer);
                _buffer = IntPtr.Zero;
                _capacity = 0;
            }

            GC.SuppressFinalize(this);
        }
    }
}
