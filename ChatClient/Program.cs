using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ENet.Managed;
using System.Net.Sockets;
using System.Net;

namespace ChatClient
{
    class Program
    {
        static ManualResetEvent _WaitEvent = new ManualResetEvent(false);
        
        static void Main(string[] args)
        {
            ManagedENet.Startup();

            ENetHost host = new ENetHost(1, 1);
            host.OnConnect += Host_OnConnect;
            host.OnDisconnect += Host_OnDisconnect;
            host.OnReceive += Host_OnReceive;
            host.StartServiceThread();

            var peer = host.Connect(new IPEndPoint(IPAddress.Loopback, 25000), 1, 0);
            Console.WriteLine("Connecting...");
            _WaitEvent.WaitOne();
            
            while (true)
            {
                var text = Console.ReadLine();
                var bytes = Encoding.UTF8.GetBytes(text);
                peer.Send(bytes, 0, ENetPacketFlags.Reliable);
            }
        }

        private static void Host_OnReceive(object sender, ENetReceiveEventArgs e)
        {
            Console.WriteLine("Received: {0}", Encoding.UTF8.GetString(e.Packet.GetPayloadFinal()));
        }

        private static void Host_OnDisconnect(object sender, ENetDisconnectEventArgs e)
        {
            Console.WriteLine("Disconnected");
        }

        private static void Host_OnConnect(object sender, ENetConnectEventArgs e)
        {
            Console.WriteLine("Connected");
            _WaitEvent.Set();
        }
    }
}
