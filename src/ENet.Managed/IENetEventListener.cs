namespace ENet.Managed
{
    /// <summary>
    /// An interface event listener which receives dispatched ENet events. 
    /// </summary>
    public interface IENetEventListener
    {
        void OnConnect(ENetPeer peer, uint data);
        void OnDisconnect(ENetPeer peer, uint data);
        void OnReceive(ENetPeer peer, ENetPacket packet, byte channelId);
    }
}
