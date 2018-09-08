using System;
using System.Runtime.InteropServices;

namespace ENet.Managed.Platforms
{
    public class Linux : Platform
    {
        public override byte[] GetENetBinaryBytes() => Environment.Is64BitProcess ?
                                                   ENetBinariesResource.libenet_X64_so :
                                                   ENetBinariesResource.libenet_X86_so;

        public override string GetENetBinaryName() => Environment.Is64BitProcess ?
                                                         "libenet_X64.so.7.0.1" :
                                                         "libenet_X86.so.7.0.1";

        public override void FreeLibrary(IntPtr hModule) => LinuxApi.dlclose(hModule);
        public override IntPtr LoadLibrary(string dllPath) => LinuxApi.dlopen(dllPath, LinuxApi.RTLD_NOW);
        public override IntPtr GetProcAddress(IntPtr handle, string procName) => LinuxApi.dlsym(handle, procName);
        public override IntPtr MemoryCopy(IntPtr dest, IntPtr src, UIntPtr count) => LinuxApi.memcpy(dest, src, count);
    }
}
