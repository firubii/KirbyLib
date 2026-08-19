using KirbyLib.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KirbyLib.Mint
{
    public static class MintUtil
    {
        public static uint CalculateHash(string typeName, string name, bool bigEndian = false)
        {
            return CRC32C.Calculate($"{typeName}.{name}", bigEndian);
        }

        public static string TrimFunctionType(string name)
        {
            bool searchBackwards = false;
            int trimIdx = 0;
            for (int i = 0; i < name.Length && i >= 0;)
            {
                if (searchBackwards)
                {
                    if (name[i] == ' ')
                    {
                        trimIdx = i + 1;
                        break;
                    }
                }
                else
                {
                    if (name[i] == '(')
                        searchBackwards = true;
                }

                i += searchBackwards ? -1 : 1;
            }

            return new string(name.Skip(trimIdx).ToArray());
        }

        public static string TrimFunctionSymbols(string name)
        {
            bool searchBackwards = false;
            int endIdx = 0;
            int trimIdx = 0;
            for (int i = 0; i < name.Length && i >= 0;)
            {
                if (searchBackwards)
                {
                    if (name[i] == ' ')
                    {
                        trimIdx = i + 1;
                        break;
                    }
                }
                else
                {
                    if (name[i] == '(')
                    {
                        searchBackwards = true;
                        endIdx = i;
                    }
                }

                i += searchBackwards ? -1 : 1;
            }

            return new string(name.Skip(trimIdx).Take(endIdx - trimIdx).ToArray());
        }
    }
}
