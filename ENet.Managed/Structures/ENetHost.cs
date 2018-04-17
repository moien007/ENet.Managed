using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENet.Managed.Structures
{
    public static class ENetHost
    {
        public static int ChecksumOffset = Environment.Is64BitProcess ? 2704 : 2136;
        public static int InterceptOffset = Environment.Is64BitProcess ? 10976 : 10380;
        public static int TotalSentDataOffset = Environment.Is64BitProcess ? 10960 : 10364;
        public static int TotalSentPacketsOffset = Environment.Is64BitProcess ? 10964 : 10368;
        public static int TotalReceivedDataOffset = Environment.Is64BitProcess ? 10968 : 10372;
        public static int TotalReceivedPacketsOffset = Environment.Is64BitProcess ? 10972 : 10376;
        public static int ReceivedAddressOffset = Environment.Is64BitProcess ? 10936 : 10348;
        public static int ReceivedDataOffset = Environment.Is64BitProcess ? 10944 : 10356;
        public static int ReceivedDataLengthOffset = Environment.Is64BitProcess ? 10952 : 10360;
        public static int ConnectedPeersOffset = Environment.Is64BitProcess ? 10984 : 10384;
    }
}