using System;

namespace ENet.Managed
{
    public class ENetMulticastException : Exception
    {
        public ENetPeer Peer { get; set; }

        public ENetMulticastException(string message, ENetPeer peer) : base(message)
        {
            Peer = peer;
        }
    }
}
