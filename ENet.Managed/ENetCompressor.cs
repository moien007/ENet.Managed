using System;
using System.Runtime.InteropServices;
using Native = ENet.Managed.Structures;

namespace ENet.Managed
{
    public unsafe abstract class ENetCompressor : IDisposable
    {
        internal static Native.ENetCompressCallback CompressDelegate;
        internal static Native.ENetDecompressCallback DecompressDelegate;
        internal static Native.ENetDestoryCompressorCallback DestroyDelegate;

        public ENetHost Host { get; internal set; } = null;

        static ENetCompressor()
        {
            CompressDelegate = CompressCallback;
            DecompressDelegate = DecompressCallback;
            DestroyDelegate = DestroyCallback;
        }

        public abstract void BeginCompress();
        public abstract void Compress(byte[] buffer);
        public abstract byte[] EndCompress();
        public abstract byte[] Decompress(byte[] input, int outputLimit);
        public abstract void Dispose();

        internal IntPtr AllocHandle()
        {
            return GCHandle.ToIntPtr(GCHandle.Alloc(this, GCHandleType.Normal));
        }

        static unsafe UIntPtr CompressCallback(IntPtr context, Native.ENetBuffer* inBuffers, UIntPtr inBufferCount, UIntPtr inLimit, IntPtr outData, UIntPtr outLimit)
        {
            if (context == IntPtr.Zero || inBufferCount == UIntPtr.Zero || inLimit == UIntPtr.Zero)
                return UIntPtr.Zero;

            var handle = GCHandle.FromIntPtr(context);
            var compressor = handle.Target as ENetCompressor;

            byte[] input;

            compressor.BeginCompress();

            for (int i = 0; i < inBufferCount.ToUInt32(); i++)
            {
                var inBuffer = inBuffers[i];
                input = new byte[inBuffer.DataLength.ToUInt32()];
                
                fixed (byte* dest = input)
                {
                    ENetUtils.MemoryCopy((IntPtr)dest, inBuffer.Data, inBuffer.DataLength);
                }

                compressor.Compress(input);
            }

            var result = compressor.EndCompress();

            if (result.Length > outLimit.ToUInt32())
                return UIntPtr.Zero;

            var resultLen = (UIntPtr)result.Length;

            fixed (byte* src = result)
            {
                ENetUtils.MemoryCopy(outData, (IntPtr)src, resultLen);
            }

            return resultLen;
        }

        static UIntPtr DecompressCallback(IntPtr context, IntPtr inData, UIntPtr inLimit, IntPtr outData, UIntPtr outLimit)
        {
            if (context == IntPtr.Zero || inLimit == UIntPtr.Zero)
                return UIntPtr.Zero;

            var handle = GCHandle.FromIntPtr(context);
            var compressor = handle.Target as ENetCompressor;
            var input = new byte[inLimit.ToUInt32()];

            fixed (byte* dest = input)
            {
                ENetUtils.MemoryCopy((IntPtr)dest, inData, inLimit);
            }

            var output = compressor.Decompress(input, (int)outLimit);
            if (output.Length > outLimit.ToUInt32())
                return UIntPtr.Zero;

            var outputLen = (UIntPtr)output.Length;

            fixed (byte* src = output)
            {
                ENetUtils.MemoryCopy(outData, (IntPtr)src, outputLen);
            }

            return outputLen;
        }

        static void DestroyCallback(IntPtr context)
        {
            var handle = GCHandle.FromIntPtr(context);
            if (handle == null) return;
            (handle.Target as IDisposable).Dispose();
            handle.Free();
        }
    }
}
