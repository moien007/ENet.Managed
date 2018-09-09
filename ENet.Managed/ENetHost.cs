using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Net;
using ENet.Managed.Platforms;
using Native = ENet.Managed.Structures;

namespace ENet.Managed
{
    public unsafe class ENetHost : IDisposable
    {
        public const int MaximumPeers = LibENet.ENET_PROTOCOL_MAXIMUM_PEER_ID;

        private Thread m_ServiceThread = null;
        private ManualResetEvent m_ServiceThreadStopEvent = null;
        private CancellationTokenSource m_ServiceThreadStopToken = null;

        private Native.ENetChecksumCallback ChecksumDelegate;
        private Native.ENetInterceptCallback m_InterceptDelegate;
        private IntPtr* m_pInterceptCallback;
        private IntPtr* m_pChecksumCallback;

        private uint* m_pTotalSentData;
        private uint* m_pTotalSentPackets;
        private uint* m_pTotalReceivedData;
        private uint* m_pTotalReceivedPackets;
        private Native.ENetAddress* m_pReceivedAddress;
        private IntPtr* m_pReceivedData;
        private UIntPtr* m_pReceivedDataLength;
        private UIntPtr* m_pConnectedPeers;

        internal List<ENetPeer> m_Peers = new List<ENetPeer>();

        public IntPtr Pointer { get; private set; }
        public ENetCompressor Compressor { get; private set; }
        public ENetChecksum Checksum { get; private set; }
        public ENetInterceptor Interceptor { get; private set; }
        public bool Disposed => Pointer == IntPtr.Zero;

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

            Pointer = LibENet.HostCreate(&enetAddress, (UIntPtr)peers, (UIntPtr)channels, (uint)incomingBandwidth, (uint)outgoingBandwidth);
            if (Pointer == IntPtr.Zero) throw new Exception("Failed to create ENet host.");

            m_pInterceptCallback = (IntPtr*)IntPtr.Add(Pointer, Native.ENetHostOffset.InterceptOffset);
            m_pChecksumCallback = (IntPtr*)IntPtr.Add(Pointer, Native.ENetHostOffset.ChecksumOffset);
            m_pTotalSentData = (uint*)IntPtr.Add(Pointer, Native.ENetHostOffset.TotalSentDataOffset);
            m_pTotalSentPackets = (uint*)IntPtr.Add(Pointer, Native.ENetHostOffset.TotalSentPacketsOffset);
            m_pTotalReceivedData = (uint*)IntPtr.Add(Pointer, Native.ENetHostOffset.TotalReceivedDataOffset);
            m_pTotalReceivedPackets = (uint*)IntPtr.Add(Pointer, Native.ENetHostOffset.TotalReceivedPacketsOffset);
            m_pReceivedAddress = (Native.ENetAddress*)IntPtr.Add(Pointer, Native.ENetHostOffset.ReceivedAddressOffset);
            m_pReceivedData = (IntPtr*)IntPtr.Add(Pointer, Native.ENetHostOffset.ReceivedDataOffset);
            m_pReceivedDataLength = (UIntPtr*)IntPtr.Add(Pointer, Native.ENetHostOffset.ReceivedDataLengthOffset);
            m_pConnectedPeers = (UIntPtr*)IntPtr.Add(Pointer, Native.ENetHostOffset.ConnectedPeersOffset);

            ChecksumDelegate = new Native.ENetChecksumCallback(ChecksumCallback);
            m_InterceptDelegate = new Native.ENetInterceptCallback(InterceptCallback);
        }

        public ENetPeer Connect(IPEndPoint endPoint, byte channels, uint data)
        {
            if (channels < 1)
                throw new ArgumentOutOfRangeException("channels");

            Native.ENetAddress address = Native.ENetAddress.FromEndPoint(endPoint);
            var native = LibENet.HostConnect(Pointer, &address, (UIntPtr)channels, data);

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

            LibENet.HostBroadcast(Pointer, channel, packet);
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

            LibENet.HostBandwidthLimit(Pointer, (uint)incomingBandwidth, (uint)outgoingBandwidth);
        }

        public void ChannelLimit(byte channels) => LibENet.HostChannelLimit(Pointer, (UIntPtr)channels);

        public ENetEvent CheckEvents()
        {
            var native = new Native.ENetEvent();
            LibENet.HostCheckEvents(Pointer, &native);
            return NativeToManagedEvent(ref native);
        }

        public void Flush() => LibENet.HostFlush(Pointer);

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

            result = LibENet.HostService(Pointer, &native, timeout);

            if (result < 0)
                throw new Exception("Service failure.");

            if (result == 0)
            {
                e = new ENetNoneEventArgs();
            }
            else
            {
                e = NativeToManagedEvent(ref native);
            }

