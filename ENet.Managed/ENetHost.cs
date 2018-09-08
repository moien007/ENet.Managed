using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Net;
using Native = ENet.Managed.Structures;

namespace ENet.Managed
{
    public unsafe class ENetHost : IDisposable
    {
        public const int MaximumPeers = LibENet.ENET_PROTOCOL_MAXIMUM_PEER_ID;

        private ENetCompressor m_Compressor = null;
        private ENetChecksum m_Checksum = null;
        private ENetInterceptor m_Interceptor = null;

        private Thread m_ServiceThread = null;
        private ManualResetEvent m_ServiceThreadStopEvent = null;
        private CancellationTokenSource m_ServiceThreadStopToken = null;

        private Native.ENetChecksumCallback m_ChecksumDelegate;
        private Native.ENetInterceptCallback m_InterceptDelegate;
        private IntPtr* m_pInterceptCallback;
        private IntPtr* m_pChecksumCallback;

        private IntPtr m_Pointer;
        private uint* m_pTotalSentData;
        private uint* m_pTotalSentPackets;
        private uint* m_pTotalReceivedData;
        private uint* m_pTotalReceivedPackets;
        private Native.ENetAddress* m_pReceivedAddress;
        private IntPtr* m_pReceivedData;
        private UIntPtr* m_pReceivedDataLength;
        private UIntPtr* m_pConnectedPeers;

        internal List<ENetPeer> m_Peers = new List<ENetPeer>();

        public IntPtr Pointer => m_Pointer;
        public bool Disposed => Pointer == IntPtr.Zero;
        public ENetCompressor Compressor =>  m_Compressor;
        public ENetChecksum Checksum => m_Checksum;
        public ENetInterceptor Interceptor => m_Interceptor;
        public long TotalSentData => *m_pTotalSentData; 
        public long TotalSentPackets => *m_pTotalSentPackets;
        public long TotalReceivedData => *m_pTotalReceivedData;
        public long TotalReceivedPackets => *m_pTotalReceivedPackets;
        public int ConnectedPeers => (int)(*m_pConnectedPeers);
        public bool ServiceThreadStarted => (m_ServiceThread != null);

        public event EventHandler<ENetConnectEventArgs> OnConnect;

        public ENetHost(int peers, byte channels) : this(new IPEndPoint(IPAddress.Any, 0), peers, channels) { }
        public ENetHost(IPEndPoint address, int peers, byte channels) : this(address, peers, channels, 0, 0) { }
        public ENetHost(IPEndPoint address, int peers, byte channels, long incomingBandwidth, long outgoingBandwidth)
        {
            if (incomingBandwidth < uint.MinValue || incomingBandwidth > uint.MaxValue)
                throw new ArgumentOutOfRangeException("incomingBandwidth");

            if (outgoingBandwidth < uint.MinValue || outgoingBandwidth > uint.MaxValue)
                throw new ArgumentOutOfRangeException("outgoingBandwidth");

            if (peers < uint.MinValue || peers > LibENet.ENET_PROTOCOL_MAXIMUM_PEER_ID)
                throw new ArgumentOutOfRangeException(string.Format("Maximum peers count is {0}", LibENet.ENET_PROTOCOL_MAXIMUM_PEER_ID));

            var enetAddress = Native.ENetAddress.FromEndPoint(address);

            m_Pointer = LibENet.HostCreate(&enetAddress, (UIntPtr)peers, (UIntPtr)channels, (uint)incomingBandwidth, (uint)outgoingBandwidth);
            if (m_Pointer == IntPtr.Zero) throw new Exception("Failed to create ENet host.");

            m_pInterceptCallback = (IntPtr*)IntPtr.Add(m_Pointer, Native.ENetHost.InterceptOffset);
            m_pChecksumCallback = (IntPtr*)IntPtr.Add(m_Pointer, Native.ENetHost.ChecksumOffset);
            m_pTotalSentData = (uint*)IntPtr.Add(m_Pointer, Native.ENetHost.TotalSentDataOffset);
            m_pTotalSentPackets = (uint*)IntPtr.Add(m_Pointer, Native.ENetHost.TotalSentPacketsOffset);
            m_pTotalReceivedData = (uint*)IntPtr.Add(m_Pointer, Native.ENetHost.TotalReceivedDataOffset);
            m_pTotalReceivedPackets = (uint*)IntPtr.Add(m_Pointer, Native.ENetHost.TotalReceivedPacketsOffset);
            m_pReceivedAddress = (Native.ENetAddress*)IntPtr.Add(m_Pointer, Native.ENetHost.ReceivedAddressOffset);
            m_pReceivedData = (IntPtr*)IntPtr.Add(m_Pointer, Native.ENetHost.ReceivedDataOffset);
            m_pReceivedDataLength = (UIntPtr*)IntPtr.Add(m_Pointer, Native.ENetHost.ReceivedDataLengthOffset);
            m_pConnectedPeers = (UIntPtr*)IntPtr.Add(m_Pointer, Native.ENetHost.ConnectedPeersOffset);

            m_ChecksumDelegate = new Native.ENetChecksumCallback(ChecksumCallback);
            m_InterceptDelegate = new Native.ENetInterceptCallback(InterceptCallback);
        }

