using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntWay.Core.Manager
{
    public class Checksum
    {
        public static Checksum Single => new Checksum();

        public enum ChecksumType { Input, Output }

        public string CalculateChecksum(string dataToCalculate)
        {
            String hash = String.Empty;

            byte[] byteToCalculate = Encoding.ASCII.GetBytes(dataToCalculate);
            foreach (byte b in Crc32.ComputeChecksumBytes(byteToCalculate))
            {
                hash += b.ToString("x2").ToLower();
            }
                
            return hash;
        }
    }

    public static class Crc32
    {
        static uint[] table;

        public static uint ComputeChecksum(byte[] bytes)
        {
            table = CreateTable();
            uint crc = 0xffffffff;
            for (int i = 0; i < bytes.Length; ++i)
            {
                byte index = (byte)(((crc) & 0xff) ^ bytes[i]);
                crc = (uint)((crc >> 8) ^ table[index]);
            }
            return ~crc;
        }

        public static byte[] ComputeChecksumBytes(byte[] bytes)
        {
            return BitConverter.GetBytes(ComputeChecksum(bytes));
        }

        private static uint[] CreateTable()
        {
            uint poly = 0xedb88320;
            table = new uint[256];
            uint temp = 0;
            for (uint i = 0; i < table.Length; ++i)
            {
                temp = i;
                for (int j = 8; j > 0; --j)
                {
                    if ((temp & 1) == 1)
                    {
                        temp = (uint)((temp >> 1) ^ poly);
                    }
                    else
                    {
                        temp >>= 1;
                    }
                }
                table[i] = temp;
            }

            return table;
        }
    }
}
