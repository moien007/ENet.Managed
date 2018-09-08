using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ENet.Managed.Platforms
{
	public class Win32 : Platform
	{
        public override string GetENetBinaryName() => Environment.Is64BitProcess ?
                                                        "libenet_X64.dll" :
                                                        "libenet_X86.dll";

        public override byte[] GetENetBinaryBytes() => Environment.Is64BitProcess ?
                                                   ENetBinariesResource.libenet_64 :
                                                   ENetBinariesResource.libenet_32;

		public override void FreeLibrary(IntPtr hModule) => Win32Api.FreeLibrary(hModule);
		public override IntPtr LoadLibrary(string dllPath) => Win32Api.LoadLibrary(dllPath);
		public override IntPtr GetProcAddress(IntPtr handle, string procName)  => Win32Api.GetProcAddress(handle, procName);
		public override IntPtr MemoryCopy(IntPtr dest, IntPtr src, UIntPtr count)  => Win32Api.memcpy(dest, src, count);
	}
}
