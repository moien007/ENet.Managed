using System;
using System.Runtime.InteropServices;

#nullable disable // Notice!

namespace ENet.Managed.Native
{
    /// <summary>
    /// ENet dynamic-link library function imports.
    /// </summary>
    public unsafe static class LibENetImports
    {
        private const string MODULE_NAME = "libenet";

        [DllImport(MODULE_NAME, CallingConvention = LibENet.ENetCallingConvention, EntryPoint = "enet_initialize")]
        public static extern int Initialize();

        [DllImport(MODULE_NAME, CallingConvention = LibENet.ENetCallingConvention, EntryPoint = "enet_initialize_with_callbacks")]
        public static extern int InitializeWithCallbacks(uint version, NativeENetCallbacks* callbacks);

        [DllImport(MODULE_NAME, CallingConvention = LibENet.ENetCallingConvention, EntryPoint = "enet_deinitialize")]
        public static extern void Deinitialize();

        [DllImport(MODULE_NAME, CallingConvention = LibENet.ENetCallingConvention, EntryPoint = "enet_linked_version")]
        public static extern uint LinkedVersion();

        [DllImport(MODULE_NAME, CallingConvention = LibENet.ENetCallingConvention, EntryPoint = "enet_address_get_host")]
        public static extern int AddressGetHost(NativeENetAddress* address, IntPtr hostName, UIntPtr nameLength);

        [DllImport(MODULE_NAME, CallingConvention = LibENet.ENetCallingConvention, EntryPoint = "enet_address_get_host_ip")]
        public static extern int AddressGetHostIP(NativeENetAddress* address, IntPtr hostName, UIntPtr nameLength);

        [DllImport(MODULE_NAME, CallingConvention = LibENet.ENetCallingConvention, EntryPoint = "enet_address_set_host")]
        public static extern int AddressSetHost(NativeENetAddress* address, [MarshalAs(UnmanagedType.LPStr)] string hostName);

        [DllImport(MODULE_NAME, CallingConvention = LibENet.ENetCallingConvention, EntryPoint = "enet_address_set_host_ip")]
        public static extern int AddressSetHostIP(NativeENetAddress* address, [MarshalAs(UnmanagedType.LPStr)] string hostName);

        [DllImport(MODULE_NAME, CallingConvention = LibENet.ENetCallingConvention, EntryPoint = "enet_host_bandwidth_limit")]
        public static extern void HostBandwidthLimit(IntPtr host, uint incomingBandwidth, uint outgoingBandwidth); 

        [DllImport(MODULE_NAME, CallingConvention = LibENet.ENetCallingConvention, EntryPoint = "enet_host_broadcast")]
        public static extern void HostBroadcast(IntPtr host, byte channel, NativeENetPacket* packet);

        [DllImport(MODULE_NAME, CallingConvention = LibENet.ENetCallingConvention, EntryPoint = "enet_host_channel_limit")]
        public static extern void HostChannelLimit(IntPtr host, UIntPtr channelLimit);

        [DllImport(MODULE_NAME, CallingConvention = LibENet.ENetCallingConvention, EntryPoint = "enet_host_check_events")]
        public unsafe static extern int HostCheckEvents(IntPtr host, NativeENetEvent* e);

        [DllImport(MODULE_NAME, CallingConvention = LibENet.ENetCallingConvention, EntryPoint = "enet_host_compress")]
        public static extern void HostCompress(IntPtr host, NativeENetCompressor* compressor);

        [DllImport(MODULE_NAME, CallingConvention = LibENet.ENetCallingConvention, EntryPoint = "enet_host_compress_with_range_coder")]
        public static extern int HostCompressWithRangeCoder(IntPtr host);

        [DllImport(MODULE_NAME, CallingConvention = LibENet.ENetCallingConvention, EntryPoint = "enet_host_connect")]
        public static extern NativeENetPeer* HostConnect(IntPtr host, NativeENetAddress* address, UIntPtr channelCount, uint data);

        [DllImport(MODULE_NAME, CallingConvention = LibENet.ENetCallingConvention, EntryPoint = "enet_host_create")]
        public static extern IntPtr HostCreate(ENetAddressType addressType, NativeENetAddress* address, UIntPtr peerCount, UIntPtr channelLimit, uint incomingBandwidth, uint outgoingBandwidth);

