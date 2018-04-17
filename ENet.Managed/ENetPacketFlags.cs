using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
