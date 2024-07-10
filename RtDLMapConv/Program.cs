using System;
using KirbyLib.IO;
using KirbyLib.Mapping;

internal class Program
{
    static void Main(string[] args)
    {
        if (args.Length > 0)
        {
            string path = Path.GetFullPath(args[0]);

            string outpath = Path.GetDirectoryName(path) + "\\" + Path.GetFileNameWithoutExtension(path);

            MapRtDL map;
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (EndianBinaryReader reader = new EndianBinaryReader(stream))
                map = new MapRtDL(reader);

            string ext;
            if (map.XData.Version[0] == 2)
            {
                map.XData.Version = new byte[] { 5, 0 };
                map.XData.Endianness = Endianness.Little;
                ext = "bin";
            }
            else
            {
                map.XData.Version = new byte[] { 2, 0 };
                map.XData.Endianness = Endianness.Big;
                ext = "dat";
            }

            outpath += "." + ext;

            using (FileStream stream = new FileStream(outpath, FileMode.Create, FileAccess.Write))
            using (EndianBinaryWriter writer = new EndianBinaryWriter(stream))
                map.Write(writer);

            if (map.XData.Version[0] == 5)
                Console.WriteLine("Successfully converted map file to Deluxe!\nWritten to " + outpath);
            else
                Console.WriteLine("Successfully converted map file to Wii!\nWritten to " + outpath);
        }
    }
}
