using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ENet.Managed.Platforms
{
    internal static class Win32Api
    {
        public const string KERNEL32 = "kernel32";
        public const string MSVCRT = "msvcrt";
        public const string SHLWAPI = "shlwapi";

        [DllImport(KERNEL32)]
        public static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport(KERNEL32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FreeLibrary(IntPtr hModule);

        [DllImport(KERNEL32)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport(MSVCRT, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr memcpy(IntPtr dest, IntPtr src, UIntPtr count);

        [DllImport(SHLWAPI, CharSet = CharSet.Unicode)]
        public static extern IntPtr StrFormatKBSize(long qdw, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder pszBuf, uint cchBuf);
    }
}
