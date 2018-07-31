using System;

namespace ENet.Managed
{
    [Flags]
    public enum ENetPacketFlags : uint
    {
        Reliable = (1 << 0),
        Unsequenced = (1 << 1),
        NoAllocate = (1 << 2),
        UnreliableFragment = (1 << 3),
    }
}
