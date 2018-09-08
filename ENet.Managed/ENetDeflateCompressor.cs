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
        private DeflateStream m_Deflate = null;

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
            if (m_Deflate != null)
                m_Deflate.Dispose();

            m_Deflate = new DeflateStream(new MemoryStream(), Level, true);
        }

        public override void Compress(byte[] buffer)
        {
            m_Deflate.Write(buffer, 0, buffer.Length);
        }

        public override byte[] EndCompress()
        {
            var memory = m_Deflate.BaseStream as MemoryStream;
            m_Deflate.Dispose();
            m_Deflate = null;
            return memory.ToArray();
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

        protected override void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            if (m_Deflate != null)
            {
                m_Deflate.Dispose();
                m_Deflate = null;
            }
        }
    }
}
