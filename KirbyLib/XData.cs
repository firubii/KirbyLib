using KirbyLib.IO;
using System.Text;

namespace KirbyLib
{
    /// <summary>
    /// Helper class for the standard XData header found in most data files
    /// </summary>
    public class XData
    {
        public const string XDATA_MAGIC = "XBIN";
        public const string RLOC_MAGIC = "RLOC";

        /// <summary>
        /// The endianness of the file
        /// </summary>
        public Endianness Endianness;

        /// <summary>
        /// XData header version<br/>Defines certain features of the header
        /// </summary>
        public byte[] Version = new byte[2];

        /// <summary>
        /// A value of unknown purpose found at 0xC in the header<br/>Usually 65001, editing is not advised
        /// </summary>
        public uint Unknown_0xC = 65001;

        /// <summary>
        /// Reads an XData header from a given stream
        /// </summary>
        /// <exception cref="InvalidDataException"></exception>
        public void Read(EndianBinaryReader reader)
        {
            string magic = Encoding.UTF8.GetString(reader.ReadBytes(4));
            if (magic != XDATA_MAGIC)
                throw new InvalidDataException("XData magic \"XBIN\" not found!");

            // Byte order mark; If 0x1234 is read, then the endianness of the reader must be flipped
            // In the actual data, this value is 0x3412 in little endian and 0x1234 in big endian
            ushort bom = reader.ReadUInt16();
            if (bom != 0x1234)
                Endianness = Endianness.Big;
            else
                Endianness = Endianness.Little;

            reader.Endianness = Endianness;

            Version = reader.ReadBytes(2);

            reader.ReadUInt32(); // File length, not needed for storing the data

            Unknown_0xC = reader.ReadUInt32();

            // RLOC address, not needed for storing the data
            if (Version[0] > 2)
                reader.ReadUInt32();
        }

        /// <summary>
        /// Writes an XData header to a given stream.<br/><b>Note:</b> Filesize and footer information are not written in this step and must be done afterwards
        /// </summary>
        public void WriteHeader(EndianBinaryWriter writer)
        {
            writer.Endianness = Endianness;

            writer.Write(XDATA_MAGIC.ToCharArray());
            writer.Write((ushort)0x1234);
            writer.Write(Version);

            // File length, MUST be calculated later
            writer.Write(-1);
            
            writer.Write(Unknown_0xC);

            // RLOC address, MUST be calculated later
            if (Version[0] > 2)
                writer.Write(-1);
        }

        /// <summary>
        /// Writes the current filesize to a given stream.<br/>Must be done after header is written.
        /// </summary>
        public void WriteFilesize(EndianBinaryWriter writer)
        {
            writer.BaseStream.Seek(0, SeekOrigin.End);
            writer.WritePositionAt(0x8);
        }

        /// <summary>
        /// Writes an RLOC footer to the end of a given stream, if XData version is 4.0 or later.<br/>Must be done after header is written.
        /// </summary>
        public void WriteFooter(EndianBinaryWriter writer)
        {
            if (Version[0] > 2)
            {
                writer.BaseStream.Seek(0, SeekOrigin.End);
                writer.WritePadding();

                writer.WritePositionAt(0x10);

                writer.Write(RLOC_MAGIC.ToCharArray());
                writer.Write(0);
                writer.Write(0);
            }
        }
    }
}
