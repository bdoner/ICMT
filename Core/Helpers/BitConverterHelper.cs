using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Helpers
{
    public enum Endianness
    {
        BigEndian,
        LittleEndian
    }
    public static class BitConverterHelper
    {
        public static byte[] GetBytes(uint u, Endianness endianness)
        {
            var bytes = BitConverter.GetBytes(u);
            if (
                BitConverter.IsLittleEndian && endianness == Endianness.LittleEndian
             || !BitConverter.IsLittleEndian && endianness == Endianness.BigEndian) return bytes;

            bytes = bytes.Reverse().ToArray();
            return bytes;
        }

        public static byte[] GetBytes(ushort u, Endianness endianness)
        {
            var bytes = BitConverter.GetBytes(u);
            if (
                BitConverter.IsLittleEndian && endianness == Endianness.LittleEndian
             || !BitConverter.IsLittleEndian && endianness == Endianness.BigEndian) return bytes;

            bytes = bytes.Reverse().ToArray();
            return bytes;
        }
    }
}
