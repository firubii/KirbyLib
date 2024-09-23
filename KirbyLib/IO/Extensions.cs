using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace KirbyLib.IO
{
    public static class Extensions
    {
        public static string ReadStringOffset(this BinaryReader reader, uint offset = 0)
        {
            uint addr = reader.ReadUInt32() + offset;
            long pos = reader.BaseStream.Position;

            reader.BaseStream.Position = addr;
            string str = reader.ReadStringHAL();

            reader.BaseStream.Position = pos;
            return str;
        }

        public static string ReadStringHAL(this BinaryReader reader)
        {
            return Encoding.UTF8.GetString(reader.ReadBytes(reader.ReadInt32()));
        }

        public static string ReadUnicodeStringHAL(this BinaryReader reader)
        {
            return Encoding.Unicode.GetString(reader.ReadBytes(reader.ReadInt32() * 2));
        }

        public static void Align(this BinaryReader reader, int alignment = 0x4)
        {
            while ((reader.BaseStream.Position % alignment) != 0x0)
                reader.BaseStream.Position++;
        }

        public static void WriteStringHAL(this BinaryWriter writer, string str)
        {
            byte[] strBytes = Encoding.UTF8.GetBytes(str);
            writer.Write(strBytes.Length);
            writer.Write(strBytes);
            writer.Write((byte)0);
            WritePadding(writer);
        }

        public static void WriteUnicodeStringHAL(this BinaryWriter writer, string str)
        {
            byte[] strBytes = Encoding.Unicode.GetBytes(str);
            writer.Write(strBytes.Length / 2);
            writer.Write(strBytes);
            writer.Write((short)0);
            WritePadding(writer);
        }

        public static void WritePadding(this BinaryWriter writer, int alignment = 0x4)
        {
            while ((writer.BaseStream.Position % alignment) != 0x0)
                writer.Write((byte)0);
        }

        public static void Align(this BinaryWriter writer, int alignment = 0x4)
        {
            while ((writer.BaseStream.Position % alignment) != 0x0)
                writer.BaseStream.Position++;
        }

        public static void WritePositionAt(this BinaryWriter writer, long position)
        {
            uint pos = (uint)writer.BaseStream.Position;
            writer.BaseStream.Position = position;
            writer.Write(pos);
            writer.BaseStream.Position = pos;
        }

        public static void WriteRelativePositionAt(this BinaryWriter writer, long position)
        {
            uint pos = (uint)writer.BaseStream.Position;
            writer.BaseStream.Position = position;
            writer.Write((uint)(pos - position));
            writer.BaseStream.Position = pos;
        }

        public static Vector3 ReadVector3(this BinaryReader reader)
        {
            return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        public static void Write(this BinaryWriter writer, Vector3 vector)
        {
            writer.Write(vector.X);
            writer.Write(vector.Y);
            writer.Write(vector.Z);
        }
    }
}
