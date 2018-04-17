using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Net;
using System.Net.Sockets;

namespace ENet.Managed.Structures
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ENetAddress
    {
        public uint Host;
        public ushort Port;
        public ushort Padding;

        public IPEndPoint ToEndPoint()
        {
            var address = new IPAddress(Host);
            return new IPEndPoint(address, Port);
        }

        public static ENetAddress FromEndPoint(IPEndPoint endPoint)
        {
            if (endPoint.AddressFamily != AddressFamily.InterNetwork)
                throw new NotSupportedException(string.Format("Address Family {0} not supported", endPoint.AddressFamily));

            ENetAddress address = new ENetAddress();
#pragma warning disable CS0618 // Type or member is obsolete
            address.Host = (uint)endPoint.Address.Address;
#pragma warning restore CS0618 // Type or member is obsolete
            address.Port = (ushort)endPoint.Port;
            return address;
        }
    }
}
