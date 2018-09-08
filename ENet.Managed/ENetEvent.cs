namespace ENet.Managed
{
    public abstract class ENetEvent { }
    public sealed class ENetNoneEventArgs : ENetEvent { }

    public sealed class ENetConnectEventArgs : ENetEvent
    {
        public ENetPeer Peer { get; set; }
        public uint Data { get; set; }
    }

    public sealed class ENetDisconnectEventArgs : ENetEvent
    {
        public ENetPeer Peer { get; set; }
        public uint Data { get; set; }
    }

    public sealed class ENetReceiveEventArgs : ENetEvent
    {
        public ENetPeer Peer { get; set; }
        public ENetPacket Packet { get; set; }
    }
}
