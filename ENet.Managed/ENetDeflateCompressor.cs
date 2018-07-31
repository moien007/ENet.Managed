using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;

namespace ENet.Managed
{
    public sealed class ENetDeflateCompressor : ENetCompressor
    {
        private DeflateStream _Deflate = null;

        public CompressionLevel Level { get; private set; }

        public ENetDeflateCompressor() : this(CompressionLevel.Optimal)
        {

        }

        public ENetDeflateCompressor(CompressionLevel level)
        {
            Level = level;
        }

        public override void BeginCompress()
        {
            if (_Deflate != null)
                _Deflate.Dispose();

            _Deflate = new DeflateStream(new MemoryStream(), Level, true);
        }

        public override void Compress(byte[] buffer)
        {
            _Deflate.Write(buffer, 0, buffer.Length);
        }

        public override byte[] EndCompress()
        {
            var memory = _Deflate.BaseStream as MemoryStream;
            _Deflate.Dispose();
            _Deflate = null;
            var result = memory.ToArray();
            return result;
        }

        public override byte[] Decompress(byte[] input, int outputLimit)
        {
            var inputStream = new MemoryStream(input);
            var outputStream = new MemoryStream();
            var deflate = new DeflateStream(inputStream, CompressionMode.Decompress, true);
            deflate.CopyTo(outputStream);
            deflate.Dispose();
            inputStream.Dispose();
            var result = outputStream.ToArray();
            outputStream.Dispose();
            return result;
        }

        public override void Dispose()
        {
            if (_Deflate != null)
            {
                _Deflate.Dispose();
                _Deflate = null;
            }
        }
    }
}
