using System;
using System.Runtime.CompilerServices;
using ENet.Managed.Platforms;

namespace ENet.Managed
{
    public unsafe static class ENetUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MemoryCopy(byte[] dest, byte[] src, int count)
        {
            fixed (byte* pDest = dest)
            fixed (byte* pSrc = src)
            {
                Platform.Current.MemoryCopy((IntPtr)pDest, (IntPtr)pSrc, (UIntPtr)count);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MemoryCopy(byte[] dest, int destOffset, byte[] src, int srcOffset, int count)
        {
            fixed (byte* pDest = dest)
            fixed (byte* pSrc = src)
            {
                Platform.Current.MemoryCopy((IntPtr)(pDest + destOffset), (IntPtr)(pSrc + srcOffset), (UIntPtr)count);
            }
        }

        public static string FormatBytes(long count)
        {
            if (count < 1000)
            {
                return string.Format("{0}b", count);
            }
            else if (count < 1e+9)
            {
                return string.Format("{0}mb", count / 1000000d);
            }

            return string.Format("{0}gb", count / 1e+9);
        }
    }
}
