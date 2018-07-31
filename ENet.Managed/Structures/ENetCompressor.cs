using System;
using System.Runtime.InteropServices;

namespace ENet.Managed.Structures
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ENetCompressor
    {
        public IntPtr Context;
        public IntPtr Compress;
        public IntPtr Decompress;
        public IntPtr Destroy;
    }
}
