using System;
using System.Runtime.InteropServices;

namespace ENet.Managed
{
    internal class Linux : Platform
    {
        public const string LIBDL = "libdl.so";
        public const string LIBC = "libc";
        public const int RTLD_NOW = 2;

        [DllImport(LIBDL)]
        public static extern IntPtr dlopen(string lpFileName, int flags);

        [DllImport(LIBDL)]
        public static extern int dlclose(IntPtr hModule);

        [DllImport(LIBDL)]
        public static extern IntPtr dlsym(IntPtr hModule, string name);

        [DllImport(LIBC, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr memcpy(IntPtr dest, IntPtr src, UIntPtr count);

        public override byte[] GetLibrary()
            => Environment.Is64BitProcess ? ENetBinariesResource.libenet_X64_so : ENetBinariesResource.libenet_X86_so;

        public override string GetLibraryName()
            => Environment.Is64BitProcess ? "libenet_X64.so.7.0.1" : "libenet_X86.so.7.0.1";

        public override void FreePlatformLibrary(IntPtr hModule)
             => dlclose(hModule);

        public override IntPtr LoadPlatformLibrary(string dllPath)
            => dlopen(dllPath, RTLD_NOW);

        public override IntPtr GetPlatformProcAddress(IntPtr handle, string procName)
            => dlsym(handle, procName);

        public override IntPtr MemoryCopy(IntPtr dest, IntPtr src, UIntPtr count)
            => memcpy(dest, src, count);

    }
}
