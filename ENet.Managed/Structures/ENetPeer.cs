using System;
using System.Runtime.InteropServices;

namespace ENet.Managed.Structures
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ENetPeer
    {
        public ENetListNode DispatchList;
        public IntPtr Host;
        public ushort OutgoingPeerID;
        public ushort IncomingPeerID;
        public uint ConnectID;
        public byte OutgoingSessionID;
        public byte IncomingSessionID;
        public ENetAddress Address;
        public IntPtr Data;
        public ENetPeerState State;
        public IntPtr Channels;
        public UIntPtr ChannelCount;
        public uint IncomingBandwidth;
        public uint OutgoingBandwidth;
        public uint IncomingBandwidthThrottleEpoch;
        public uint OutgoingBandwidthThrottleEpoch;
        public uint IncomingDataTotal;
        public uint OutgoingDataTotal;
        public uint LastSendTime;
        public uint LastReceiveTime;
        public uint NextTimeout;
        public uint EarliestTimeout;
        public uint PacketLossEpoch;
        public uint PacketsSent;
        public uint PacketsLost;
        public uint PacketLoss;
        public uint PacketLossVariance;
        public uint PacketThrottle;
        public uint PacketThrottleLimit;
        public uint PacketThrottleCounter;
        public uint PacketThrottleEpoch;
        public uint PacketThrottleAcceleration;
        public uint PacketThrottleDeceleration;
        public uint PacketThrottleInterval;
        public uint PingInterval;
        public uint TimeoutLimit;
        public uint TimeoutMinimum;
        public uint TimeoutMaximum;
        public uint LastRoundTripTime;
        public uint LowestRoundTripTime;
        public uint LastRoundTripTimeVariance;
        public uint HighestRoundTripTimeVariance;
        public uint RoundTripTime;
        public uint RoundTripTimeVariance;
        public uint MTU;
        public uint WindowSize;
        public uint ReliableDataInTransit;
        public ushort OutgoingReliableSequenceNumber;
        public ENetList Acknowledgements;
        public ENetList SentReliableCommands;
        public ENetList SentUnreliableCommands;
        public ENetList OutgoingReliableCommands;
        public ENetList OutgoingUnreliableCommands;
        public ENetList DispatchedCommands;
        public int NeedsDispatch;
        public ushort IncomingUnsequencedGroup;
        public ushort OutgoingUnsequencedGroup;
        public fixed uint UnsequencedWindow[LibENet.ENET_PEER_UNSEQUENCED_WINDOW_SIZE / 32];
        public uint EventData;
        public UIntPtr TotalWaitingData;
    }
}