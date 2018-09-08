using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ENet.Managed.Platforms
{
    internal static class LinuxApi
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
    }
}
