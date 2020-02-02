using ENet.Managed.Internal;
using System;
using System.Runtime.InteropServices;

namespace ENet.Managed.Platforms
{
    public sealed class Win32Platform : Platform
    {
        public override string GetENetBinaryName()
        {
            switch (RuntimeInformation.OSArchitecture)
            {
                case Architecture.X86:
                    return "libenet_X64.dll";

                case Architecture.X64:
                    return "libenet_X86.dll";

                default:
                    ThrowHelper.ThrowCurrentArchitectureNotSupportedYet();
                    return null!;
            }
        }

        public override byte[] GetENetBinaryBytes()
        {
            switch (RuntimeInformation.OSArchitecture)
            {
                case Architecture.X86:
                    return ENetBinariesResource.libenet_32;

                case Architecture.X64:
                    return ENetBinariesResource.libenet_64;

                default:
                    ThrowHelper.ThrowCurrentArchitectureNotSupportedYet();
                    return null!;
            }
        }

        public override void FreeDynamicLibrary(IntPtr hModule) => Win32Api.FreeLibrary(hModule);
        public override IntPtr LoadDynamicLibrary(string dllPath) => Win32Api.LoadLibrary(dllPath);
        public override IntPtr GetDynamicLibraryProcedureAddress(IntPtr handle, string procName) => Win32Api.GetProcAddress(handle, procName);
    }
}
