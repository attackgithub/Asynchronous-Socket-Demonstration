using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace socketDemonstration.Network.Shared
{
    public static class Conversions
    {
        public static int FromBytes(byte[] buffer, int index)
        {
            int j = 0;
            int r = 0;
            for (int i = index; i < index + 4; i++)
            {
                r |= buffer[i] << j * 8;
                j++;
            }
            return r;
        }

        public static byte[] ToBytes(int value )
        {
            byte[] buffer = new byte[4];
            for (int i =0; i < 4; i ++)
            {
                buffer[i] = (byte)(value >> i * 8);
            }
            return buffer;
        }
    }
}
