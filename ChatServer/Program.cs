using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using ENet.Managed;

namespace ChatServer
{
    class Program
    {
        static void Main(string[] args)
        {
            ManagedENet.Startup();

            ENetHost host = new ENetHost(new IPEndPoint(IPAddress.Loopback, 25000), 1, 1);
            host.OnConnect += Host_OnConnect;
            host.OnDisconnect += Host_OnDisconnect;
            host.OnReceive += Host_OnReceive;

            host.StartServiceThread();

            while (true) Console.ReadKey();
        }

        private static void Host_OnConnect(object sender, ENetConnectEventArgs e)
        {
            Console.WriteLine("Peer connected from {0}", e.Peer.RemoteEndPoint);
            (sender as ENetHost).Multicast(Encoding.UTF8.GetBytes("Welcome"), 0, ENetPacketFlags.Reliable, new[] { e.Peer });
        }

        private static void Host_OnReceive(object sender, ENetReceiveEventArgs e)
        {
            Console.WriteLine("Peer: {0}", Encoding.UTF8.GetString(e.Packet.GetPayloadFinal()));
        }

        private static void Host_OnDisconnect(object sender, ENetDisconnectEventArgs e)
        {
            Console.WriteLine("Peer disconnected");
        }
    }
}
