using System;
using System.Runtime.InteropServices;

namespace ENet.Managed.Native
{
    // Defining ENetHost structure is very complex due its platform and architecture specific fields
    // especially socket fd which it's size differs from linux to win32
    public static class ENetHostOffset
    {
        public static readonly int
            ChecksumOffset,
            InterceptOffset,
            TotalSentDataOffset,
            TotalSentPacketsOffset,
            TotalReceivedDataOffset,
            TotalReceivedPacketsOffset,
            ReceivedAddressOffset,
            ReceivedDataOffset,
            ReceivedDataLengthOffset,
            ConnectedPeersOffset,
            PeersOffset,
            PeerCountOffset,
            DuplicatePeers;

        static ENetHostOffset()
        {
            // These offsets should be in sync with native implementation
            // Also these are Win32 offsets which we will convert to Posix if required.
            ChecksumOffset = Environment.Is64BitProcess ? 2704 : 2136;
            InterceptOffset = Environment.Is64BitProcess ? 10976 : 10380;
            TotalSentDataOffset = Environment.Is64BitProcess ? 10960 : 10364;
            TotalSentPacketsOffset = Environment.Is64BitProcess ? 10964 : 10368;
            TotalReceivedDataOffset = Environment.Is64BitProcess ? 10968 : 10372;
            TotalReceivedPacketsOffset = Environment.Is64BitProcess ? 10972 : 10376;
            ReceivedAddressOffset = Environment.Is64BitProcess ? 10936 : 10348;
            ReceivedDataOffset = Environment.Is64BitProcess ? 10944 : 10356;
            ReceivedDataLengthOffset = Environment.Is64BitProcess ? 10952 : 10360;
            ConnectedPeersOffset = Environment.Is64BitProcess ? 10984 : 10384;
            PeersOffset = Environment.Is64BitProcess ? 40 : 36;
            PeerCountOffset = Environment.Is64BitProcess ? 48 : 40;

            // If we are running on a 64-bit Posix system, subtract 4 from offsets.
            // Explained: 
            // ENetHost contains a socket file descriptor field which
            // on Windows is sized as IntPtr and on Posix is sized as int.
            // In 32-bit process there is no difference since both are sized as Int32,
            // but when it comes to 64-bit process on Win32 its Int64 and on Posix still Int32.
            if (Environment.Is64BitProcess && !RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                ChecksumOffset -= 4;
                InterceptOffset -= 4;
                TotalSentDataOffset -= 4;
                TotalSentPacketsOffset -= 4;
                TotalReceivedDataOffset -= 4;
                TotalReceivedPacketsOffset -= 4;
                ReceivedAddressOffset -= 4;
                ReceivedDataOffset -= 4;
                ReceivedDataLengthOffset -= 4;
                ConnectedPeersOffset -= 4;
                PeersOffset -= 4;
                PeerCountOffset -= 4;
            }
        }
    }
}