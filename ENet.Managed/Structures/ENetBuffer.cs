using System;
using System.Runtime.InteropServices;

namespace ENet.Managed.Structures
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ENetBuffer
    {
        public UIntPtr DataLength;
        public IntPtr Data;
    }
}
