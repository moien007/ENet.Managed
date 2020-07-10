using System;
using System.Runtime.InteropServices;

namespace ENet.Managed
{
    public static unsafe class ENetExtensionMethods
    {
        public static void SetUserData<TData>(this ENetPeer peer, TData data)
        {
            var native = peer.GetNativePointer();
            SetUserData(ref native->Data, data);
        }

        public static void SetUserData<TData>(this ENetPacket packet, TData data)
        {
            var native = packet.GetNativePointer();
            SetUserData(ref native->UserData, data);
        }

        public static bool TryGetUserData<TData>(this ENetPeer peer, out TData data)
        {
            var native = peer.GetNativePointer();
            return TryGetUserData(native->Data, out data);
        }

        public static bool TryGetUserData<TData>(this ENetPacket packet, out TData data)
        {
            var native = packet.GetNativePointer();
            return TryGetUserData(native->UserData, out data);
        }

        public static void UnsetUserData(this ENetPeer peer)
        {
            var native = peer.GetNativePointer();
            UnsetUserData(ref native->Data);
        }

        public static void UnsetUserData(this ENetPacket packet)
        {
            var native = packet.GetNativePointer();
            UnsetUserData(ref native->UserData);
        }

        private static void SetUserData<TData>(ref IntPtr dataField, TData data)
        {
            GCHandle gcHandle;
            ENetUserDataContainer<TData> container;
            if (dataField == IntPtr.Zero)
                goto allocNew;

            gcHandle = GCHandle.FromIntPtr(dataField);
            if (!gcHandle.IsAllocated)
                goto allocNew;

            if (gcHandle.Target is ENetUserDataContainer<TData> casted)
            {
                casted.Data = data;
                return;
            }
            else
            {
                gcHandle.Free();
                goto allocNew;
            }

        allocNew:
            container = new ENetUserDataContainer<TData>(data);
            gcHandle = GCHandle.Alloc(container, GCHandleType.Normal);
            dataField = GCHandle.ToIntPtr(gcHandle);
            return;
        }

        private static void UnsetUserData(ref IntPtr dataField)
        {
            if (dataField == IntPtr.Zero)
                return;

            var gcHandle = GCHandle.FromIntPtr(dataField);
            if (gcHandle.IsAllocated)
                gcHandle.Free();

            dataField = IntPtr.Zero;
        }

        private static bool TryGetUserData<TData>(IntPtr dataFieldValue, out TData data)
        {
            var container = TryGetUserDataContainer(dataFieldValue);
            if (container == null)
            {
                data = default!;
                return false;
            }

            if (container is ENetUserDataContainer<TData> castedContainer)
            {
                data = castedContainer.Data;
                return true;
            }

            if (container.GetData() is TData casted)
            {
                data = casted;
                return true;
            }

            data = default!;
            return false;
        }

        private static IENetUserDataContainer? TryGetUserDataContainer(IntPtr dataFieldValue)
        {
            if (dataFieldValue == IntPtr.Zero)
                return null;

            var gcHandle = GCHandle.FromIntPtr(dataFieldValue);
            if (!gcHandle.IsAllocated)
                return null;

            if (gcHandle.Target is IENetUserDataContainer casted)
                return casted;
            else
                return null;
        }
    }
}
