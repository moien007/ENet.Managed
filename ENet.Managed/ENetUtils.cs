using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace ENet.Managed
{
    public unsafe static class ENetUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MemoryCopy(IntPtr dest, IntPtr src, UIntPtr count)
        {
            Win32.MemoryCopy(dest, src, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MemoryCopy(byte[] dest, int destOffset, byte[] src, int srcOffset, int count)
        {
            fixed (byte* pDest = dest)
            fixed (byte* pSrc = src)
            {
                MemoryCopy((IntPtr)(pDest + destOffset), (IntPtr)(pSrc + srcOffset), (UIntPtr)count);
            }
        }

        public static void MemoryCopy(byte[] dest, byte[] src, int count)
        {
            MemoryCopy(dest, 0, src, 0, count);
        }

        public static string FormatBytes(long count)
        {
            var sb = new StringBuilder(40);
            if (Win32.StrFormatKBSize(count, sb, (uint)sb.Capacity) == IntPtr.Zero)
                throw new Exception("Failed to format bytes to string.");
            return sb.ToString();
        }
    }
}
