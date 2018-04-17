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
    public unsafe class ENetPacket
    {
        internal byte[] _Payload;

        public ENetPacketFlags Flags { get; private set; }
        public byte Channel { get; private set; }

        internal ENetPacket()
        {

        }

        internal ENetPacket(Native.ENetPacket* packet, byte channel)
        {
            Flags = packet->Flags;
            _Payload = new byte[packet->DataLength.ToUInt32()];
            fixed (byte* dest = _Payload)
            {
                ENetUtils.MemoryCopy((IntPtr)dest, (IntPtr)packet->Data, packet->DataLength);
            }
            Channel = channel;
        }

        public byte[] GetPayloadFinal()
        {
            return _Payload;
        }

        public byte[] GetPayloadCopy()
        {
            byte[] clone = new byte[_Payload.Length];
            ENetUtils.MemoryCopy(clone, _Payload, clone.Length);
            return clone;
        }

        public MemoryStream GetPayloadStream(bool copy)
        {
            if (copy)
            {
                return new MemoryStream(GetPayloadCopy());
            }

            return new MemoryStream(_Payload, false);
        }
    }
}
