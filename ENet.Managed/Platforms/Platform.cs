using System;
using System.Runtime.InteropServices;

namespace ENet.Managed.Platforms
{
    public abstract class Platform
    {
        static Platform _CurrentPlatform = null;

        public static Platform Current
        {
            // According to https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/constructor
            // We should not throw exception from static ctor
            // Thats why we are using property

            get
            {
                if (_CurrentPlatform != null)
                    return _CurrentPlatform;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    _CurrentPlatform = new Win32();
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    _CurrentPlatform = new Linux();
                }
                else
                {
                    throw new NotSupportedException("Current Operation System is not supported yet.");
                }

                return _CurrentPlatform;
            }
        }

        public abstract string GetENetBinaryName();
        public abstract byte[] GetENetBinaryBytes();
        public abstract IntPtr LoadLibrary(string dllPath);
        public abstract void FreeLibrary(IntPtr hModule);
        public abstract IntPtr GetProcAddress(IntPtr handle, string procName);
        public abstract IntPtr MemoryCopy(IntPtr dest, IntPtr src, UIntPtr count);
    }
}

