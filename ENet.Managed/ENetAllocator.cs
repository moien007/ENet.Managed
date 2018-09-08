using System;

namespace ENet.Managed
{
    public abstract class ENetAllocator : IDisposable
    {
        ~ENetAllocator() => Dispose(false);

        public abstract IntPtr Alloc(int size);
        public abstract void Free(IntPtr ptr);
        protected virtual void Dispose(bool disposing) { }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
