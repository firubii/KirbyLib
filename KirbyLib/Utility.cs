using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KirbyLib
{
    public class Utility
    {
        public int CountBits(int value)
        {
            int c = 0;
            for (int i = 0; i < 32; i++)
            {
                if ((value & (1 << i)) != 0)
                    c++;
            }
            return c;
        }
    }
}
