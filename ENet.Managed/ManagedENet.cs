using System;
using System.Runtime.InteropServices;
using Native = ENet.Managed.Structures;

namespace ENet.Managed
{
    public unsafe static class ManagedENet
    {
        static bool _Started;
        static ENetAllocator _Allocator;
        static Native.ENetMemoryAllocCallback MemAllocDelegate;
        static Native.ENetMemoryFreeCallback MemFreeDelegate;
        static Native.ENetNoMemoryCallback NoMemoryDelegate;

        public static bool Started => _Started;
        public static ENetAllocator Allocator => _Allocator;
        public static Version LinkedVersion { get; private set; }

        static ManagedENet()
        {
            _Started = false;
            MemAllocDelegate = MemAllocCallback;
            MemFreeDelegate = MemFreeCallback;
            NoMemoryDelegate = NoMemoryCallback;
        }

        public static void Startup(ENetAllocator allocator = null)
        {
            if (_Started) return;
            _Started = true;

            _Allocator = (allocator == null) ? new ENetManagedAllocator() : allocator;

            LibENet.Load();

            Native.ENetCallbacks callbacks = new Native.ENetCallbacks();
            callbacks.Malloc = Marshal.GetFunctionPointerForDelegate(MemAllocDelegate);
            callbacks.Free = Marshal.GetFunctionPointerForDelegate(MemFreeDelegate);
            callbacks.NoMemory = Marshal.GetFunctionPointerForDelegate(NoMemoryDelegate);

            var linkedVer = LibENet.LinkedVersion();
            if (LibENet.InitializeWithCallbacks(linkedVer, &callbacks) != 0)
                throw new Exception("ENet library initialization failed.");

            LinkedVersion = new Version((int)(((linkedVer) >> 16) & 0xFF),
                                        (int)(((linkedVer) >> 8) & 0xFF), 
                                        (int)((linkedVer) & 0xFF));
        }

        public static void Shutdown(bool delete)
        {
            if (!_Started) return;
            _Started = false;

            LibENet.Unload();
            if (delete) LibENet.TryDelete();

            _Allocator.Dispose();
            _Allocator = null;
        }

        private static void NoMemoryCallback() => throw new OutOfMemoryException("ENet out of memory");
        private static IntPtr MemAllocCallback(UIntPtr size) => _Allocator.Alloc((int)size.ToUInt32());
        private static void MemFreeCallback(IntPtr memory) => _Allocator.Free(memory);
    }
}
