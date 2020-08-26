using System;
using System.Runtime.InteropServices;

using ENet.Managed.Internal;

namespace ENet.Managed.Platforms
{
    public sealed class LinuxPlatform : Platform
    {
        public override byte[] GetENetBinaryBytes()
        {
            switch (RuntimeInformation.OSArchitecture)
            {
                case Architecture.X86:
                    return ENetBinariesResource.enet_linux_x86;

                case Architecture.X64:
                    return ENetBinariesResource.enet_linux_x86_64;

                case Architecture.Arm:
                    return ENetBinariesResource.enet_linux_arm;

                case Architecture.Arm64:
                    return ENetBinariesResource.enet_linux_arm64;

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
                    return "enet-linux-x86.so";

                case Architecture.X64:
                    return "enet-linux-x86-64.so";

                case Architecture.Arm:
                    return "enet-linux-arm.so";

                case Architecture.Arm64:
                    return "enet-linux-arm64.so";

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
