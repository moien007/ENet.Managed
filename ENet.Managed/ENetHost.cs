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
        private ENetCompressor _Compressor = null;
        private ENetChecksum _Checksum = null;
        private ENetInterceptor _Interceptor = null;

        private Thread _Thread = null;
        private ManualResetEvent _ThreadStopEvent = null;
        private CancellationTokenSource _ThreadStopToken = null;

        private Native.ENetChecksumCallback ChecksumDelegate;
        private Native.ENetInterceptCallback InterceptDelegate;
        private IntPtr* _pInterceptCallback;
        private IntPtr* _pChecksumCallback;

        private uint* _pTotalSentData;
        private uint* _pTotalSentPackets;
        private uint* _pTotalReceivedData;
        private uint* _pTotalReceivedPackets;
        private Native.ENetAddress* _pReceivedAddress;
        private IntPtr* _pReceivedData;
        private UIntPtr* _pReceivedDataLength;
        private UIntPtr* _pConnectedPeers;


        internal List<ENetPeer> _Peers = new List<ENetPeer>();

        public IntPtr Pointer { get; private set; }
        public ENetCompressor Compressor { get { lock (Sync) return _Compressor; } }
        public ENetChecksum Checksum { get { lock (Sync) return _Checksum; }  }
        public ENetInterceptor Interceptor { get { lock (Sync) return _Interceptor; } }
        public long TotalSentData { get { lock (Sync) return *_pTotalSentData; } }
        public long TotalSentPackets { get { lock (Sync) return *_pTotalSentPackets; } }
        public long TotalReceivedData { get { lock (Sync) return *_pTotalReceivedData; } }
        public long TotalReceivedPackets { get { lock (Sync) return *_pTotalReceivedPackets; } }
        public int ConnectedPeers { get { lock (Sync) return (int)(*_pConnectedPeers); } }
        public bool ServiceThreadStarted { get { lock (Sync) return _Thread != null; } }

        public event EventHandler<ENetConnectEventArgs> OnConnect;
        public event EventHandler<ENetDisconnectEventArgs> OnDisconnect;
        public event EventHandler<ENetReceiveEventArgs> OnReceive;

        public readonly object Sync = new object();


        public ENetHost(int peers, byte channels) : this(new IPEndPoint(IPAddress.Any, 0), peers, channels)
        {
            
        }

        public ENetHost(IPEndPoint address, int peers, byte channels) : this(address, peers, channels, 0, 0)
        {

        }

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

            _pInterceptCallback = (IntPtr*)IntPtr.Add(Pointer, Native.ENetHost.InterceptOffset);
            _pChecksumCallback = (IntPtr*)IntPtr.Add(Pointer, Native.ENetHost.ChecksumOffset);
            _pTotalSentData = (uint*)IntPtr.Add(Pointer, Native.ENetHost.TotalSentDataOffset);
            _pTotalSentPackets = (uint*)IntPtr.Add(Pointer, Native.ENetHost.TotalSentPacketsOffset);
            _pTotalReceivedData = (uint*)IntPtr.Add(Pointer, Native.ENetHost.TotalReceivedDataOffset);
            _pTotalReceivedPackets = (uint*)IntPtr.Add(Pointer, Native.ENetHost.TotalReceivedPacketsOffset);
            _pReceivedAddress = (Native.ENetAddress*)IntPtr.Add(Pointer, Native.ENetHost.ReceivedAddressOffset);
            _pReceivedData = (IntPtr*)IntPtr.Add(Pointer, Native.ENetHost.ReceivedDataOffset);
            _pReceivedDataLength = (UIntPtr*)IntPtr.Add(Pointer, Native.ENetHost.ReceivedDataLengthOffset);
            _pConnectedPeers = (UIntPtr*)IntPtr.Add(Pointer, Native.ENetHost.ConnectedPeersOffset);

            ChecksumDelegate = new Native.ENetChecksumCallback(ChecksumCallback);
            InterceptDelegate = new Native.ENetInterceptCallback(InterceptCallback);
        }

        public ENetPeer Connect(IPEndPoint endPoint, byte channels, uint data)
        {
            lock (Sync)
            {
                if (channels < 1)
                    throw new ArgumentOutOfRangeException("channels");

                Native.ENetAddress address = Native.ENetAddress.FromEndPoint(endPoint);
                var native = LibENet.HostConnect(Pointer, &address, (UIntPtr)channels, data);

                if (((IntPtr)native) == IntPtr.Zero)
                    throw new Exception("Failed to initiate connection.");

                return new ENetPeer(this, native);
            }
        }

        public void Broadcast(byte[] buffer, Enum channel, ENetPacketFlags flags)
        {
            Broadcast(buffer, Convert.ToByte(channel), flags);
        }

        public void Broadcast(byte[] buffer, byte channel, ENetPacketFlags flags)
        {
            lock (Sync)
            {
                Native.ENetPacket* packet;

                fixed (byte* p = buffer)
                {
                    packet = LibENet.PacketCreate((IntPtr)p, (UIntPtr)buffer.Length, flags & ~ENetPacketFlags.NoAllocate);
                }
                LibENet.HostBroadcast(Pointer, channel, packet);
            }
        }

        public void Multicast(byte[] buffer, Enum channel, ENetPacketFlags flags, IEnumerable<ENetPeer> peers)
        {
            Multicast(buffer, Convert.ToByte(channel), flags, peers);
        }

        public void Multicast(byte[] buffer, byte channel, ENetPacketFlags flags, IEnumerable<ENetPeer> peers)
        {
            lock (Sync)
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

                    if (peer.Unsafe->State != ENetPeerState.Connected) continue;

                    if (LibENet.PeerSend(peer.Unsafe, channel, packet) != 0)
                        throw new ENetMulticastException("Failed to send packet to speicfied peer.", peer);
                }

                if (packet->ReferenceCount.ToUInt32() == 0)
                    LibENet.PacketDestroy(packet);
            }
        }

        public void BandwidthLimit(long incomingBandwidth, long outgoingBandwidth)
        {
            lock (Sync)
            {
                if (incomingBandwidth < uint.MinValue || incomingBandwidth > uint.MaxValue)
                    throw new ArgumentOutOfRangeException("incomingBandwidth");

                if (outgoingBandwidth < uint.MinValue || outgoingBandwidth > uint.MaxValue)
                    throw new ArgumentOutOfRangeException("outgoingBandwidth");

                LibENet.HostBandwidthLimit(Pointer, (uint)incomingBandwidth, (uint)outgoingBandwidth);
            }
        }

        public void ChannelLimit(byte channels)
        {
            lock (Sync)
            {
                LibENet.HostChannelLimit(Pointer, (UIntPtr)channels);
            }
        }

        public ENetEvent CheckEvents()
        {
            lock (Sync)
            {
                var native = new Native.ENetEvent();

                LibENet.HostCheckEvents(Pointer, &native);

                return NativeToManagedEvent(native);
            }
        }

        public void Flush()
        {
            lock (Sync)
            {
                LibENet.HostFlush(Pointer);
            }
        }

        public bool Service()
        {
            return Service(1);
        }

        public bool Service(uint timeout)
        {
            lock (Sync)
            {
                ENetEvent e;

                if (!Service(out e, timeout)) return false;

                if (e is ENetConnectEventArgs)
                {
                    OnConnect?.Invoke(this, e as ENetConnectEventArgs);
                    return true;
                }

                if (e is ENetDisconnectEventArgs)
                {
                    OnDisconnect?.Invoke(this, e as ENetDisconnectEventArgs);
                    return true;
                }

                if (e is ENetReceiveEventArgs)
                {
                    OnReceive?.Invoke(this, e as ENetReceiveEventArgs);
                    return true;
                }

                throw new NotImplementedException();
            }
        }

        public bool Service(out ENetEvent e, uint timeout)
        {
            lock (Sync)
            {
                e = null;
                var native = new Native.ENetEvent();
                int service;

                service = LibENet.HostService(Pointer, &native, timeout);

                if (service == 0)
                {
                    return false;
                }

                if (service < 0)
                {
                    throw new Exception("Service failure.");
                }

                e = NativeToManagedEvent(native);
                return e != null;
            }
        }

        public void CompressWith(ENetCompressor compressor)
        {
            lock (compressor)
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
                LibENet.HostCompress(Pointer, &native);
                _Compressor = compressor;
            }
        }

        public void CompressWithRangeCoder()
        {
            lock (Sync)
            {
                if (LibENet.HostCompressWithRangeCoder(Pointer) != 0)
                    throw new Exception("Failed to set compressor to range coder.");
                _Compressor = null;
            }
        }

        public void DoNotCompress()
        {
            lock (Sync)
            {
                LibENet.HostCompress(Pointer, (Native.ENetCompressor*)IntPtr.Zero);
                _Compressor = null;
            }
        }

        public void ChecksumWith(ENetChecksum checksum)
        {
            lock (Sync)
            {
                if (checksum.Host != null && checksum.Host != this)
                {
                    throw new Exception("Checksum is already in-use by another host.");
                }

                if (_Checksum != null)
                {
                    _Checksum.Dispose();
                    _Checksum = null;
                }

                _Checksum = checksum;
                *_pChecksumCallback = Marshal.GetFunctionPointerForDelegate(ChecksumDelegate);
            }
        }

        private uint ChecksumCallback(Native.ENetBuffer* buffers, UIntPtr buffersCount)
        {
            _Checksum.Begin();

            for (int i = 0; i < buffersCount.ToUInt32(); i++)
            {
                var buffer = buffers[i];

                if (_Checksum.Method == ENetChecksumMethod.Pointer)
                {
                    _Checksum.Sum((byte*)buffer.Data, (int)buffer.DataLength);
                    continue;
                }

                if (_Checksum.Method == ENetChecksumMethod.Array)
                {
                    byte[] input = new byte[buffer.DataLength.ToUInt32()];

                    fixed (byte* dest = input)
                    {
                        ENetUtils.MemoryCopy((IntPtr)dest, buffer.Data, buffer.DataLength);
                    }

                    _Checksum.Sum(input);
                    continue;
                }

                throw new NotImplementedException();
            }

            return _Checksum.End();
        }

        public void ChecksumWithCRC32()
        {
            lock (Sync)
            {
                if (_Checksum != null)
                {
                    _Checksum.Dispose();
                    _Checksum = null;
                }

                *_pChecksumCallback = Win32.GetProcAddress(LibENet.DllHandle, "enet_crc32");
            }
        }

        public void DoNotChecksum()
        {
            lock (Sync)
            {
                if (_Checksum != null)
                {
                    _Checksum.Dispose();
                    _Checksum = null;
                }

                *_pChecksumCallback = IntPtr.Zero;
            }
        }

        [Obsolete("This feature hasn't tested.")]
        public void Intercept(ENetInterceptor interceptor)
        {
            lock (Sync)
            {
                if (Interceptor != null)
                {
                    _Interceptor.Dispose();
                    _Interceptor = null;
                }

                if (interceptor.Host != null && interceptor.Host != this)
                {
                    throw new Exception("Interceptor is already in-use by another host.");
                }

                _Interceptor = interceptor;
                *_pInterceptCallback = Marshal.GetFunctionPointerForDelegate(InterceptDelegate);
            }
        }

        private ENetInterceptionResult InterceptCallback(IntPtr host, Native.ENetEvent* e)
        {
            if (_Interceptor.Method == ENetInterceptionMethod.Unmanaged)
            {
                return _Interceptor.Intercept(_pReceivedAddress, (byte**)_pReceivedData, _pReceivedDataLength, e);
            }

            if (_Interceptor.Method == ENetInterceptionMethod.Managed)
            {
                IPEndPoint endPoint = (*_pReceivedAddress).ToEndPoint();
                byte[] buffer = new byte[(*_pReceivedDataLength).ToUInt32()];

                fixed (byte* dest = buffer)
                {
                    Win32.MemoryCopy((IntPtr)dest, *_pReceivedData, *_pReceivedDataLength);
                }

                ENetEvent @event;
                byte[] bufferRef = buffer;
                var result = _Interceptor.Intercept(endPoint, ref bufferRef, out @event);

                if (result == ENetInterceptionResult.Error)
                    return result;

                if (bufferRef == buffer)
                {
                    fixed (byte* src = bufferRef)
                    {
                        ENetUtils.MemoryCopy((IntPtr)src, *_pReceivedData, *_pReceivedDataLength);
                    }
                }
                else if (bufferRef != null)
                {
                    Marshal.FreeHGlobal(*_pReceivedData);
                    var bufferPtr = Marshal.AllocHGlobal(bufferRef.Length);
                    *_pReceivedData = bufferPtr;
                    *_pReceivedDataLength = (UIntPtr)bufferRef.Length;

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
                    fixed (byte* data = receive.Packet._Payload)
                    {
                        e->Packet = LibENet.PacketCreate((IntPtr)data, (UIntPtr)receive.Packet._Payload.Length, receive.Packet.Flags & ~ENetPacketFlags.NoAllocate);
                    }
                    return result;
                }
            }

            throw new NotImplementedException();
        }

        public void DoNotIntercept()
        {
            lock (Sync)
            {
                if (Interceptor != null)
                {
                    _Interceptor.Dispose();
                    _Interceptor = null;
                }

                *_pInterceptCallback = IntPtr.Zero;
            }
        }

        public void Dispose()
        {
            lock (Sync)
            {
                if (Pointer == IntPtr.Zero) return;

                try { StopServiceThread(); } catch { }

                if (Compressor != null)
                {
                    _Compressor.Dispose();
                    _Compressor = null;
                }

                if (Checksum != null)
                {
                    _Checksum.Dispose();
                    _Checksum = null;
                }

                if (Interceptor != null)
                {
                    _Interceptor.Dispose();
                    _Interceptor = null;
                }

                _Peers.ForEach(p => p.FreeHandle());
                LibENet.HostDestroy(Pointer);
                Pointer = IntPtr.Zero;
            }
        }

        public void StartServiceThread()
        {
            lock (Sync)
            {
                if (_Thread != null)
                    throw new Exception("Servicing thread is already started.");

                _ThreadStopEvent = new ManualResetEvent(false);
                _ThreadStopToken = new CancellationTokenSource();
                _Thread = new Thread(ServiceThread);
                _Thread.IsBackground = true;
                _Thread.Start();
            }
        }

        public void StopServiceThread()
        {
            lock (Sync)
            {
                if (_Thread == null)
                    throw new Exception("Servicing thread is already stopped.");

                _ThreadStopToken.Cancel();
                _ThreadStopEvent.WaitOne();

                _ThreadStopToken.Dispose();
                _ThreadStopToken = null;
                _ThreadStopEvent.Dispose();
                _ThreadStopEvent = null;
                _Thread = null;
            }
        }

        private void ServiceThread()
        {
            while (!_ThreadStopToken.IsCancellationRequested)
            {
                Service();
            }

            _ThreadStopEvent.Set();
        }

        private ENetEvent NativeToManagedEvent(Native.ENetEvent native)
        {
            switch (native.Type)
            {
                case ENetEventType.None: return null;
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
                    var disconnect = new ENetDisconnectEventArgs();
                    if (native.Peer->Data == IntPtr.Zero)
                    {
                        throw new NullReferenceException("Peer->Data");
                    }
                    disconnect.Peer = ENetPeer.FromPtr(native.Peer->Data);
                    disconnect.Data = native.Data;
                    disconnect.Peer.FreeHandle();
                    return disconnect;
                case ENetEventType.Receive:
                    var receive = new ENetReceiveEventArgs();
                    if (native.Peer->Data == IntPtr.Zero)
                    {
                        throw new NullReferenceException("Peer->Data");
                    }
                    receive.Peer = ENetPeer.FromPtr(native.Peer->Data);
                    receive.Packet = new ENetPacket(native.Packet, native.ChannelID);
                    LibENet.PacketDestroy(native.Packet);
                    return receive;
                default: throw new NotImplementedException(native.Type.ToString());
            }
        }
    }
}
