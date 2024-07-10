using KirbyLib.IO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace KirbyLib
{
    /// <summary>
    /// Handles reading and writing of the older Cinemo Dynamics (CND) format.<br/><br/>
    /// Works for CND files found in:
    /// <list type="bullet">
    ///     <item>Kirby Star Allies</item>
    ///     <item>Super Kirby Clash</item>
    /// </list>
    /// </summary>
    public class CinemoKSA
    {
        /// <summary>
        /// A Cinemo object.<br/>
        /// Contains its name, type, and variables.
        /// </summary>
        public class CinemoObject
        {
            public string Name;
            public string Type;
            public List<CinemoVariable> Variables = new List<CinemoVariable>();
        }

        public const ulong MAGIC_NUMBER = 10100;
        public const uint MAGIC_NUMBER_2 = 0x24;

        public XData XData { get; private set; } = new XData()
        {
            Version = new byte[] { 4, 0 },
            Endianness = Endianness.Little
        };

        /// <summary>
        /// The internal name of the Cinemo file.
        /// </summary>
        public string Name;
        /// <summary>
        /// The internal type of the Cinemo file.
        /// </summary>
        public string Type;

        public List<CinemoObject> VisualSection = new List<CinemoObject>();
        public List<CinemoObject> RenderSection = new List<CinemoObject>();

        public CinemoKSA() { }

        public CinemoKSA(EndianBinaryReader reader)
        {
            Read(reader);
        }

        public void Read(EndianBinaryReader reader)
        {
            XData.Read(reader);

            ulong magic = reader.ReadUInt64();
            if (magic != MAGIC_NUMBER)
                throw new InvalidDataException($"Expected magic {MAGIC_NUMBER}, got {magic}");

            uint magic2 = reader.ReadUInt32();
            if (magic2 != MAGIC_NUMBER_2)
                throw new InvalidDataException($"Expected second magic {MAGIC_NUMBER_2}, got {magic2}");

            uint visualSection = reader.ReadUInt32();
            if (reader.ReadUInt32() != 0)
                Console.WriteLine($"Cinemo file has non-zero value at 0x{reader.BaseStream.Position - 4:X8}");

            Name = reader.ReadStringOffset();
            Type = reader.ReadStringOffset();
            uint renderSection = reader.ReadUInt32();

            reader.BaseStream.Position = renderSection;
            RenderSection = ReadObjectSection(reader);

            reader.BaseStream.Position = visualSection;
            VisualSection = ReadObjectSection(reader);
        }

        List<CinemoObject> ReadObjectSection(EndianBinaryReader reader)
        {
            long sectionStart = reader.BaseStream.Position;
            List<CinemoObject> objects = new List<CinemoObject>();

            uint objCount = reader.ReadUInt32();
            for (int i = 0; i < objCount; i++)
            {
                reader.BaseStream.Position = sectionStart + 4 + (i * 4);
                reader.BaseStream.Position = reader.ReadUInt32();

                CinemoObject obj = new CinemoObject();
                obj.Name = reader.ReadStringOffset();
                obj.Type = reader.ReadStringOffset();
                if (reader.ReadInt32() != 0)
                    Console.WriteLine($"CinemoObject {i} has non-zero value at 0x8!");
                reader.BaseStream.Position = reader.ReadUInt32();

                obj.Variables = new List<CinemoVariable>();

                long varListStart = reader.BaseStream.Position;
                uint varCount = reader.ReadUInt32();
                for (int v = 0; v < varCount; v++)
                {
                    reader.BaseStream.Position = varListStart + 4 + (v * 4);
                    reader.BaseStream.Position = reader.ReadUInt32();

                    CinemoVariable var = new CinemoVariable(reader);
                    obj.Variables.Add(var);
                }

                objects.Add(obj);
            }

            return objects;
        }

        public void Write(EndianBinaryWriter writer)
        {
            XData.WriteHeader(writer);

            StringHelperContainer strings = new StringHelperContainer();

            long header = writer.BaseStream.Position;

            writer.Write(MAGIC_NUMBER);
            writer.Write(MAGIC_NUMBER_2);
            writer.Write(-1);
            writer.Write(0);
            strings.Add(writer.BaseStream.Position, Name);
            writer.Write(-1);
            strings.Add(writer.BaseStream.Position, Type);
            writer.Write(-1);
            writer.Write(-1);

            writer.WritePositionAt(header + 0x1C);
            WriteObjectSection(writer, RenderSection, strings);

            writer.WritePositionAt(header + 0xC);
            WriteObjectSection(writer, VisualSection, strings);

            strings.WriteAll(writer);

            XData.WriteFilesize(writer);
            XData.WriteFooter(writer);
        }

        void WriteObjectSection(EndianBinaryWriter writer, List<CinemoObject> objects, StringHelperContainer strings)
        {
            long sectionStart = writer.BaseStream.Position;
            writer.Write(objects.Count);
            for (int i = 0; i < objects.Count; i++)
                writer.Write(-1);

            for (int i = 0; i < objects.Count; i++)
            {
                var cObj = objects[i];

                writer.WritePositionAt(sectionStart + 4 + (i * 4));

                strings.Add(writer.BaseStream.Position, cObj.Name);
                writer.Write(-1);
                strings.Add(writer.BaseStream.Position, cObj.Type);
                writer.Write(-1);
                writer.Write(0);
                writer.Write(writer.BaseStream.Position + 4);

                long varListStart = writer.BaseStream.Position;
                writer.Write(cObj.Variables.Count);
                for (int v = 0; v < cObj.Variables.Count; v++)
                    writer.Write(-1);

                for (int v = 0; v < cObj.Variables.Count; v++)
                {
                    var var = cObj.Variables[v];

                    writer.WritePositionAt(varListStart + 4 + (v * 4));

                    strings.Add(writer.BaseStream.Position, var.Name);
                    writer.Write(-1);
                    strings.Add(writer.BaseStream.Position, var.Type.ToString());
                    writer.Write(-1);
                    writer.Write(0);

                    var.WriteData(writer);
                }
            }
        }
    }
}