        public ENetPeer Connect(IPEndPoint endPoint, byte channels, uint data)
        {
            if (channels < 1)
                throw new ArgumentOutOfRangeException("channels");

            Native.ENetAddress address = Native.ENetAddress.FromEndPoint(endPoint);
            var native = LibENet.HostConnect(m_Pointer, &address, (UIntPtr)channels, data);

            if (((IntPtr)native) == IntPtr.Zero)
                throw new Exception("Failed to initiate connection.");

            return new ENetPeer(this, native);
        }

        public void Broadcast(byte[] buffer, Enum channel, ENetPacketFlags flags) => Broadcast(buffer, Convert.ToByte(channel), flags);
        public void Broadcast(byte[] buffer, byte channel, ENetPacketFlags flags)
        {
            Native.ENetPacket* packet;
            fixed (byte* p = buffer)
            {
                packet = LibENet.PacketCreate((IntPtr)p, (UIntPtr)buffer.Length, flags & ~ENetPacketFlags.NoAllocate);
            }

            LibENet.HostBroadcast(m_Pointer, channel, packet);
        }


        public void Multicast(byte[] buffer, Enum channel, ENetPacketFlags flags, IEnumerable<ENetPeer> peers) =>
            Multicast(buffer, Convert.ToByte(channel), flags, peers, null);

        public void Multicast(byte[] buffer, Enum channel, ENetPacketFlags flags, IEnumerable<ENetPeer> peers, ENetPeer except) =>
            Multicast(buffer, Convert.ToByte(channel), flags, peers, except);

        public void Multicast(byte[] buffer, byte channel, ENetPacketFlags flags, IEnumerable<ENetPeer> peers) =>
            Multicast(buffer, channel, flags, peers, null);

        public void Multicast(byte[] buffer, byte channel, ENetPacketFlags flags, IEnumerable<ENetPeer> peers, ENetPeer except)
        {
            Native.ENetPacket* packet;

            fixed (byte* p = buffer)
            {
                packet = LibENet.PacketCreate((IntPtr)p, (UIntPtr)buffer.Length, flags & ~ENetPacketFlags.NoAllocate);
            }

            foreach (var peer in peers)
            {
                if (peer == null)
                    throw new NullReferenceException();

                if (peer.Host != this)
                    throw new ENetMulticastException("Speicfied peer is not of this host.", peer);

                if (peer == except) continue;

                if (peer.Unsafe->State != ENetPeerState.Connected) continue;

                if (LibENet.PeerSend(peer.Unsafe, channel, packet) != 0)
                    throw new ENetMulticastException("Failed to send packet to speicfied peer.", peer);
            }

            if (packet->ReferenceCount.ToUInt32() == 0)
                LibENet.PacketDestroy(packet);
        }

