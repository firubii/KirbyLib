using KirbyLib.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KirbyLib
{
    /// <summary>
    /// Class for handling generic archives containing files with XData headers.<br/>
    /// Typically used for game parameters outside of Cinemo and Yaml files.
    /// </summary>
    public class GenericArchive
    {
        public struct FileInfo
        {
            /// <summary>
            /// The name of the file, usually a path.
            /// </summary>
            public string Name;
            /// <summary>
            /// The file's raw data.
            /// </summary>
            public byte[] Data;
        }

        public XData XData { get; private set; } = new XData();

        public List<FileInfo> Files = new List<FileInfo>();

        public GenericArchive() { }

        public GenericArchive(EndianBinaryReader reader)
        {
            Read(reader);
        }

        public void Read(EndianBinaryReader reader)
        {
            XData.Read(reader);

            Files = new List<FileInfo>();

            long listStart = reader.BaseStream.Position;
            uint count = reader.ReadUInt32();
            for (int i = 0; i < count; i++)
            {
                reader.BaseStream.Position = listStart + 4 + (i * 8);

                FileInfo info = new FileInfo();
                info.Name = reader.ReadStringOffset();

                reader.BaseStream.Position = reader.ReadUInt32();

                reader.BaseStream.Position += 0x8;
                int fileLength = reader.ReadInt32();
                reader.BaseStream.Position -= 0xC;

                info.Data = reader.ReadBytes(fileLength);
            }
        }

        public void Write(EndianBinaryWriter writer)
        {
            XData.WriteHeader(writer);

            StringHelperContainer strings = new StringHelperContainer();

            long listStart = writer.BaseStream.Position;
            writer.Write(Files.Count);
            for (int i = 0; i < Files.Count; i++)
            {
                strings.Add(writer.BaseStream.Position, Files[i].Name);
                writer.Write(-1);
                writer.Write(-1);
            }

            for (int i = 0; i < Files.Count; i++)
            {
                writer.WritePositionAt(listStart + 4 + (i * 8) + 4);
                writer.Write(Files[i].Data);
                writer.WritePadding();
            }

            strings.WriteAll(writer);

            XData.WriteFilesize(writer);
            XData.WriteFooter(writer);
        }
    }
}
