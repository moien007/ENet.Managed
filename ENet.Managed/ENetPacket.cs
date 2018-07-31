using System;
using System.IO;
using Native = ENet.Managed.Structures;

namespace ENet.Managed
{
    public unsafe class ENetPacket
    {
        internal byte[] m_Payload;

        public byte Channel { get; }
        public ENetPacketFlags Flags { get; }

        internal ENetPacket() { }

        internal ENetPacket(Native.ENetPacket* packet, byte channel)
        {
            Flags = packet->Flags;
            m_Payload = new byte[packet->DataLength.ToUInt32()];
            fixed (byte* dest = m_Payload)
            {
                ENetUtils.MemoryCopy((IntPtr)dest, (IntPtr)packet->Data, packet->DataLength);
            }
            Channel = channel;
        }

        public byte[] GetPayloadFinal() => m_Payload;
        public byte[] GetPayloadCopy()
        {
            byte[] clone = new byte[m_Payload.Length];
            ENetUtils.MemoryCopy(clone, m_Payload, clone.Length);
            return clone;
        }

        public MemoryStream GetPayloadStream(bool copy)
        {
            if (copy)
            {
                return new MemoryStream(GetPayloadCopy(), false);
            }

            return new MemoryStream(m_Payload, false);
        }
    }
}
