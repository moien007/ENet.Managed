using System;
using System.Net;
using Native = ENet.Managed.Structures;

namespace ENet.Managed
{
    public enum ENetInterceptionResult : int
    {
        Intercept = 1,
        Ignore = 0,
        Error = -1,
    }

    public unsafe abstract class ENetInterceptor : IDisposable
    {
        public enum InterceptionMethod
        {
            Managed,
            Unmanaged,
        }

        public ENetHost Host { get; internal set; }
        public InterceptionMethod Method { get; private set; }

        protected ENetInterceptor(InterceptionMethod method)
        {
            Method = method;
        }

        ~ENetInterceptor() => Dispose(false);

        protected virtual void Dispose(bool disposing) { }

        public virtual ENetInterceptionResult Intercept(IPEndPoint endPoint, ref byte[] buffer, out ENetEvent e)
        {
            throw new NotImplementedException();
        }

        public virtual ENetInterceptionResult Intercept(Native.ENetAddress* address, byte** buffer, UIntPtr* count, Native.ENetEvent* e)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
