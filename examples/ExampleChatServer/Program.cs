using System;
using System.Net;
using System.Text;
using ENet.Managed;

namespace ExampleChatServer
{
    class Program
    {
        const int MaximumPeers = 16;
        const int MaximumChannels = 1;

        static void Main(string[] args)
        {
            // You should call this at start of your application or before any usage of ENet
            Console.WriteLine("Starting ENet...");
            ManagedENet.Startup();

            // The IP endpoint which we are going to listen on
            var listenEndPoint = new IPEndPoint(IPAddress.Loopback, 27015);

            // By creating ENetHost we bind the endpoint 
            Console.WriteLine("Creating host...");
            var host = new ENetHost(listenEndPoint, MaximumPeers, MaximumChannels);

            Console.WriteLine($"Servicing on {listenEndPoint}");

            while (true)
            {
                var Event = host.Service(TimeSpan.FromSeconds(60));

                switch (Event.Type)
                {
                    case ENetEventType.None:
                        continue;

                    case ENetEventType.Connect:
                        Console.WriteLine($"Peer connected from {Event.Peer.GetRemoteEndPoint()}");
                        continue;

                    case ENetEventType.Disconnect:
                        Console.WriteLine($"Peer disconnected from {Event.Peer.GetRemoteEndPoint()}");
                        continue;

                    case ENetEventType.Receive:
                        // Decode packet data bytes to ASCII string
                        var dataString = Encoding.ASCII.GetString(Event.Packet.Data);

                        // Here we prefix the dataString with peer's remote endpoint
                        dataString = $"{Event.Peer.GetRemoteEndPoint()}: {dataString}";

                        Console.WriteLine($"Peer {Event.Peer.GetRemoteEndPoint()}: {dataString}");

                        // this will broadcast the packet to all connected peers
                        // including the peer that sent this packet 
                        host.Broadcast(Event.ChannelId, Event.Packet.Data, ENetPacketFlags.Reliable);

                        // We are done with the packet that the peer sent so we destroy it
                        // if you miss this you will end up with memory leaks
                        Event.Packet.Destroy();
                        continue;

                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }
}
