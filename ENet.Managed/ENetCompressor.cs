using System;
using System.Runtime.InteropServices;
using ENet.Managed.Platforms;
using Native = ENet.Managed.Structures;
using ByteArrayPool = System.Buffers.ArrayPool<byte>; 

namespace ENet.Managed
{
    public unsafe abstract class ENetCompressor : IDisposable
    {
        internal static readonly Native.ENetCompressCallback CompressDelegate;
        internal static readonly Native.ENetDecompressCallback DecompressDelegate;
        internal static readonly Native.ENetDestoryCompressorCallback DestroyDelegate;

        public ENetHost Host { get; internal set; } = null;

        static ENetCompressor()
        {
            CompressDelegate = CompressCallback;
            DecompressDelegate = DecompressCallback;
            DestroyDelegate = DestroyCallback;
        }

        ~ENetCompressor() => Dispose(false);

        public abstract void BeginCompress();
        public abstract void Compress(byte[] buffer, int count);
        public abstract byte[] EndCompress();
        public abstract byte[] Decompress(byte[] input, int count, int outputLimit);
        protected virtual void Dispose(bool disposing) { }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

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

            byte[] input = null;

            compressor.BeginCompress();

            for (int i = 0; i < inBufferCount.ToUInt32(); i++)
            {
                var inBuffer = inBuffers[i];
                var count = (int)inBuffer.DataLength.ToUInt32();

                if (input == null)
                {
                    input = ByteArrayPool.Shared.Rent(count);
                }
                else if (input.Length < count)
                {
                    ByteArrayPool.Shared.Return(input);
                    input = ByteArrayPool.Shared.Rent(count);
                }

                fixed (byte* dest = input)
                {
                    Platform.Current.MemoryCopy((IntPtr)dest, inBuffer.Data, inBuffer.DataLength);
                }

                compressor.Compress(input, count);
            }

            if (input != null)
            {
                ByteArrayPool.Shared.Return(input);
            }

            var result = compressor.EndCompress();
            if (result.Length > outLimit.ToUInt32())
                return UIntPtr.Zero;

            var resultLen = (UIntPtr)result.Length;

            fixed (byte* src = result)
            {
                Platform.Current.MemoryCopy(outData, (IntPtr)src, resultLen);
            }

            return resultLen;
        }

        static UIntPtr DecompressCallback(IntPtr context, IntPtr inData, UIntPtr inLimit, IntPtr outData, UIntPtr outLimit)
        {
            if (context == IntPtr.Zero || inLimit == UIntPtr.Zero)
                return UIntPtr.Zero;

            var compressor = GCHandle.FromIntPtr(context).Target as ENetCompressor;
            var count = (int)inLimit.ToUInt32();
            var input = ByteArrayPool.Shared.Rent(count);

            fixed (byte* dest = input)
            {
                Platform.Current.MemoryCopy((IntPtr)dest, inData, inLimit);
            }

            var output = compressor.Decompress(input, count, (int)outLimit);
            if (output.Length > outLimit.ToUInt32())
                return UIntPtr.Zero;

            ByteArrayPool.Shared.Return(input);

            var outputLen = (UIntPtr)output.Length;
            fixed (byte* src = output)
            {
                Platform.Current.MemoryCopy(outData, (IntPtr)src, outputLen);
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
