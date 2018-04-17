using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    public enum ENetInterceptionMethod
    {
        Managed,
        Unmanaged,
    }

    public unsafe abstract class ENetInterceptor : IDisposable
    {
        public ENetHost Host { get; internal set; }
        public ENetInterceptionMethod Method { get; private set; }

        protected ENetInterceptor(ENetInterceptionMethod method)
        {
            Method = method;
        }

        public virtual ENetInterceptionResult Intercept(IPEndPoint endPoint, ref byte[] buffer, out ENetEvent e)
        {
            throw new NotImplementedException();
        }

        public virtual ENetInterceptionResult Intercept(Native.ENetAddress* address, byte** buffer, UIntPtr* count, Native.ENetEvent* e)
        {
            throw new NotImplementedException();
        }

        public abstract void Dispose();
    }
}
