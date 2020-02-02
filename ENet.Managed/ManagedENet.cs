using System;
using System.Runtime.InteropServices;
using ENet.Managed.Allocators;
using ENet.Managed.Internal;
using ENet.Managed.Native;

namespace ENet.Managed
{
    /// <summary>
    /// Manages ENet's initialization and deinitialization of ENet library
    /// </summary>
    /// <remarks>
    /// The methods of this should be manually called at beginning and end 
    /// of your application. This class is not thread-safe.
    /// </remarks>
    public unsafe static class ManagedENet
    {
        private static ENetAllocator? s_Allocator;
        private static Version? s_LinkedVersion;
        
        // We hold this delegates references in a static variable
        // in order to prevent garbage collector to collect them
        private static readonly ENetMemoryAllocCallback MemAllocDelegate;
        private static readonly ENetMemoryFreeCallback MemFreeDelegate;
        private static readonly ENetNoMemoryCallback NoMemoryDelegate;

        /// <summary>
        /// Indicates whether ENet has initialized or not
        /// </summary>
        public static bool Started { get; private set; }

        /// <summary>
        /// The memory allocator currently in-use by ENet
        /// </summary>
        public static ENetAllocator Allocator
        {
            get
            {
                if (!Started)
                    ThrowHelper.ThrowENetIsNotInitialized();

                return s_Allocator!;
            }
        }

        /// <summary>
        /// The ENet static-linked version 
        /// </summary>
        public static Version LinkedVersion
        {
            get
            {
                if (!Started)
                    ThrowHelper.ThrowENetIsNotInitialized();

                return s_LinkedVersion!;
            }
        }

        static ManagedENet()
        {
            Started = false;
            MemAllocDelegate = MemAllocCallback;
            MemFreeDelegate = MemFreeCallback;
            NoMemoryDelegate = NoMemoryCallback;
            
            s_LinkedVersion = null!;
            s_Allocator = null!;
        }

        /// <summary>
        /// Initializes ENEt with specified memory allocator
        /// </summary>
        /// <param name="allocator">If this parameter receives null <see cref="ENetGlobalHeapAllocator"/> will be used instead</param>
        public static void Startup(ENetAllocator? allocator = null)
        {
            if (Started) return;
            Started = true;

            s_Allocator = (allocator == null) ? ENetGlobalHeapAllocator.Instance : allocator;

            LibENet.Load();

            NativeENetCallbacks callbacks = new NativeENetCallbacks();
            callbacks.Malloc = Marshal.GetFunctionPointerForDelegate(MemAllocDelegate);
            callbacks.Free = Marshal.GetFunctionPointerForDelegate(MemFreeDelegate);
            callbacks.NoMemory = Marshal.GetFunctionPointerForDelegate(NoMemoryDelegate);

            var linkedVer = LibENet.LinkedVersion();
            if (LibENet.InitializeWithCallbacks(linkedVer, &callbacks) != 0)
                ThrowHelper.ThrowENetInitializationFailed();

            s_LinkedVersion = new Version((int)(((linkedVer) >> 16) & 0xFF),
                                          (int)(((linkedVer) >> 8) & 0xFF),
                                          (int)((linkedVer) & 0xFF));
        }

        /// <summary>
        /// Shutdowns and unloads ENet dynamic library
        /// </summary>
        /// <param name="delete">Specifies the ENet dynamic library should be removed or not</param>
        /// <remarks>
        /// Any interaction with ENet managed wrapper instances like <see cref="ENetHost"/> should be avoided
        /// after calling this method
        /// </remarks>
        public static void Shutdown(bool delete = true)
        {
            if (!Started) return;
            Started = false;

            LibENet.Unload();
            if (delete) LibENet.TryDelete();

            s_Allocator!.Dispose();
            s_Allocator = null;
        }

        private static void NoMemoryCallback() => throw new OutOfMemoryException("ENet rans out of memory");
        private static IntPtr MemAllocCallback(UIntPtr size)
        {
            var allocator = s_Allocator!;

            if (allocator != null)
            {
                return allocator!.Allocate((int)size.ToUInt32());
            }
            else
            {
                ThrowHelper.ThrowENetAllocatorRefIsNull();
                return IntPtr.Zero;
            }
        }

        private static void MemFreeCallback(IntPtr memory)
        {
            var allocator = s_Allocator!;

            if (allocator != null)
            {
                allocator.Free(memory);
            }
            else
            {
                ThrowHelper.ThrowENetAllocatorRefIsNull();
            }
        }
    }
}
