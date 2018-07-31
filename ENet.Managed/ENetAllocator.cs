using System;

namespace ENet.Managed
{
    public abstract class ENetAllocator : IDisposable
    {
        public abstract IntPtr Alloc(int size);
        public abstract void Free(IntPtr ptr);
        public abstract void Dispose();
    }
}
