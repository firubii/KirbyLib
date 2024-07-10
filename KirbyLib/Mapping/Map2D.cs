using KirbyLib.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace KirbyLib.Mapping
{
    /// <summary>
    /// Basic class for handling 2D maps
    /// </summary>
    public abstract class Map2D
    {
        public const uint HEADER_END = 0x12345678;

        public XData XData { get; protected set; } = new XData();

        /// <summary>
        /// The width of the map, in tiles.
        /// </summary>
        public abstract int Width { get; }
        /// <summary>
        /// The height of the map, in tiles.
        /// </summary>
        public abstract int Height { get; }

        public Map2D() { }

        public Map2D(int width, int height) { }

        public Map2D(EndianBinaryReader reader)
        {
            Read(reader);
        }

        public abstract void Read(EndianBinaryReader reader);
        public abstract void Write(EndianBinaryWriter writer);

        protected CollisionTile[,] ReadCollision(EndianBinaryReader reader)
        {
            uint width = reader.ReadUInt32();
            uint height = reader.ReadUInt32();
            CollisionTile[,] collision = new CollisionTile[width, height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    collision[x, y] = new CollisionTile(
                        (LandGridShapeKind)reader.ReadByte(),
                        (LandGridProperty)reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadSByte()
                    );
                }
            }
            return collision;
        }

        protected void WriteCollision(EndianBinaryWriter writer, CollisionTile[,] collision)
        {
            int width = collision.GetLength(0);
            int height = collision.GetLength(1);
            writer.Write(width);
            writer.Write(height);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var tile = collision[x, y];
                    writer.Write((byte)tile.Shape);
                    writer.Write((byte)tile.PropertyFlags);
                    writer.Write(tile.Material);
                    writer.Write(tile.ConveyorSpeed);
                }
            }
        }

        protected DecorationTile[,] ReadDecorLayer(EndianBinaryReader reader)
        {
            uint width = reader.ReadUInt32();
            uint height = reader.ReadUInt32();
            DecorationTile[,] layer = new DecorationTile[width, height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    DecorationTile tile = new DecorationTile(
                        reader.ReadInt16(),
                        reader.ReadByte(),
                        reader.ReadSByte()
                    );

                    layer[x, y] = tile;
                }
            }

            return layer;
        }

        protected void WriteDecorLayer(EndianBinaryWriter writer, DecorationTile[,] layer)
        {
            int width = layer.GetLength(0);
            int height = layer.GetLength(1);
            writer.Write(width);
            writer.Write(height);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var tile = layer[x, y];
                    writer.Write(tile.Tile);
                    writer.Write(tile.Unknown);
                    writer.Write(tile.Group);
                }
            }
        }

        protected short[,] ReadBlocks(EndianBinaryReader reader)
        {
            uint width = reader.ReadUInt32();
            uint height = reader.ReadUInt32();
            short[,] blocks = new short[width, height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    blocks[x, y] = reader.ReadInt16();
                }
            }
            return blocks;
        }

        protected void WriteBlocks(EndianBinaryWriter writer, short[,] blocks)
        {
            int width = blocks.GetLength(0);
            int height = blocks.GetLength(1);
            writer.Write(width);
            writer.Write(height);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    writer.Write(blocks[x, y]);
                }
            }
        }

        protected Yaml ReadYaml(EndianBinaryReader reader)
        {
            long pos = reader.BaseStream.Position;
            reader.BaseStream.Position += 0xC;
            int size = reader.ReadInt32();

            reader.BaseStream.Position = pos;
            byte[] rawYaml = reader.ReadBytes(size);

            Yaml yaml;
            using (MemoryStream stream = new MemoryStream(rawYaml))
            using (EndianBinaryReader yamlReader = new EndianBinaryReader(stream))
                yaml = new Yaml(yamlReader);

            return yaml;
        }

        protected void WriteYaml(EndianBinaryWriter writer, Yaml yaml)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (EndianBinaryWriter yamlWriter = new EndianBinaryWriter(stream))
                    yaml.Write(yamlWriter);

                writer.Write(stream.ToArray());
            }
        }

        protected List<Yaml> ReadYamlSection(EndianBinaryReader reader)
        {
            long sectionStart = reader.BaseStream.Position;
            uint count = reader.ReadUInt32();
            List<Yaml> yamls = new List<Yaml>();
            for (int i = 0; i < count; i++)
            {
                reader.BaseStream.Position = sectionStart + 4 + (i * 4);
                reader.BaseStream.Position = reader.ReadUInt32();

                yamls.Add(ReadYaml(reader));
            }

            return yamls;
        }

        protected void WriteYamlSection(EndianBinaryWriter writer, List<Yaml> yamlArray)
        {
            long sectionStart = writer.BaseStream.Position;

            writer.Write(yamlArray.Count);
            for (int i = 0; i < yamlArray.Count; i++)
                writer.Write(-1);

            for (int i = 0; i < yamlArray.Count; i++)
            {
                writer.WritePositionAt(sectionStart + 4 + (i * 4));
                WriteYaml(writer, yamlArray[i]);
            }
        }
    }
}
