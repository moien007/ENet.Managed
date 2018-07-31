using System.Runtime.InteropServices;

namespace ENet.Managed.Structures
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ENetEvent
    {
        public ENetEventType Type;
        public ENetPeer* Peer;
        public byte ChannelID;
        public uint Data;
        public ENetPacket* Packet;
    }
}
