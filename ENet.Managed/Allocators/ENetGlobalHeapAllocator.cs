using System;
using System.Runtime.InteropServices;

namespace ENet.Managed.Allocators
{
    /// <summary>
    /// Implements <see cref="ENetAllocator"/> which uses <see cref="Marshal.AllocHGlobal(int)"/> and <see cref="Marshal.FreeHGlobal(IntPtr)"/> to allocate and deallocate memory.
    /// This class is singleton.
    /// </summary>
    /// <remarks>
    /// It should be noted that this allocater informs GC about de\allocations.
    /// </remarks>
    public sealed class ENetGlobalHeapAllocator : ENetAllocator
    {
        public static readonly ENetGlobalHeapAllocator Instance = new ENetGlobalHeapAllocator();

        private ENetGlobalHeapAllocator()
        {
            // Singleton
        }

        public override IntPtr Allocate(int size) 
        {
            var memory = Marshal.AllocHGlobal(sizeof(int) + size);
            GC.AddMemoryPressure(size);
            Marshal.WriteInt32(memory, size);
            return IntPtr.Add(memory, sizeof(int));
        }

        public override void Free(IntPtr ptr)
        {
            var size = Marshal.ReadInt32(ptr);
            Marshal.FreeHGlobal(ptr);
            GC.RemoveMemoryPressure(size);
        }
    }
}
