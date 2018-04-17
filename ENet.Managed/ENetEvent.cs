namespace ENet.Managed
{
    public abstract class ENetEvent
    {
        public ENetPeer Peer { get; set; }
    }

    public class ENetConnectEventArgs : ENetEvent
    {
        public uint Data { get; set; }
    }

    public class ENetDisconnectEventArgs : ENetEvent
    {
        public uint Data { get; set; }
    }

    public class ENetReceiveEventArgs : ENetEvent
    {
        public ENetPacket Packet { get; set; }
    }
}
