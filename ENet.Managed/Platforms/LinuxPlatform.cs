using ENet.Managed.Internal;
using System;
using System.Runtime.InteropServices;

namespace ENet.Managed.Platforms
{
    public sealed class LinuxPlatform : Platform
    {
        public override byte[] GetENetBinaryBytes()
        {
            switch (RuntimeInformation.OSArchitecture)
            {
                case Architecture.X86:
                    return ENetBinariesResource.libenet_X86_so;

                case Architecture.X64:
                    return ENetBinariesResource.libenet_X64_so;

                case Architecture.Arm:
                    return ENetBinariesResource.libenet_ARM_so;

                default:
                    ThrowHelper.ThrowCurrentArchitectureNotSupportedYet();
                    return null!;
            }
        }

        public override string GetENetBinaryName()
        {
            switch (RuntimeInformation.OSArchitecture)
            {
                case Architecture.X86:
                    return "libenet_X86.so.7.0.1";

                case Architecture.X64:
                    return "libenet_X64.so.7.0.1";

                case Architecture.Arm:
                    return "libenet_ARM.so.7.0.1";

                default:
                    ThrowHelper.ThrowCurrentArchitectureNotSupportedYet();
                    return null!;
            }
        }

        public override void FreeDynamicLibrary(IntPtr hModule) => LinuxApi.dlclose(hModule);
        public override IntPtr LoadDynamicLibrary(string dllPath) => LinuxApi.dlopen(dllPath, LinuxApi.RTLD_NOW);
        public override IntPtr GetDynamicLibraryProcedureAddress(IntPtr handle, string procName) => LinuxApi.dlsym(handle, procName);
    }
}
