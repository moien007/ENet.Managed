using ENet.Managed.Native;
using System.Runtime.CompilerServices;

namespace ENet.Managed
{
    public readonly struct ENetEvent
    {
        public ENetEventType Type { get; }
        public ENetPeer Peer { get; }
        public ENetPacket Packet { get; }
        public uint Data { get; }
        public byte ChannelId { get; }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ENetEvent(NativeENetEvent native)
        {
            Type = native.Type;
            ChannelId = native.ChannelID;
            Data = native.Data;

            if (native.Peer != null)
            {
                Peer = new ENetPeer(native.Peer);
            }
            else
            {
                Peer = default;
            }

            if (native.Packet != null)
            {
                Packet = new ENetPacket(native.Packet);
            }
            else
            {
                Packet = default;
            }
        }
    }
}
