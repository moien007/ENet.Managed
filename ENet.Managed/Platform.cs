using System;
using System.Runtime.InteropServices;

namespace ENet.Managed
{
    internal abstract class Platform
    {
        public abstract string GetLibraryName();

        public abstract byte[] GetLibrary();

        public abstract IntPtr LoadPlatformLibrary(string dllPath);

        public abstract void FreePlatformLibrary(IntPtr hModule);

        public abstract IntPtr GetPlatformProcAddress(IntPtr handle, string procName);

        public abstract IntPtr MemoryCopy(IntPtr dest, IntPtr src, UIntPtr count);

        public static Platform GetPlatformDefaultLoader()
        {
            OperatingSystem os = Environment.OSVersion;
            PlatformID pid = os.Platform;

            switch (pid)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.WinCE:
                    return new Win32();
                case PlatformID.MacOSX:
                    throw new PlatformNotSupportedException("Mac OS is not yet supported.");
                case PlatformID.Unix:
                    return new Linux();
                default:
                    throw new PlatformNotSupportedException("Your platform is not supported.");
            }
        }
    }
}

