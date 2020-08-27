using System;
using System.Diagnostics;
using System.Linq;
using System.Net;

using ENet.Managed;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            ManagedENet.Startup();
            try
            {
                Start();
            }
            finally
            {
                ManagedENet.Shutdown();
            }
        }

        static void Start()
        {
            var host1EP = new IPEndPoint(IPAddress.Loopback, 51222);
            var host1 = new ENetHost(host1EP, 20, 1);

            // Test peer list enumerator functionality.
            Trace.Assert(host1.PeerList.Count() == host1.PeerList.Count);
            Trace.Assert(host1.PeerList.ElementAt(19) == host1.PeerList[19]);
            Trace.Assert(host1.PeerList.ElementAt(1) != host1.PeerList[0]);


            //var host2EP = new IPEndPoint(IPAddress.Loopback, 51223);
            //var host2 = new ENetHost(host2EP, 30, 1);
        }
    }

    /*
    sealed class ENetHostTestClamp : IENetEventListener
    {
        public ENetHost Actual { get; }

        public ENetHostTestClamp(ENetHost actual)
        {
            Actual = actual;
        }

        public void OnConnect(ENetPeer peer, uint data)
        {
            throw new NotImplementedException();
        }

        public void OnDisconnect(ENetPeer peer, uint data)
        {
            throw new NotImplementedException();
        }

        public void OnReceive(ENetPeer peer, ENetPacket packet, byte channelId)
        {
            throw new NotImplementedException();
        }
    }
    */
}
