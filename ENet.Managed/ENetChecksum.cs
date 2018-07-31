using System;

namespace ENet.Managed
{
    public enum ENetChecksumMethod
    {
        Array,
        Pointer,
    }

    public unsafe abstract class ENetChecksum : IDisposable
    {
        public ENetHost Host { get; internal set; }
        public ENetChecksumMethod Method { get; private set; }

        protected ENetChecksum(ENetChecksumMethod method)
        {
            Method = method;
        }

        public abstract void Begin();
        public virtual void Sum(byte[] buffer) { throw new NotImplementedException(); }
        public virtual void Sum(byte* p, int count) { throw new NotImplementedException(); }
        public abstract uint End();
        public abstract void Dispose();
    }
}
