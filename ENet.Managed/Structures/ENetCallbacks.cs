using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace ENet.Managed.Structures
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ENetCallbacks
    {
        public IntPtr Malloc;
        public IntPtr Free;
        public IntPtr NoMemory;
    }
}