            return true;
        }

        public void CompressWith(ENetCompressor compressor)
        {
            if (compressor.Host != null && compressor.Host != this)
            {
                throw new Exception("Compressor is already in use by another host.");
            }

            compressor.Host = this;
            Native.ENetCompressor native = new Native.ENetCompressor();
            native.Context = compressor.AllocHandle();
            native.Compress = Marshal.GetFunctionPointerForDelegate(ENetCompressor.CompressDelegate);
            native.Decompress = Marshal.GetFunctionPointerForDelegate(ENetCompressor.DecompressDelegate);
            native.Destroy = Marshal.GetFunctionPointerForDelegate(ENetCompressor.DestroyDelegate);
            LibENet.HostCompress(Pointer, &native);
            Compressor = compressor;
        }

        public void CompressWithRangeCoder()
        {
            if (LibENet.HostCompressWithRangeCoder(Pointer) != 0)
                throw new Exception("Failed to set compressor to RangeCoder.");
            Compressor = null;
        }

        public void DoNotCompress()
        {
            LibENet.HostCompress(Pointer, (Native.ENetCompressor*)IntPtr.Zero);
            Compressor = null;
        }

        public void ChecksumWith(ENetChecksum checksum)
        {
            if (checksum.Host != null && checksum.Host != this)
            {
                throw new Exception("Checksum is already in-use by another host.");
            }

            if (Checksum != null)
            {
                Checksum.Dispose();
                Checksum = null;
            }

            Checksum = checksum;
            *m_pChecksumCallback = Marshal.GetFunctionPointerForDelegate(ChecksumDelegate);
        }

        private uint ChecksumCallback(Native.ENetBuffer* buffers, UIntPtr buffersCount)
        {
            Checksum.Begin();

            for (int i = 0; i < buffersCount.ToUInt32(); i++)
            {
                var buffer = buffers[i];

                if (Checksum.Method == ENetChecksum.ChecksumMethod.Pointer)
                {
                    Checksum.Sum((byte*)buffer.Data, (int)buffer.DataLength);
                    continue;
                }

                if (Checksum.Method == ENetChecksum.ChecksumMethod.Array)
                {
                    byte[] input = new byte[buffer.DataLength.ToUInt32()];

                    fixed (byte* dest = input)
                    {
                        Platform.Current.MemoryCopy((IntPtr)dest, buffer.Data, buffer.DataLength);
                    }

                    Checksum.Sum(input);
                    continue;
                }

                throw new NotImplementedException();
            }

            return Checksum.End();
        }

        public void ChecksumWithCRC32()
        {
            if (Checksum != null)
            {
                Checksum.Dispose();
                Checksum = null;
            }

            *m_pChecksumCallback = Platform.Current.GetProcAddress(LibENet.DllHandle, "enet_crc32");
        }

        public void DoNotChecksum()
        {
            if (Checksum != null)
            {
                Checksum.Dispose();
                Checksum = null;
            }

            *m_pChecksumCallback = IntPtr.Zero;
        }

        [Obsolete("This feature is not tested")]
        public void Intercept(ENetInterceptor interceptor)
        {
            if (Interceptor != null)
            {
                Interceptor.Dispose();
                Interceptor = null;
            }

            if (interceptor.Host != null && interceptor.Host != this)
            {
                throw new Exception("Interceptor is already in-use by another host.");
            }

            Interceptor = interceptor;
            *m_pInterceptCallback = Marshal.GetFunctionPointerForDelegate(m_InterceptDelegate);
        }

        private ENetInterceptionResult InterceptCallback(IntPtr host, Native.ENetEvent* e)
        {
            if (Interceptor.Method == ENetInterceptor.InterceptionMethod.Unmanaged)
            {
                return Interceptor.Intercept(m_pReceivedAddress, (byte**)m_pReceivedData, m_pReceivedDataLength, e);
            }

            if (Interceptor.Method == ENetInterceptor.InterceptionMethod.Managed)
            {
                IPEndPoint endPoint = (*m_pReceivedAddress).ToEndPoint();
                byte[] buffer = new byte[(*m_pReceivedDataLength).ToUInt32()];

                fixed (byte* dest = buffer)
                {
                    Platform.Current.MemoryCopy((IntPtr)dest, *m_pReceivedData, *m_pReceivedDataLength);
                }

                byte[] bufferRef = buffer;
                var result = Interceptor.Intercept(endPoint, ref bufferRef, out ENetEvent @event);

                if (result == ENetInterceptionResult.Error)
                    return result;

                if (bufferRef == buffer)
                {
                    fixed (byte* src = bufferRef)
                    {
                        Platform.Current.MemoryCopy((IntPtr)src, *m_pReceivedData, *m_pReceivedDataLength);
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
                        Platform.Current.MemoryCopy((IntPtr)src, bufferPtr, (UIntPtr)bufferRef.Length);
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

            throw new NotImplementedException(Interceptor.Method.ToString());
        }

        public void DoNotIntercept()
        {
            if (Interceptor != null)
            {
                Interceptor.Dispose();
                Interceptor = null;
            }

            *m_pInterceptCallback = IntPtr.Zero;
        }

        public void Dispose()
        {
            if (Pointer == IntPtr.Zero) return;

            try { StopServiceThread(); } catch { }

            if (Compressor != null)
            {
                Compressor.Dispose();
                Compressor = null;
            }

            if (Checksum != null)
            {
                Checksum.Dispose();
                Checksum = null;
            }

            if (Interceptor != null)
            {
                Interceptor.Dispose();
                Interceptor = null;
            }

            m_Peers.ForEach(p => p.FreeHandle());
            LibENet.HostDestroy(Pointer);
            Pointer = IntPtr.Zero;
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

        private ENetEvent NativeToManagedEvent(ref Native.ENetEvent native)
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
