using KirbyLib.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KirbyLib
{
    /// <summary>
    /// A filter file that controls which glyphs get loaded with each font.
    /// </summary>
    public class MsgFilter
    {
        public struct FontFilter
        {
            /// <summary>
            /// The name of the font.
            /// </summary>
            public string Font;
            /// <summary>
            /// The list of characters that will be loaded.
            /// </summary>
            public string Characters;
        }

        public XData XData { get; private set; } = new XData();

        public List<FontFilter> Filters = new List<FontFilter>();

        public MsgFilter() { }

        public MsgFilter(EndianBinaryReader reader)
        {
            Read(reader);
        }

        public void Read(EndianBinaryReader reader)
        {
            XData.Read(reader);

            long listStart = reader.BaseStream.Position;

            Filters = new List<FontFilter>();
            uint count = reader.ReadUInt32();
            for (uint i = 0; i < count; i++)
            {
                reader.BaseStream.Position = listStart + 4 + (i * 4);
                reader.BaseStream.Position = reader.ReadUInt32();

                FontFilter filter = new FontFilter();
                filter.Font = reader.ReadStringOffset();
                filter.Characters = reader.ReadUnicodeStringHAL();

                Filters.Add(filter);
            }
        }

        public void Write(EndianBinaryWriter writer)
        {
            XData.WriteHeader(writer);

            StringHelperContainer strings = new StringHelperContainer();

            long listStart = writer.BaseStream.Position;

            writer.Write(Filters.Count);
            for (int i = 0; i < Filters.Count; i++)
                writer.Write(-1);

            for (int i = 0; i < Filters.Count; i++)
            {
                var filter = Filters[i];

                writer.WritePositionAt(listStart + 4 + (i * 4));

                strings.Add(writer.BaseStream.Position, filter.Font);
                writer.Write(-1);
                writer.WriteUnicodeStringHAL(filter.Characters);
                writer.WritePadding(0x4);
            }

            strings.WriteAll(writer);

            XData.WriteFilesize(writer);
            XData.WriteFooter(writer);
        }
    }
}
