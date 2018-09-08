using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ENet.Managed
{
	internal class Win32 : Platform
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

        public override string GetLibraryName()
            => Environment.Is64BitProcess ? "libenet_X64.dll" : "libenet_X86.dll";

        public override byte[] GetLibrary()
            => Environment.Is64BitProcess ? ENetBinariesResource.libenet_64 : ENetBinariesResource.libenet_32;

		public override void FreePlatformLibrary(IntPtr hModule)
            => FreeLibrary(hModule);
		
		public override IntPtr LoadPlatformLibrary(string dllPath)
            => LoadLibrary(dllPath);
		
		public override IntPtr GetPlatformProcAddress(IntPtr handle, string procName)
            => GetProcAddress(handle, procName);
		
		public override IntPtr MemoryCopy(IntPtr dest, IntPtr src, UIntPtr count)
            => memcpy(dest, src, count);
		
	}
}
