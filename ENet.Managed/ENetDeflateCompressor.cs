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
        private MemoryStream m_Memory;
        private DeflateStream m_Deflate = null;

        public CompressionLevel Level { get; private set; }

        public ENetDeflateCompressor() : this(CompressionLevel.Optimal) { }
        public ENetDeflateCompressor(CompressionLevel level)
        {
            m_Memory = new MemoryStream();
            Level = level;
        }

        public override void BeginCompress()
        {
            m_Deflate = new DeflateStream(m_Memory, Level, true);
        }

        public override void Compress(byte[] buffer, int count)
        {
            m_Deflate.Write(buffer, 0, count);
        }

        public override byte[] EndCompress()
        {
            m_Deflate.Dispose();
            m_Deflate = null;

            try
            {
                return m_Memory.ToArray();
            }
            finally
            {
                m_Memory.Position = 0;
                m_Memory.SetLength(0);
            }
        }

        public override byte[] Decompress(byte[] input, int count, int outputLimit)
        {
            var inputStream = new MemoryStream(input, 0, count);
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
