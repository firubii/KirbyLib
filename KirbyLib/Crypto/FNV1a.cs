using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KirbyLib.Crypto
{
    public static class FNV1a
    {
        public static ulong Calculate(byte[] data)
        {
            ulong hash = 0xcbf29ce484222325;

            for (var i = 0; i < data.Length; i++)
            {
                hash = hash ^ data[i];
                hash *= 0x100000001b3;
            }

            return hash;
        }
    }
}