        [DllImport(MODULE_NAME, CallingConvention = LibENet.ENetCallingConvention, EntryPoint = "enet_host_destroy")]
        public static extern void HostDestroy(IntPtr host);

        [DllImport(MODULE_NAME, CallingConvention = LibENet.ENetCallingConvention, EntryPoint = "enet_host_flush")]
        public static extern void HostFlush(IntPtr host);

        [DllImport(MODULE_NAME, CallingConvention = LibENet.ENetCallingConvention, EntryPoint = "enet_host_service")]
        public static extern int HostService(IntPtr host, NativeENetEvent* e, uint timeout);

        [DllImport(MODULE_NAME, CallingConvention = LibENet.ENetCallingConvention, EntryPoint = "enet_crc32")]
        public static extern uint Crc32(NativeENetBuffer* buffers, UIntPtr buffersCount);

        [DllImport(MODULE_NAME, CallingConvention = LibENet.ENetCallingConvention, EntryPoint = "enet_packet_create")]
        public static extern NativeENetPacket* PacketCreate(IntPtr data, UIntPtr dataLength, ENetPacketFlags flags);

        [DllImport(MODULE_NAME, CallingConvention = LibENet.ENetCallingConvention, EntryPoint = "enet_packet_destroy")]
        public static extern void PacketDestroy(NativeENetPacket* packet);

        [DllImport(MODULE_NAME, CallingConvention = LibENet.ENetCallingConvention, EntryPoint = "enet_packet_resize")]
        public static extern int PacketResize(NativeENetPacket* packet, UIntPtr dataLength);

        [DllImport(MODULE_NAME, CallingConvention = LibENet.ENetCallingConvention, EntryPoint = "enet_peer_disconnect")]
        public static extern void PeerDisconnect(NativeENetPeer* peer, uint data);

        [DllImport(MODULE_NAME, CallingConvention = LibENet.ENetCallingConvention, EntryPoint = "enet_peer_disconnect_now")]
        public static extern void PeerDisconnectNow(NativeENetPeer* peer, uint data);

        [DllImport(MODULE_NAME, CallingConvention = LibENet.ENetCallingConvention, EntryPoint = "enet_peer_disconnect_later")]
        public static extern void PeerDisconnectLater(NativeENetPeer* peer, uint data);

        [DllImport(MODULE_NAME, CallingConvention = LibENet.ENetCallingConvention, EntryPoint = "enet_peer_ping")]
        public static extern void PeerPing(NativeENetPeer* peer);

        [DllImport(MODULE_NAME, CallingConvention = LibENet.ENetCallingConvention, EntryPoint = "enet_peer_ping_interval")]
        public static extern void PeerPingInterval(NativeENetPeer* peer, uint pingInterval);

        [DllImport(MODULE_NAME, CallingConvention = LibENet.ENetCallingConvention, EntryPoint = "enet_peer_receive")]
        public static extern NativeENetPacket* PeerReceive(NativeENetPeer* peer, byte* channelID);

        [DllImport(MODULE_NAME, CallingConvention = LibENet.ENetCallingConvention, EntryPoint = "enet_peer_reset")]
        public static extern void PeerReset(NativeENetPeer* peer);

        [DllImport(MODULE_NAME, CallingConvention = LibENet.ENetCallingConvention, EntryPoint = "enet_peer_send")]
        public static extern int PeerSend(NativeENetPeer* peer, byte channelID, NativeENetPacket* packet);

        [DllImport(MODULE_NAME, CallingConvention = LibENet.ENetCallingConvention, EntryPoint = "enet_peer_throttle_configure")]
        public static extern void PeerThrottleConfigure(NativeENetPeer* peer, uint interval, uint acceleration, uint deceleration);

        [DllImport(MODULE_NAME, CallingConvention = LibENet.ENetCallingConvention, EntryPoint = "enet_peer_timeout")]
        public static extern void PeerTimeout(NativeENetPeer* peer, uint timeoutLimit, uint timeoutMinimum, uint timeoutMaximum);

        [DllImport(MODULE_NAME, CallingConvention = LibENet.ENetCallingConvention, EntryPoint = "enet_interophelper_sizeoroffset")]
        public static extern IntPtr InteropHelperSizeOrOffset(uint id);
    }
}
