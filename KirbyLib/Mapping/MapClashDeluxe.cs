using KirbyLib.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace KirbyLib.Mapping
{
    /// <summary>
    /// A map file for Team Kirby Clash Deluxe.
    /// </summary>
    public class MapClashDeluxe : Map2D
    {
        #region Structs

        public struct Gimmick
        {
            public GridPos X;
            public GridPos Y;
            public string Name;
            public int Unknown1;
            public int TerrainGroup;
            public int Unknown2;
            public int Unknown3;
            public int Unknown4;
            public int Unknown5;
            public int Unknown6;
            public int Unknown7;
            public int Unknown8;
            public int Unknown9;
        }

        #endregion

        public const uint MAGIC_NUMBER = 0x25;

        public override int Width => Collision.GetLength(0);

        public override int Height => Collision.GetLength(1);

        /// <summary>
        /// Fixed collision tiles.
        /// </summary>
        public CollisionTile[,] Collision;
        /// <summary>
        /// Foreground decoration layer.
        /// </summary>
        public DecorationTile[,] FLand;
        /// <summary>
        /// Tileset decoration layer.
        /// </summary>
        public DecorationTile[,] MLand;
        /// <summary>
        /// Background decoration layer.
        /// </summary>
        public DecorationTile[,] BLand;

        /// <summary>
        /// Interactable blocks. If -1, there is no block.
        /// </summary>
        public short[,] Blocks;

        /// <summary>
        /// The background ID the map will use.
        /// </summary>
        public uint Background = 14;
        /// <summary>
        /// The decoration set ID the map will use.<br/>This controls which models to load for the tileset, background objects, and foreground objects.
        /// </summary>
        public uint DecorSet = 14;

        /// <summary>
        /// A list of Gimmick objects found in the map.
        /// </summary>
        public List<Gimmick> Gimmicks = new List<Gimmick>();
        /// <summary>
        /// A list of bosses found in the map.
        /// </summary>
        public List<Yaml> Bosses = new List<Yaml>();

        #region General Section

        /// <summary>
        /// The light set to load for the map.
        /// </summary>
        public string LightSet = "Grass";
        public Vector3 Unknown1;
        public Vector3 Unknown2;
        public Vector3 Unknown3;

        #endregion

        public MapClashDeluxe()
        {
            XData.Version = new byte[] { 2, 0 };
            XData.Endianness = Endianness.Little;
        }

        public MapClashDeluxe(int width, int height)
        {
            XData.Version = new byte[] { 2, 0 };
            XData.Endianness = Endianness.Little;

            Collision = new CollisionTile[width, height];
            BLand = new DecorationTile[width, height];
            MLand = new DecorationTile[width, height];
            FLand = new DecorationTile[width, height];
            Blocks = new short[width, height];
        }

        public MapClashDeluxe(EndianBinaryReader reader)
        {
            Read(reader);
        }

        public override void Read(EndianBinaryReader reader)
        {
            XData.Read(reader);

            uint magic = reader.ReadUInt32();
            if (magic != MAGIC_NUMBER)
                throw new InvalidDataException($"Expected magic {MAGIC_NUMBER}, got {magic}");

            uint blockSection = reader.ReadUInt32();
            uint collisionSection = reader.ReadUInt32();
            uint decorSection = reader.ReadUInt32();
            uint generalSection = reader.ReadUInt32();
            uint gimmickSection = reader.ReadUInt32();
            uint bossSection = reader.ReadUInt32();

            uint headerEnd = reader.ReadUInt32();
            if (headerEnd != HEADER_END)
                throw new InvalidDataException($"Expected header to end with 0x{HEADER_END:X8}, got 0x{headerEnd:X8}");

            reader.BaseStream.Position = blockSection;
            Blocks = ReadBlocks(reader);

            reader.BaseStream.Position = collisionSection;
            reader.BaseStream.Position = reader.ReadUInt32();
            Collision = ReadCollision(reader);

            reader.BaseStream.Position = decorSection;
            Background = reader.ReadUInt32();
            DecorSet = reader.ReadUInt32();

            reader.BaseStream.Position = decorSection + 0x8;
            reader.BaseStream.Position = reader.ReadUInt32();
            FLand = ReadDecorLayer(reader);

            reader.BaseStream.Position = decorSection + 0xC;
            reader.BaseStream.Position = reader.ReadUInt32();
            MLand = ReadDecorLayer(reader);

            reader.BaseStream.Position = decorSection + 0x10;
            reader.BaseStream.Position = reader.ReadUInt32();
            BLand = ReadDecorLayer(reader);

            reader.BaseStream.Position = generalSection;
            LightSet = reader.ReadStringOffset();
            Unknown1 = reader.ReadVector3();
            Unknown2 = reader.ReadVector3();
            Unknown3 = reader.ReadVector3();

            reader.BaseStream.Position = gimmickSection + 4;
            reader.BaseStream.Position = reader.ReadUInt32();
            uint gimmickNameCount = reader.ReadUInt32();
            string[] gimmickNames = new string[gimmickNameCount];
            for (int i = 0; i < gimmickNameCount; i++)
            {
                gimmickNames[i] = reader.ReadStringOffset();
            }

            reader.BaseStream.Position = gimmickSection;
            reader.BaseStream.Position = reader.ReadUInt32();
            uint gimmickCount = reader.ReadUInt32();
            Gimmicks = new List<Gimmick>();
            for (int i = 0; i < gimmickCount; i++)
            {
                Gimmick gimmick = new Gimmick();
                gimmick.X = reader.ReadUInt32();
                gimmick.Y = reader.ReadUInt32();
                gimmick.Name = gimmickNames[reader.ReadUInt32()];
                gimmick.Unknown1 = reader.ReadInt32();
                gimmick.TerrainGroup = reader.ReadInt32();
                gimmick.Unknown2 = reader.ReadInt32();
                gimmick.Unknown3 = reader.ReadInt32();
                gimmick.Unknown4 = reader.ReadInt32();
                gimmick.Unknown5 = reader.ReadInt32();
                gimmick.Unknown6 = reader.ReadInt32();
                gimmick.Unknown7 = reader.ReadInt32();
                gimmick.Unknown8 = reader.ReadInt32();
                gimmick.Unknown9 = reader.ReadInt32();

                Gimmicks.Add(gimmick);
            }

            reader.BaseStream.Position = bossSection;
            Bosses = ReadYamlSection(reader);
        }

        public override void Write(EndianBinaryWriter writer)
        {
            XData.WriteHeader(writer);

            StringHelperContainer strings = new StringHelperContainer();

            long headerStart = writer.BaseStream.Position;

            writer.Write(MAGIC_NUMBER);
            writer.Write(-1);
            writer.Write(-1);
            writer.Write(-1);
            writer.Write(-1);
            writer.Write(-1);
            writer.Write(-1);
            writer.Write(HEADER_END);
            writer.WritePadding(0x10);

            writer.WritePositionAt(headerStart + 0x4);
            WriteBlocks(writer, Blocks);
            writer.WritePadding(0x10);

            writer.WritePositionAt(headerStart + 0x8);
            writer.Write((uint)writer.BaseStream.Position + 4);
            WriteCollision(writer, Collision);
            writer.WritePadding(0x10);

            long decorSection = writer.BaseStream.Position;
            writer.WritePositionAt(headerStart + 0xC);
            writer.Write(Background);
            writer.Write(DecorSet);
            writer.Write(-1);
            writer.Write(-1);
            writer.Write(-1);
            writer.WritePadding(0x10);

            writer.WritePositionAt(decorSection + 0x8);
            WriteDecorLayer(writer, FLand);
            writer.WritePadding(0x10);

            writer.WritePositionAt(decorSection + 0xC);
            WriteDecorLayer(writer, MLand);
            writer.WritePadding(0x10);

            writer.WritePositionAt(decorSection + 0x10);
            WriteDecorLayer(writer, BLand);
            writer.WritePadding(0x10);

            writer.WritePositionAt(headerStart + 0x10);
            strings.Add(writer.BaseStream.Position, LightSet);
            writer.Write(-1);
            writer.Write(Unknown1);
            writer.Write(Unknown2);
            writer.Write(Unknown3);

            writer.WritePadding(0x10);

            List<string> gimmickNames = new List<string>();
            for (int i = 0; i < Gimmicks.Count; i++)
            {
                if (!gimmickNames.Contains(Gimmicks[i].Name))
                    gimmickNames.Add(Gimmicks[i].Name);
            }

            long gimmickSection = writer.BaseStream.Position;
            writer.WritePositionAt(headerStart + 0x14);
            writer.Write(-1);
            writer.Write(-1);
            writer.WritePadding(0x10);

            writer.WritePositionAt(gimmickSection);
            for (int i = 0; i < Gimmicks.Count; i++)
            {
                var gimmick = Gimmicks[i];

                writer.Write(gimmick.X);
                writer.Write(gimmick.Y);
                writer.Write(gimmickNames.IndexOf(gimmick.Name));
                writer.Write(gimmick.Unknown1);
                writer.Write(gimmick.TerrainGroup);
                writer.Write(gimmick.Unknown2);
                writer.Write(gimmick.Unknown3);
                writer.Write(gimmick.Unknown4);
                writer.Write(gimmick.Unknown5);
                writer.Write(gimmick.Unknown6);
                writer.Write(gimmick.Unknown7);
                writer.Write(gimmick.Unknown8);
                writer.Write(gimmick.Unknown9);
            }
            writer.WritePadding(0x10);

            writer.WritePositionAt(gimmickSection + 0x4);
            writer.Write(gimmickNames.Count);
            for (int i = 0; i < gimmickNames.Count; i++)
            {
                strings.Add(writer.BaseStream.Position, gimmickNames[i]);
                writer.Write(-1);
            }
            writer.WritePadding(0x10);

            writer.WritePositionAt(headerStart + 0x14);
            WriteYamlSection(writer, Bosses);
            writer.WritePadding(0x10);

            strings.WriteAll(writer);

            XData.WriteFilesize(writer);
            XData.WriteFooter(writer);
        }
    }
}
