using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