        public void BandwidthLimit(long incomingBandwidth, long outgoingBandwidth)
        {
            if (incomingBandwidth < uint.MinValue || incomingBandwidth > uint.MaxValue)
                throw new ArgumentOutOfRangeException("incomingBandwidth");

            if (outgoingBandwidth < uint.MinValue || outgoingBandwidth > uint.MaxValue)
                throw new ArgumentOutOfRangeException("outgoingBandwidth");

            LibENet.HostBandwidthLimit(m_Pointer, (uint)incomingBandwidth, (uint)outgoingBandwidth);
        }

        public void ChannelLimit(byte channels) => LibENet.HostChannelLimit(m_Pointer, (UIntPtr)channels);

        public ENetEvent CheckEvents()
        {
            var native = new Native.ENetEvent();
            LibENet.HostCheckEvents(m_Pointer, &native);
            return NativeToManagedEvent(native);
        }

        public void Flush() => LibENet.HostFlush(m_Pointer);

        public void Service() => Service(1);
        public void Service(uint timeout)
        {
            while (Service(out ENetEvent e, timeout))
            {
                if (e is ENetNoneEventArgs) continue;

                if (e is ENetConnectEventArgs)
                {
                    OnConnect?.Invoke(this, e as ENetConnectEventArgs);
                    continue;
                }

                if (e is ENetDisconnectEventArgs)
                {
                    var disconnect = e as ENetDisconnectEventArgs;
                    disconnect.Peer.RaiseDisconnectEvent(disconnect.Data);
                    continue;
                }

                if (e is ENetReceiveEventArgs)
                {
                    var receive = e as ENetReceiveEventArgs;
                    receive.Peer.RaiseReceiveEvent(receive.Packet);
                    continue;
                }
            }
        }

        public bool Service(out ENetEvent e, uint timeout)
        {
            e = null;
            var native = new Native.ENetEvent();
            int result;

            result = LibENet.HostService(m_Pointer, &native, timeout);

            if (result < 0)
                throw new Exception("Service failure.");

            if (result == 0)
            {
                e = new ENetNoneEventArgs();
            }
            else
            {
                e = NativeToManagedEvent(native);
            }

            return true;
        }

        public void CompressWith(ENetCompressor compressor)
        {
            if (compressor.Host != null && compressor.Host != this)
            {
                throw new Exception("Compressor is already in use by another host.");
            }

            Native.ENetCompressor native = new Native.ENetCompressor();
            native.Context = compressor.AllocHandle();
            native.Compress = Marshal.GetFunctionPointerForDelegate(ENetCompressor.CompressDelegate);
            native.Decompress = Marshal.GetFunctionPointerForDelegate(ENetCompressor.DecompressDelegate);
            native.Destroy = Marshal.GetFunctionPointerForDelegate(ENetCompressor.DestroyDelegate);
            LibENet.HostCompress(m_Pointer, &native);
            m_Compressor = compressor;
        }

        public void CompressWithRangeCoder()
        {
            if (LibENet.HostCompressWithRangeCoder(m_Pointer) != 0)
                throw new Exception("Failed to set compressor to RangeCoder.");
            m_Compressor = null;
        }

        public void DoNotCompress()
        {
            LibENet.HostCompress(m_Pointer, (Native.ENetCompressor*)IntPtr.Zero);
            m_Compressor = null;
        }

        public void ChecksumWith(ENetChecksum checksum)
        {
            if (checksum.Host != null && checksum.Host != this)
            {
                throw new Exception("Checksum is already in-use by another host.");
            }

            if (m_Checksum != null)
            {
                m_Checksum.Dispose();
                m_Checksum = null;
            }

            m_Checksum = checksum;
            *m_pChecksumCallback = Marshal.GetFunctionPointerForDelegate(m_ChecksumDelegate);
        }

        private uint ChecksumCallback(Native.ENetBuffer* buffers, UIntPtr buffersCount)
        {
            m_Checksum.Begin();

            for (int i = 0; i < buffersCount.ToUInt32(); i++)
            {
                var buffer = buffers[i];

                if (m_Checksum.Method == ENetChecksumMethod.Pointer)
                {
                    m_Checksum.Sum((byte*)buffer.Data, (int)buffer.DataLength);
                    continue;
                }

                if (m_Checksum.Method == ENetChecksumMethod.Array)
                {
                    byte[] input = new byte[buffer.DataLength.ToUInt32()];

                    fixed (byte* dest = input)
                    {
                        ENetUtils.MemoryCopy((IntPtr)dest, buffer.Data, buffer.DataLength);
                    }

                    m_Checksum.Sum(input);
                    continue;
                }

                throw new NotImplementedException();
            }

            return m_Checksum.End();
        }

