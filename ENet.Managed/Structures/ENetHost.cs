using System;

namespace ENet.Managed.Structures
{
    public static class ENetHost
    {
        public static readonly int ChecksumOffset = Environment.Is64BitProcess ? 2704 : 2136;
        public static readonly int InterceptOffset = Environment.Is64BitProcess ? 10976 : 10380;
        public static readonly int TotalSentDataOffset = Environment.Is64BitProcess ? 10960 : 10364;
        public static readonly int TotalSentPacketsOffset = Environment.Is64BitProcess ? 10964 : 10368;
        public static readonly int TotalReceivedDataOffset = Environment.Is64BitProcess ? 10968 : 10372;
        public static readonly int TotalReceivedPacketsOffset = Environment.Is64BitProcess ? 10972 : 10376;
        public static readonly int ReceivedAddressOffset = Environment.Is64BitProcess ? 10936 : 10348;
        public static readonly int ReceivedDataOffset = Environment.Is64BitProcess ? 10944 : 10356;
        public static readonly int ReceivedDataLengthOffset = Environment.Is64BitProcess ? 10952 : 10360;
        public static readonly int ConnectedPeersOffset = Environment.Is64BitProcess ? 10984 : 10384;
    }
}