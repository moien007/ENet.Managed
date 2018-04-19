using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Native = ENet.Managed.Structures;
using System.Net;

namespace ENet.Managed
{
    public unsafe class ENetPeer
    {
        public ENetHost Host { get; private set; }
        public IntPtr Handle { get; private set; }
        public Native.ENetPeer* Unsafe { get; private set; }
        public ENetPeerState State { get { lock (Host.Sync) return Unsafe->State; } }
        public object Data { get; set; }
        public IPEndPoint RemoteEndPoint { get; internal set; }

        internal ENetPeer(ENetHost host, Native.ENetPeer* native)
        {
            Host = host;
            host._Peers.Add(this);
            Unsafe = native;
            Handle = GCHandle.ToIntPtr(GCHandle.Alloc(this, GCHandleType.Normal));
            Unsafe->Data = Handle;
        }

        public void Disconnect(uint data)
        {
            lock (Host.Sync)
            {
                LibENet.PeerDisconnect(Unsafe, data);
            }
        }

        public void DisconnectLater(uint data)
        {
            lock (Host.Sync)
            {
                LibENet.PeerDisconnectLater(Unsafe, data);
            }
        }

        public void DisconnectNow(uint data)
        {
            lock (Host.Sync)
            {
                LibENet.PeerDisconnectNow(Unsafe, data);
                FreeHandle();
            }
        }

        public void Reset()
        {
            lock (Host.Sync)
            {
                LibENet.PeerReset(Unsafe);
                FreeHandle();
            }
        }

        public void Ping()
        {
            lock (Host.Sync)
            {
                LibENet.PeerPing(Unsafe);
            }
        }

        public void PingInterval(uint pingInterval)
        {
            lock (Host.Sync)
            {
                LibENet.PeerPingInterval(Unsafe, pingInterval);
            }
        }

        public void Send(byte[] buffer, Enum channel, ENetPacketFlags flags)
        {
            Send(buffer, Convert.ToByte(channel), flags);
        }

        public void Send(byte[] buffer, byte channel, ENetPacketFlags flags)
        {
            lock (Host.Sync)
            {
                Native.ENetPacket* packet;

                fixed (byte* p = buffer)
                {
                    packet = LibENet.PacketCreate((IntPtr)p, (UIntPtr)buffer.Length, flags & ~ENetPacketFlags.NoAllocate);
                }

                if (LibENet.PeerSend(Unsafe, channel, packet) < 0)
                    throw new Exception("Failed to send packet to peer.");
            }
        }

        public bool Receive(out ENetPacket packet)
        {
            lock (Host.Sync)
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
        }

        public void ThrottleConfigure(uint interval, uint acceleration, uint deceleration)
        {
            lock (Host.Sync)
            {
                LibENet.PeerThrottleConfigure(Unsafe, interval, acceleration, deceleration);
            }
        }

        public void Timeout(uint timeoutLimit, uint timeoutMinimum, uint timeoutMaximum)
        {
            lock (Host.Sync)
            {
                LibENet.PeerTimeout(Unsafe, timeoutLimit, timeoutMinimum, timeoutMaximum);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetDataAs<T>()
        {
            return (T)Data;
        }

        internal void FreeHandle()
        {
            if (Handle == IntPtr.Zero)
                return;

            Host._Peers.Remove(this);
            Unsafe->Data = IntPtr.Zero;
            GCHandle.FromIntPtr(Handle).Free();
            Handle = IntPtr.Zero;
        }

        internal static ENetPeer FromPtr(IntPtr ptr)
        {
            var handle = GCHandle.FromIntPtr(ptr);
            return (ENetPeer)handle.Target;
        }
    }
}
