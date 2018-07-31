namespace ENet.Managed
{
    public abstract class ENetEvent
    {

    }

    public class ENetNoneEventArgs : ENetEvent { }

    public class ENetConnectEventArgs : ENetEvent
    {
        public ENetPeer Peer { get; set; }
        public uint Data { get; set; }
    }

    public class ENetDisconnectEventArgs : ENetEvent
    {
        public ENetPeer Peer { get; set; }
        public uint Data { get; set; }
    }

    public class ENetReceiveEventArgs : ENetEvent
    {
        public ENetPeer Peer { get; set; }
        public ENetPacket Packet { get; set; }
    }
}