        public void ChecksumWithCRC32()
        {
            if (m_Checksum != null)
            {
                m_Checksum.Dispose();
                m_Checksum = null;
            }

            *m_pChecksumCallback = Win32.GetProcAddress(LibENet.DllHandle, "enet_crc32");
        }

        public void DoNotChecksum()
        {
            if (m_Checksum != null)
            {
                m_Checksum.Dispose();
                m_Checksum = null;
            }

            *m_pChecksumCallback = IntPtr.Zero;
        }

        [Obsolete("This feature is not tested")]
        public void Intercept(ENetInterceptor interceptor)
        {
            if (Interceptor != null)
            {
                m_Interceptor.Dispose();
                m_Interceptor = null;
            }

            if (interceptor.Host != null && interceptor.Host != this)
            {
                throw new Exception("Interceptor is already in-use by another host.");
            }

            m_Interceptor = interceptor;
            *m_pInterceptCallback = Marshal.GetFunctionPointerForDelegate(m_InterceptDelegate);
        }

        private ENetInterceptionResult InterceptCallback(IntPtr host, Native.ENetEvent* e)
        {
            if (m_Interceptor.Method == ENetInterceptionMethod.Unmanaged)
            {
                return m_Interceptor.Intercept(m_pReceivedAddress, (byte**)m_pReceivedData, m_pReceivedDataLength, e);
            }

            if (m_Interceptor.Method == ENetInterceptionMethod.Managed)
            {
                IPEndPoint endPoint = (*m_pReceivedAddress).ToEndPoint();
                byte[] buffer = new byte[(*m_pReceivedDataLength).ToUInt32()];

                fixed (byte* dest = buffer)
                {
                    Win32.MemoryCopy((IntPtr)dest, *m_pReceivedData, *m_pReceivedDataLength);
                }

                byte[] bufferRef = buffer;
                var result = m_Interceptor.Intercept(endPoint, ref bufferRef, out ENetEvent @event);

                if (result == ENetInterceptionResult.Error)
                    return result;

                if (bufferRef == buffer)
                {
                    fixed (byte* src = bufferRef)
                    {
                        ENetUtils.MemoryCopy((IntPtr)src, *m_pReceivedData, *m_pReceivedDataLength);
                    }
                }
                else if (bufferRef != null)
                {
                    Marshal.FreeHGlobal(*m_pReceivedData);
                    var bufferPtr = Marshal.AllocHGlobal(bufferRef.Length);
                    *m_pReceivedData = bufferPtr;
                    *m_pReceivedDataLength = (UIntPtr)bufferRef.Length;

                    fixed (byte* src = bufferRef)
                    {
                        ENetUtils.MemoryCopy((IntPtr)src, bufferPtr, (UIntPtr)bufferRef.Length);
                    }
                }

                if (result == ENetInterceptionResult.Ignore)
                    return result;

                if (@event == null)
                {
                    e->Type = ENetEventType.None;
                    return result;
                }

                if (@event is ENetConnectEventArgs)
                {
                    var connect = @event as ENetConnectEventArgs;
                    e->Type = ENetEventType.Connect;
                    e->Peer = connect.Peer.Unsafe;
                    e->Data = connect.Data;
                    return result;
                }

                if (@event is ENetDisconnectEventArgs)
                {
                    var disconnect = @event as ENetConnectEventArgs;
                    e->Type = ENetEventType.Disconnect;
                    e->Peer = disconnect.Peer.Unsafe;
                    e->Data = disconnect.Data;
                    return result;
                }

                if (@event is ENetReceiveEventArgs)
                {
                    var receive = @event as ENetReceiveEventArgs;
                    e->Type = ENetEventType.Receive;
                    e->Peer = receive.Peer.Unsafe;
                    e->ChannelID = receive.Packet.Channel;
                    fixed (byte* data = receive.Packet.m_Payload)
                    {
                        e->Packet = LibENet.PacketCreate((IntPtr)data, (UIntPtr)receive.Packet.m_Payload.Length, receive.Packet.Flags & ~ENetPacketFlags.NoAllocate);
                    }
                    return result;
                }
            }

            throw new NotImplementedException(m_Interceptor.Method.ToString());
        }

