using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace socketDemonstration.Network.Shared
{
    public class Packet
    {
        public byte[] Data
        {
            get { return m_Data; }
            protected set { m_Data = value; }
        }

        public static Packet Create(byte[] data)
        {
            Packet packet = new Packet();
            packet.Data = data;
            return packet;
        }

        public byte[] Serialize()
        {
            byte[] data = (byte[])Data.Clone();
            int dLength = data.Length;

            Array.Resize(ref data, dLength + 8);

            byte[] packetSignature = Conversions.ToBytes(0xBEEF);
            byte[] payloadLength = Conversions.ToBytes(dLength);

            Buffer.BlockCopy(data, 0, data, 8, dLength);

            Buffer.BlockCopy(packetSignature, 0, data, 0, 4);
            Buffer.BlockCopy(payloadLength, 0, data, 4, 4);

            return data;
        }

        private byte[] m_Data;
    }
}
