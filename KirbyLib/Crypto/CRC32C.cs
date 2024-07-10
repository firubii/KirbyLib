using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Force.Crc32;

namespace KirbyLib.Crypto
{
    public static class Crc32C
    {
        private static byte[] Calculate(string str)
        {
            byte[] hash;
            Crc32CAlgorithm crc = new Crc32CAlgorithm();
            hash = crc.ComputeHash(Encoding.UTF8.GetBytes(str));
            return hash;
        }

        /// <summary>
        /// Calculates an inverted Crc32C hash
        /// </summary>
        public static byte[] CalculateInv(string str)
        {
            byte[] hash = Calculate(str);
            for (int i = 0; i < hash.Length; i++)
                hash[i] = (byte)(255 - hash[i]);
            return hash;
        }
    }
}
