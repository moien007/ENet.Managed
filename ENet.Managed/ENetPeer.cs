using System;
using System.Runtime.InteropServices;
using Native = ENet.Managed.Structures;
using System.Net;

namespace ENet.Managed
{
    public unsafe class ENetPeer
    {
        internal GCHandle Handle;

        public ENetHost Host { get; }
        public Native.ENetPeer* Unsafe { get; private set; }
        public ENetPeerState State => Unsafe->State;
        public object Data { get; set; }
        public IPEndPoint RemoteEndPoint { get; internal set; }

        public event EventHandler<ENetPacket> OnReceive;
        public event EventHandler<uint> OnDisconnect; 

        internal ENetPeer(ENetHost host, Native.ENetPeer* native)
        {
            Host = host;
            host.m_Peers.Add(this);
            Unsafe = native;
            Handle = GCHandle.Alloc(this, GCHandleType.Normal);
            Unsafe->Data = GCHandle.ToIntPtr(Handle);
        }

        public void Disconnect(uint data) => LibENet.PeerDisconnect(Unsafe, data);
        public void DisconnectLater(uint data) => LibENet.PeerDisconnectLater(Unsafe, data);
        public void DisconnectNow(uint data)
        {
            LibENet.PeerDisconnectNow(Unsafe, data);
            FreeHandle();
        }

        public void Reset()
        {
            LibENet.PeerReset(Unsafe);
            FreeHandle();
        }

        public void Ping()
        {
            LibENet.PeerPing(Unsafe);
        }

        public void PingInterval(uint pingInterval)
        {
            LibENet.PeerPingInterval(Unsafe, pingInterval);
        }

        public void Send(byte[] buffer, Enum channel, ENetPacketFlags flags)
        {
            Send(buffer, Convert.ToByte(channel), flags);
        }

        public void Send(byte[] buffer, byte channel, ENetPacketFlags flags)
        {
            Native.ENetPacket* packet;

            fixed (byte* p = buffer)
            {
                packet = LibENet.PacketCreate((IntPtr)p, (UIntPtr)buffer.Length, flags & ~ENetPacketFlags.NoAllocate);
            }

            if (LibENet.PeerSend(Unsafe, channel, packet) < 0)
                throw new Exception("Failed to send packet to peer.");
        }

        public bool Receive(out ENetPacket packet)
        {
            byte channel = 0;
            packet = null;
            Native.ENetPacket* native;

            native = LibENet.PeerReceive(Unsafe, &channel);

            if (((IntPtr)native) == IntPtr.Zero)
                return false;

            packet = new ENetPacket(native, channel);
            LibENet.PacketDestroy(native);
            return true;
        }

        public void ThrottleConfigure(uint interval, uint acceleration, uint deceleration) =>
                LibENet.PeerThrottleConfigure(Unsafe, interval, acceleration, deceleration);


        public void Timeout(uint timeoutLimit, uint timeoutMinimum, uint timeoutMaximum) =>
                LibENet.PeerTimeout(Unsafe, timeoutLimit, timeoutMinimum, timeoutMaximum);


        internal void RaiseReceiveEvent(ENetPacket packet) => OnReceive?.Invoke(this, packet);
        internal void RaiseDisconnectEvent(uint data) => OnDisconnect?.Invoke(this, data);

        internal void FreeHandle()
        {
            if (Handle == default(GCHandle))
                return;

            Host.m_Peers.Remove(this);
            Unsafe->Data = IntPtr.Zero;
            Handle.Free();
            Handle = default(GCHandle);
        }

        internal static ENetPeer FromPtr(IntPtr ptr)
        {
            var handle = GCHandle.FromIntPtr(ptr);
            return (ENetPeer)handle.Target;
        }
    }
}
