using System;
using System.Runtime.InteropServices;
using System.IO;
using Native = ENet.Managed.Structures;

namespace ENet.Managed
{
    public static unsafe class LibENet
    {
        internal const CallingConvention ENetCallingConvention = CallingConvention.Cdecl;

        public const int ENET_PROTOCOL_MAXIMUM_PACKET_COMMANDS = 32;
        public const int ENET_PEER_UNSEQUENCED_WINDOW_SIZE = 1024;
        public const int ENET_PROTOCOL_MAXIMUM_PEER_ID = 0xFFF;

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate int ENetInitializeDelegate();
        public static ENetInitializeDelegate Initialize { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate int ENetInitializeWithCallbacksDelegate(uint version, Native.ENetCallbacks* callbacks);
        public static ENetInitializeWithCallbacksDelegate InitializeWithCallbacks { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate void ENetDeinitializeDelegate();
        public static ENetDeinitializeDelegate Deinitialize { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate uint ENetLinkedVersionDelegate();
        public static ENetLinkedVersionDelegate LinkedVersion { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate int ENetAddressGetHost(Native.ENetAddress* address, IntPtr hostName, UIntPtr nameLength);
        public static ENetAddressGetHost AddressGetHost { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate int ENetAddressGetHostIP(Native.ENetAddress* address, IntPtr hostName, UIntPtr nameLength);
        public static ENetAddressGetHostIP AddressGetHostIP { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate int ENetAddressSetHost(Native.ENetAddress* address, [MarshalAs(UnmanagedType.LPStr)] string hostName);
        public static ENetAddressSetHost AddressSetHost { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate void ENetHostBandwidthLimitDelegate(IntPtr host, uint incomingBandwidth, uint outgoingBandwidth);
        public static ENetHostBandwidthLimitDelegate HostBandwidthLimit { get; private set; }

        //[UnmanagedFunctionPointer(ENetCallingConvention)]
        //public delegate void ENetHostBandwidthThrottleDelegate(IntPtr host);
        //public static ENetHostBandwidthThrottleDelegate HostBandwidthThrottle { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate void ENetHostBroadcastDelegate(IntPtr host, byte channel, Native.ENetPacket* packet);
        public static ENetHostBroadcastDelegate HostBroadcast { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate void ENetHostChannelLimitDelegate(IntPtr host, UIntPtr channelLimit);
        public static ENetHostChannelLimitDelegate HostChannelLimit { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public unsafe delegate void ENetHostCheckEventsDelegate(IntPtr host, Native.ENetEvent* e);
        public static ENetHostCheckEventsDelegate HostCheckEvents { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate void ENetHostCompressDelegate(IntPtr host, Native.ENetCompressor* compressor);
        public static ENetHostCompressDelegate HostCompress { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate int ENetHostCompressWithRangeCoderDelegate(IntPtr host);
        public static ENetHostCompressWithRangeCoderDelegate HostCompressWithRangeCoder { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate Native.ENetPeer* ENetHostConnectDelegate(IntPtr host, Native.ENetAddress* address, UIntPtr channelCount, uint data);
        public static ENetHostConnectDelegate HostConnect { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate IntPtr ENetHostCreateDelegate(Native.ENetAddress* address, UIntPtr peerCount, UIntPtr channelLimit, uint incomingBandwidth, uint outgoingBandwidth);
        public static ENetHostCreateDelegate HostCreate { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate void ENetHostDestroyDelegate(IntPtr host);
        public static ENetHostDestroyDelegate HostDestroy { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate void ENetHostFlushDelegate(IntPtr host);
        public static ENetHostFlushDelegate HostFlush { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate int ENetHostServiceDelegate(IntPtr host, Native.ENetEvent* e, uint timeout);
        public static ENetHostServiceDelegate HostService { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate void ENetCrc32Delegate(Native.ENetBuffer* buffers, UIntPtr buffersCount);
        public static ENetCrc32Delegate Crc32 { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate Native.ENetPacket* ENetPacketCreateDelegate(IntPtr data, UIntPtr dataLength, ENetPacketFlags flags);
        public static ENetPacketCreateDelegate PacketCreate { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate void ENetPacketDestroyDelegate(Native.ENetPacket* packet);
        public static ENetPacketDestroyDelegate PacketDestroy;

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate int ENetPacketResizeDelegate(Native.ENetPacket* packet, UIntPtr dataLength);
        public static ENetPacketResizeDelegate PacketResize { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate void ENetPeerDisconnectDelegate(Native.ENetPeer* peer, uint data);
        public static ENetPeerDisconnectDelegate PeerDisconnect { get; private set; }
        public static ENetPeerDisconnectDelegate PeerDisconnectNow { get; private set; }
        public static ENetPeerDisconnectDelegate PeerDisconnectLater { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate void ENetPeerPingDelegate(Native.ENetPeer* peer);
        public static ENetPeerPingDelegate PeerPing { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate void ENetPeerPingIntervalDelegate(Native.ENetPeer* peer, uint pingInterval);
        public static ENetPeerPingIntervalDelegate PeerPingInterval { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate Native.ENetPacket* ENetPeerReceiveDelegate(Native.ENetPeer* peer, byte* channelID);
        public static ENetPeerReceiveDelegate PeerReceive { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate void ENetPeerResetDelegate(Native.ENetPeer* peer);
        public static ENetPeerResetDelegate PeerReset { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate int ENetPeerSendDelegate(Native.ENetPeer* peer, byte channelID, Native.ENetPacket* packet);
        public static ENetPeerSendDelegate PeerSend { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate void ENetPeerThrottleConfigureDelegate(Native.ENetPeer* peer, uint interval, uint acceleration, uint deceleration);
        public static ENetPeerThrottleConfigureDelegate PeerThrottleConfigure { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate void ENetPeerTimeoutDelegate(Native.ENetPeer* peer, uint timeoutLimit, uint timeoutMinimum, uint timeoutMaximum);
        public static ENetPeerTimeoutDelegate PeerTimeout { get; private set; }

        public static string DllPath { get; set; }
        public static IntPtr DllHandle { get; private set; } = IntPtr.Zero;
        public static bool IsLoaded { get; private set; } = false;

        static LibENet()
        {
            var dllName = Environment.Is64BitProcess ? "libenet_X64.dll" : "libenet_X86.dll";
            DllPath = Path.Combine(Path.GetTempPath(), "enet_managed_resource", dllName);
        }

        public static bool TryDelete()
        {
            if (IsLoaded) return false;
            try
            {
                File.Delete(DllPath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void Unload()
        {
            if (!IsLoaded) return;
            Deinitialize();
            Win32.FreeLibrary(DllHandle);
            DllHandle = IntPtr.Zero;
            IsLoaded = false;
        }

        public static void Load()
        {
            if (IsLoaded) return;
            try
            {
                Load(false);
            }
            catch
            {
                Load(true);
            }
            IsLoaded = true;
        }

        static void Load(bool overwrite)
        {
            LoadDll(overwrite);

            Initialize = GetProc<ENetInitializeDelegate>("enet_initialize");
            InitializeWithCallbacks = GetProc<ENetInitializeWithCallbacksDelegate>("enet_initialize_with_callbacks");
            Deinitialize = GetProc<ENetDeinitializeDelegate>("enet_deinitialize");
            LinkedVersion = GetProc<ENetLinkedVersionDelegate>("enet_linked_version");

            AddressGetHost = GetProc<ENetAddressGetHost>("enet_address_get_host");
            AddressGetHostIP = GetProc<ENetAddressGetHostIP>("enet_address_get_host_ip");
            AddressSetHost = GetProc<ENetAddressSetHost>("enet_address_set_host");

            HostBandwidthLimit = GetProc<ENetHostBandwidthLimitDelegate>("enet_host_bandwidth_limit");
            //HostBandwidthThrottle = GetProc<ENetHostBandwidthThrottleDelegate>("enet_host_bandwidth_throttle");
            HostBroadcast = GetProc<ENetHostBroadcastDelegate>("enet_host_broadcast");
            HostChannelLimit = GetProc<ENetHostChannelLimitDelegate>("enet_host_channel_limit");
            HostCheckEvents = GetProc<ENetHostCheckEventsDelegate>("enet_host_check_events");
            HostCompress = GetProc<ENetHostCompressDelegate>("enet_host_compress");
            HostCompressWithRangeCoder = GetProc<ENetHostCompressWithRangeCoderDelegate>("enet_host_compress_with_range_coder");
            HostConnect = GetProc<ENetHostConnectDelegate>("enet_host_connect");
            HostCreate = GetProc<ENetHostCreateDelegate>("enet_host_create");
            HostDestroy = GetProc<ENetHostDestroyDelegate>("enet_host_destroy");
            HostFlush = GetProc<ENetHostFlushDelegate>("enet_host_flush");
            HostService = GetProc<ENetHostServiceDelegate>("enet_host_service");

            Crc32 = GetProc<ENetCrc32Delegate>("enet_crc32");
            PacketCreate = GetProc<ENetPacketCreateDelegate>("enet_packet_create");
            PacketDestroy = GetProc<ENetPacketDestroyDelegate>("enet_packet_destroy");
            PacketResize = GetProc<ENetPacketResizeDelegate>("enet_packet_resize");

            PeerDisconnect = GetProc<ENetPeerDisconnectDelegate>("enet_peer_disconnect");
            PeerDisconnectLater = GetProc<ENetPeerDisconnectDelegate>("enet_peer_disconnect_later");
            PeerDisconnectNow = GetProc<ENetPeerDisconnectDelegate>("enet_peer_disconnect_now");
            PeerPing = GetProc<ENetPeerPingDelegate>("enet_peer_ping");
            PeerPingInterval = GetProc<ENetPeerPingIntervalDelegate>("enet_peer_ping_interval");
            PeerReceive = GetProc<ENetPeerReceiveDelegate>("enet_peer_receive");
            PeerReset = GetProc<ENetPeerResetDelegate>("enet_peer_reset");
            PeerSend = GetProc<ENetPeerSendDelegate>("enet_peer_send");
            PeerThrottleConfigure = GetProc<ENetPeerThrottleConfigureDelegate>("enet_peer_throttle_configure");
            PeerTimeout = GetProc<ENetPeerTimeoutDelegate>("enet_peer_timeout");
        }

        static void LoadDll(bool overwrite)
        {
            if (DllHandle != IntPtr.Zero)
                Win32.FreeLibrary(DllHandle);

            var dllBytes = Environment.Is64BitProcess ?
                           ENetBinariesResource.libenet_64 : 
                           ENetBinariesResource.libenet_32;

            if (!File.Exists(DllPath) || overwrite)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(DllPath));
                File.WriteAllBytes(DllPath, dllBytes);
            }

            DllHandle = Win32.LoadLibrary(DllPath);
            if (DllHandle == IntPtr.Zero) throw new Exception("Failed to load ENet library.");
        }

        public static T GetProc<T>(string procName)
        {
            if (DllHandle == IntPtr.Zero)
                throw new Exception("ENet library wasn't loaded.");
        
            IntPtr address = Win32.GetProcAddress(DllHandle, procName);

            if (address == IntPtr.Zero)
            {
                throw new DllNotFoundException(string.Format("Function {0} didn't found within ENet library.", procName));
            }

            return (T)(object)Marshal.GetDelegateForFunctionPointer(address, typeof(T));
        }
    }
}
