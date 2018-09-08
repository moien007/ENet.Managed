using System;

namespace ENet.Managed
{
    public unsafe abstract class ENetChecksum : IDisposable
    {
        public enum ChecksumMethod
        {
            Array,
            Pointer,
        }

        public ENetHost Host { get; internal set; }
        public ChecksumMethod Method { get; private set; }

        protected ENetChecksum(ChecksumMethod method)
        {
            Method = method;
        }

        ~ENetChecksum() => Dispose(false);

        public abstract void Begin();
        public virtual void Sum(byte[] buffer) { throw new NotImplementedException(); }
        public virtual void Sum(byte* p, int count) { throw new NotImplementedException(); }
        public abstract uint End();
        protected virtual void Dispose(bool disposing) { }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
