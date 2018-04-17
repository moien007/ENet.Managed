using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Native = ENet.Managed.Structures;
using System.IO;

namespace ENet.Managed
{
    public unsafe static class ManagedENet
    {
        static bool _Started = false;
        static Native.ENetMemoryAllocCallback MemAllocDelegate;
        static Native.ENetMemoryFreeCallback MemFreeDelegate;
        static Native.ENetNoMemoryCallback NoMemoryDelegate;

        public static Version LinkedVersion { get; private set; }

        static ManagedENet()
        {
            MemAllocDelegate = MemAllocCallback;
            MemFreeDelegate = MemFreeCallback;
            NoMemoryDelegate = NoMemoryCallback;
        }

        public static void Startup()
        {
            if (_Started) return;

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
            _Started = true;
        }

        public static void Shutdown(bool delete)
        {
            if (!_Started) return;
            LibENet.Unload();
            if (delete) LibENet.TryDelete();
            _Started = false;
        }

        private static void NoMemoryCallback()
        {
            throw new OutOfMemoryException("ENet is out of memory.");
        }

        private static void MemFreeCallback(IntPtr memory)
        {
            Marshal.FreeHGlobal(memory);
        }

        private static IntPtr MemAllocCallback(UIntPtr size)
        { 
            return Marshal.AllocHGlobal((int)size);
        }
    }
}
