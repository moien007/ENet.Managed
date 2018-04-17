using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

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
