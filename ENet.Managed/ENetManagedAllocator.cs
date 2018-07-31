using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace ENet.Managed
{
    internal sealed class ENetManagedAllocator : ENetAllocator
    {
        private class Segment
        {
            public int Offset;
            public int Count;

            public Segment(int offset, int count)
            {
                Offset = offset;
                Count = count;
            }
        }

        private class Chunk
        {
            private IntPtr m_Ptr;
            private List<Segment> m_Segments;
            private List<Segment> m_TakenSegments;


            public Chunk(int size)
            {
                m_Ptr = Marshal.AllocHGlobal(size);
                m_Segments = new List<Segment>();
                m_TakenSegments = new List<Segment>();

                m_Segments.Add(new Segment(0, size));
            }

            public IntPtr Take(int size)
            {
                var index = m_Segments.FindIndex(p => p.Count >= size);
                if (index < 0) return IntPtr.Zero;

                var segment = m_Segments[index];
                if (segment.Count == size)
                {
                    m_Segments.FastRemoveAt(index);
                    m_TakenSegments.Add(segment);
                    return IntPtr.Add(m_Ptr, segment.Offset);
                }

                m_Segments[index] = new Segment(segment.Offset + size, segment.Count - size);
                segment.Count = size;
                m_TakenSegments.Add(segment);

                return IntPtr.Add(m_Ptr, segment.Offset);
            }

            public bool Return(IntPtr ptr)
            {
                var offset = (IntPtr.Size == 4) ? 
                             ptr.ToInt32() - m_Ptr.ToInt32() :
                             ptr.ToInt64() - m_Ptr.ToInt64();

                var index = m_TakenSegments.FindIndex(p => p.Offset == offset);
                if (index < 0) return false;

                var segment = m_TakenSegments[index];
                m_TakenSegments.FastRemoveAt(index);

                index = m_Segments.FindIndex(p => segment.Offset + segment.Count == p.Offset);
                if (0 <= index)
                {
                    segment.Count += m_Segments[index].Count;
                    m_Segments.FastRemoveAt(index);
                }

                index = m_Segments.FindIndex(p => p.Offset + p.Count == segment.Offset);
                if (0 <= index)
                {
                    segment.Count += m_Segments[index].Count;
                    segment.Offset = m_Segments[index].Offset;
                    m_Segments.FastRemoveAt(index);
                }

                m_Segments.Add(segment);
                return true;
            }

            public void Free() => Marshal.FreeHGlobal(m_Ptr);
        }

        private const int ChunksSize = 1000000;

        private List<Chunk> m_Chunks = new List<Chunk>();
        private List<IntPtr> m_LargeAllocations = new List<IntPtr>();

        public bool Disposed { get; private set; } = false;

        public override IntPtr Alloc(int size)
        {
            if (Disposed) return IntPtr.Zero;
            if (size <= 0) return IntPtr.Zero; 

            if (size >= ChunksSize)
            {
                var ptr = Marshal.AllocHGlobal(size);
                lock (m_LargeAllocations) m_LargeAllocations.Add(ptr);
                return ptr;
            }

            lock (m_Chunks)
            {
                foreach (var chunk in m_Chunks)
                {
                    var ptr = chunk.Take(size);
                    if (ptr == IntPtr.Zero) continue;
                    return ptr;
                }

                var newchunk = new Chunk(ChunksSize);
                m_Chunks.Add(newchunk);
                return newchunk.Take(size);
            }
        }

        public override void Free(IntPtr ptr)
        {
            if (Disposed) return;
            if (ptr == IntPtr.Zero) return;

            lock (m_LargeAllocations)
            {
                if (m_LargeAllocations.FastRemove(ptr)) goto free;
            }

            lock (m_Chunks)
            {
                foreach (var chunk in m_Chunks)
                {
                    if (chunk.Return(ptr)) return;
                }
            }

            free:
            Marshal.FreeHGlobal(ptr);
        }

        public override void Dispose()
        {
            if (Disposed) return;
            Disposed = true;

            m_LargeAllocations.ForEach(ptr => Marshal.FreeHGlobal(ptr));
            m_LargeAllocations = null;

            m_Chunks.ForEach(chunk => chunk.Free());
            m_Chunks = null;
        }
    }
}
