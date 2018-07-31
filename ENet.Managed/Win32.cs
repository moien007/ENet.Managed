using System;
using System.Text;
using System.Runtime.InteropServices;

namespace ENet.Managed
{
    internal unsafe static class Win32
    {
        public const string KERNEL32 = "kernel32.dll";
        public const string MSVCRT = "msvcrt.dll";
        public const string SHLWAPI = "shlwapi.dll";

        [DllImport(KERNEL32)]
        public static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport(KERNEL32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FreeLibrary(IntPtr hModule);

        [DllImport(KERNEL32)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport(MSVCRT, EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr MemoryCopy(IntPtr dest, IntPtr src, UIntPtr count);

        [DllImport(SHLWAPI, CharSet = CharSet.Unicode)]
        public static extern IntPtr StrFormatKBSize(long qdw, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder pszBuf, uint cchBuf);
    }
}
