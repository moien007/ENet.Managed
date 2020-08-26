using System;
using System.Runtime.CompilerServices;

using ENet.Managed.Native;

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

        /// <summary>
        /// Disptaches this event to the specified listener based on the event type.
        /// </summary>
        /// <param name="listener">Listener to disptach event to.</param>
        /// <returns>Returns true if any event disptached to listener; otherwise false.</returns>
        public bool DisptachTo(IENetEventListener listener)
        {
            if (listener is null)
            {
                throw new ArgumentNullException(nameof(listener));
            }

            switch (Type)
            {
                case ENetEventType.None:
                    return false;

                case ENetEventType.Connect:
                    listener.OnConnect(Peer, Data);
                    return true;

                case ENetEventType.Disconnect:
                    listener.OnDisconnect(Peer, Data);
                    return true;

                case ENetEventType.Receive:
                    listener.OnReceive(Peer, Packet, ChannelId);
                    return true;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
