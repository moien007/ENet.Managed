using System;
using System.Runtime.InteropServices;

namespace ENet.Managed.Structures
{
    [UnmanagedFunctionPointer(LibENet.ENetCallingConvention)]
    public unsafe delegate UIntPtr ENetCompressCallback(IntPtr context, ENetBuffer* inBuffers, UIntPtr inBufferCount, UIntPtr inLimit, IntPtr outData, UIntPtr outLimit);

    [UnmanagedFunctionPointer(LibENet.ENetCallingConvention)]
    public delegate UIntPtr ENetDecompressCallback(IntPtr context, IntPtr inData, UIntPtr inLimit, IntPtr outData, UIntPtr outLimit);

    [UnmanagedFunctionPointer(LibENet.ENetCallingConvention)]
    public delegate void ENetDestoryCompressorCallback(IntPtr context);

    [UnmanagedFunctionPointer(LibENet.ENetCallingConvention)]
    public delegate void ENetMemoryFreeCallback(IntPtr memory);

    [UnmanagedFunctionPointer(LibENet.ENetCallingConvention)]
    public delegate IntPtr ENetMemoryAllocCallback(UIntPtr size);

    [UnmanagedFunctionPointer(LibENet.ENetCallingConvention)]
    public delegate void ENetNoMemoryCallback();

    [UnmanagedFunctionPointer(LibENet.ENetCallingConvention)]
    public unsafe delegate void ENetPacketFreeCallback(IntPtr data);

    [UnmanagedFunctionPointer(LibENet.ENetCallingConvention)]
    public unsafe delegate uint ENetChecksumCallback(ENetBuffer* buffers, UIntPtr buffersCount);

    [UnmanagedFunctionPointer(LibENet.ENetCallingConvention)]
    public unsafe delegate ENetInterceptionResult ENetInterceptCallback(IntPtr host, ENetEvent* e);
}
