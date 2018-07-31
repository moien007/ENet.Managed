using System;
using System.Runtime.InteropServices;

namespace ENet.Managed.Structures
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ENetPacket
    {
        public UIntPtr ReferenceCount;
        public ENetPacketFlags Flags;
        public byte* Data;
        public UIntPtr DataLength;
        public IntPtr FreeCallback;
        public IntPtr UserData;
    }
}
