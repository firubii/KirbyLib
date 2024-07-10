using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KirbyLib.IO
{
    public class StringHelperContainer : List<StringHelper>
    {
        public void Add(long position, string str)
        {
            Add(new StringHelper(position, str));
        }

        new public void Add(StringHelper stringHelper)
        {
            for (int i = 0; i < Count; i++)
            {
                if (this[i].String == stringHelper.String)
                {
                    this[i].WriteAddresses.AddRange(stringHelper.WriteAddresses);
                    return;
                }
            }

            base.Add(stringHelper);
        }

        public void WriteAll(EndianBinaryWriter writer)
        {
            for (int i = 0; i < Count; i++)
            {
                StringHelper sh = this[i];
                for (int a = 0; a < sh.WriteAddresses.Count; a++)
                    writer.WritePositionAt(sh.WriteAddresses[a]);

                writer.WriteStringHAL(sh.String);
            }
        }

        public void WriteAllRelative(EndianBinaryWriter writer)
        {
            for (int i = 0; i < Count; i++)
            {
                StringHelper sh = this[i];
                for (int a = 0; a < sh.WriteAddresses.Count; a++)
                    writer.WriteRelativePositionAt(sh.WriteAddresses[a]);

                writer.WriteStringHAL(sh.String);
            }
        }
    }

    public struct StringHelper
    {
        public List<long> WriteAddresses;
        public string String;

        public StringHelper(long addr, string str)
        {
            WriteAddresses = new List<long>() { addr };
            String = str;
        }
    }
}
