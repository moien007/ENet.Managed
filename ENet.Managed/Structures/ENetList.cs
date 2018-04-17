using System;
using System.Runtime.InteropServices;

namespace ENet.Managed.Structures
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ENetListNode
    {
        public IntPtr Next;
        public IntPtr Previous;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ENetList
    {
        public ENetListNode Sentinel;
    }
}