        public void DoNotIntercept()
        {
            if (Interceptor != null)
            {
                m_Interceptor.Dispose();
                m_Interceptor = null;
            }

            *m_pInterceptCallback = IntPtr.Zero;
        }

        public void Dispose()
        {
            if (m_Pointer == IntPtr.Zero) return;

            try { StopServiceThread(); } catch { }

            if (Compressor != null)
            {
                m_Compressor.Dispose();
                m_Compressor = null;
            }

            if (Checksum != null)
            {
                m_Checksum.Dispose();
                m_Checksum = null;
            }

            if (Interceptor != null)
            {
                m_Interceptor.Dispose();
                m_Interceptor = null;
            }

            m_Peers.ForEach(p => p.FreeHandle());
            LibENet.HostDestroy(m_Pointer);
            m_Pointer = IntPtr.Zero;
        }

        public void StartServiceThread()
        {
            if (m_ServiceThread != null)
                throw new Exception("Servicing thread is already started.");

            m_ServiceThreadStopEvent = new ManualResetEvent(false);
            m_ServiceThreadStopToken = new CancellationTokenSource();
            m_ServiceThread = new Thread(ServiceThread);
            m_ServiceThread.IsBackground = true;
            m_ServiceThread.Start();
        }

        public void StopServiceThread()
        {
            if (m_ServiceThread == null)
                throw new Exception("Servicing thread is already stopped.");

            m_ServiceThreadStopToken.Cancel();
            m_ServiceThreadStopEvent.WaitOne();

            m_ServiceThreadStopToken.Dispose();
            m_ServiceThreadStopToken = null;
            m_ServiceThreadStopEvent.Dispose();
            m_ServiceThreadStopEvent = null;
            m_ServiceThread = null;
        }

        private void ServiceThread()
        {
            while (!m_ServiceThreadStopToken.IsCancellationRequested)
            {
                Service();
            }

            m_ServiceThreadStopEvent.Set();
        }

        private ENetEvent NativeToManagedEvent(Native.ENetEvent native)
        {
            switch (native.Type)
            {
                case ENetEventType.None:
                    return null;

                case ENetEventType.Connect:
                    var connect = new ENetConnectEventArgs();
                    if (native.Peer->Data == IntPtr.Zero)
                    {
                        connect.Peer = new ENetPeer(this, native.Peer);
                    }
                    else
                    {
                        connect.Peer = ENetPeer.FromPtr(native.Peer->Data);
                    }
                    connect.Peer.RemoteEndPoint = native.Peer->Address.ToEndPoint();
                    connect.Data = native.Data;
                    return connect;

                case ENetEventType.Disconnect:
                    if (native.Peer->Data == IntPtr.Zero)
                        throw new NullReferenceException("Peer->Data");

                    var disconnect = new ENetDisconnectEventArgs();
                    disconnect.Peer = ENetPeer.FromPtr(native.Peer->Data);
                    disconnect.Data = native.Data;
                    disconnect.Peer.FreeHandle();
                    return disconnect;

                case ENetEventType.Receive:
                    if (native.Peer->Data == IntPtr.Zero)
                        throw new NullReferenceException("Peer->Data");

                    var receive = new ENetReceiveEventArgs();
                    receive.Peer = ENetPeer.FromPtr(native.Peer->Data);
                    receive.Packet = new ENetPacket(native.Packet, native.ChannelID);
                    LibENet.PacketDestroy(native.Packet);
                    return receive;

                default:
                    throw new NotImplementedException(native.Type.ToString());
            }
        }
    }
}
