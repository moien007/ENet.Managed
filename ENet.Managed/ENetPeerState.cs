namespace ENet.Managed
{
    public enum ENetPeerState
    {
        Disconnected,
        Connecting,
        AcknowledgingConnect,
        ConnectionPending,
        ConnectingSucceeded,
        Connected,
        DisconnectLater,
        Disconnecting,
        AcknowledgingDisconnect,
        Zombie,
    }
}
