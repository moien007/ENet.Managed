using System;
using System.Runtime.InteropServices;

namespace ENet.Managed.Internal
{
    internal static class ThrowHelper
    {
        public static void ThrowIfArgumentNull<T>(T arg, string argName) where T : class
        {
            if (arg == null)
                throw new ArgumentNullException(argName);
        }

        public static void ThrowCurrentArchitectureNotSupportedYet()
        {
            throw new PlatformNotSupportedException($"Architecture {RuntimeInformation.OSArchitecture} is not supported yet on current platform.");
        }

        public static void ThrowCurrentPlatfromIsNotSupportedYet()
        {
            throw new PlatformNotSupportedException("Current OS platform is not supported yet.");
        }

        public static void ThrowENetInitializationFailed()
        {
            throw new ENetException("ENet library initializion failed.");
        }

        public static void ThrowENetIsNotInitialized()
        {
            throw new ENetException("ENet library is not initialized");
        }

        public static void ThrowENetAllocatorRefIsNull()
        {
            throw new NullReferenceException($"{nameof(Allocators.ENetAllocator)} reference is null unexpectedly");
        }

        public static void ThrowENetLibraryLoadFailed()
        {
            throw new ENetException($"Failed to load ENet dynamic library. (Native last error: {Marshal.GetLastWin32Error()})");
        }

        public static void ThrowENetLibraryNotLoaded()
        {
            throw new InvalidOperationException("ENet library is not loaded.");
        }

        public static void ThrowENetLibraryProcNotFound(string procName)
        {
            throw new DllNotFoundException($"Procedure '{procName}' doesn't found within ENet dynamic library.");
        }

        public static void ThrowENetPacketPointerNull()
        {
            throw new NullReferenceException("Pointer to packet structure is null.");
        }

        public static void ThrowENetPacketResizeFailed()
        {
            throw new ENetException("ENet packet resizing failed.");
        }

        public static void ThrowENetHostNoChecksumInUse()
        {
            throw new InvalidOperationException("Host is not using any checksum method.");
        }

        public static void ThrowENetHostIsUsingCRC32()
        {
            throw new InvalidOperationException("Host is using ENet's builtin CRC32 checksum method.");
        }

        public static void ThrowENetHostNoCompresserInUse()
        {
            throw new InvalidOperationException("Host is not using any compression method.");
        }

        public static void ThrowENetHostIsUsingRangeCoder()
        {
            throw new InvalidOperationException("Host is using ENet's builtin range-coder compression method.");
        }

        public static void ThrowENetHostIsNotUsingInterceptor()
        {
            throw new InvalidOperationException("Host is not using any interceptor.");
        }

        public static void ThrowENetNullPeerPointer()
        {
            throw new NullReferenceException("Pointer to peer structure is null.");
        }

        public static void ThrowENetPeerSendFailed()
        {
            throw new ENetException("Failed to send packet.");
        }

        public static void ThrowENetHostSetCompressWithRangeCoderFailed()
        {
            throw new ENetException("Failed to set compressor to ENet's builtin compressor");
        }

        public static void ThrowENetFailure()
        {
            throw new ENetException("ENet method returned failure code.");
        }

        public static void ThrowENetConnectFailure()
        {
            throw new ENetException("ENet failed connection failure.");
        }

        public static void ThrowENetCreateHostFailed()
        {
            throw new ENetException("ENet host creation failed.");
        }
    }
}
