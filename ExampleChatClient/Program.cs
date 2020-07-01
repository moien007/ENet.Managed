using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using ENet.Managed;

namespace ExampleChatClient
{
    class Program
    {
        const int MaximumPeers = 1;
        const int MaximumChannels = 1;

        static void Main(string[] args)
        {
            // You should call this at start of your application or before any usage of ENet library
            Console.WriteLine("Starting ENet...");
            ManagedENet.Startup();

            // By passing null as endpoint the system will pick up a random open endpoint to listen on
            // since we are the client we choose a random endpoint
            IPEndPoint listenEndPoint = null;
            var host = new ENetHost(listenEndPoint, MaximumPeers, MaximumChannels);

            // This is the endpoint that the server is listening on
            // IPAddress.Loopback equals to 127.0.0.1 on most systems
            IPEndPoint connectEndPoint = new IPEndPoint(IPAddress.Loopback, 27015);

            // Here we connect to the server by creating a peer and sending the connect packet
            // Connect Data is a number which we can supply with our packet 
            // this number can be ignored by server
            uint connectData = 0;
            var peer = host.Connect(connectEndPoint, MaximumChannels, connectData);

            while (true)
            {
                var Event = host.Service(TimeSpan.FromMilliseconds(250));

                switch (Event.Type)
                {
                    case ENetEventType.None:

                        // Check if user is about to write something to input
                        if (Console.KeyAvailable)
                        {
                            // Read user input
                            var line = Console.ReadLine();

                            // Encode input into ASCII bytes
                            var data = Encoding.ASCII.GetBytes(line);

                            // Send packet through channel 0 with reliable flag set
                            peer.Send(0, data, ENetPacketFlags.Reliable);
                        }

                        continue;

                    case ENetEventType.Connect:
                        Console.WriteLine("Connected");
                        continue;

                    case ENetEventType.Disconnect:
                        Console.WriteLine("Disconnected");
                        break;

                    case ENetEventType.Receive:
                        // Decode packet data into ASCII string
                        var dataString = Encoding.ASCII.GetString(Event.Packet.Data);

                        // We are done with this packet so we destroy it
                        Event.Packet.Destroy();

                        if (dataString.Trim().EndsWith("/shutdown"))
                        {
                            Console.WriteLine("Server shutdown");
                            break; // End switch block in order to break the while loop (goto X)
                        }
                        else
                        {
                            Console.WriteLine(dataString);
                            continue;
                        }

                    default:
                        throw new NotImplementedException();
                }

                // X: Break the while loop
                break;
            }

            host.Dispose();

            Console.WriteLine("Shutdown ENet...");
            ManagedENet.Shutdown();

            Console.WriteLine("Client stopped, press any key to close the app");
            Console.ReadKey();
        }
    }
}
