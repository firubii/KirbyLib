using KirbyLib.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KirbyLib.Mapping
{
    /// <summary>
    /// A map file for Kirby: Planet Robobot.
    /// </summary>
    public class MapKPR : Map2D
    {
        public const uint MAGIC_NUMBER = 0x0B;

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
        /// The background BCH to load for the map.
        /// </summary>
        public string Background = "LampMount";
        /// <summary>
        /// The decoration set BCH to load for the map.
        /// </summary>
        public string DecorSet = "GrassNeo";

        /// <summary>
        /// A list of carryable items found in the map.
        /// </summary>
        public List<Yaml> CarryItems = new List<Yaml>();
        /// <summary>
        /// A list of Gimmick objects found in the map.
        /// </summary>
        public List<Yaml> Gimmicks = new List<Yaml>();
        /// <summary>
        /// A list of items found in the map.
        /// </summary>
        public List<Yaml> Items = new List<Yaml>();
        /// <summary>
        /// A list of bosses found in the map.
        /// </summary>
        public List<Yaml> Bosses = new List<Yaml>();
        /// <summary>
        /// A list of enemies found in the map.
        /// </summary>
        public List<Yaml> Enemies = new List<Yaml>();
        /// <summary>
        /// A list of enemies found in the map for Jet Armor sections.
        /// </summary>
        public List<Yaml> ShootingEnemies = new List<Yaml>();

        #region General Section

        /// <summary>
        /// The BGM ID to play in the map.<br/>
        /// References stream names in the BCSAR.
        /// </summary>
        public string BGM = "BGM_LP_TTNORMAL1";
        /// <summary>
        /// The light set to load for the map.
        /// </summary>
        public string LightSet = "GrassNeo_1";
        public uint Unknown1;
        public uint Unknown2;
        public uint Unknown3;
        public int Unknown4;
        public uint Unknown5;
        public uint Unknown6;
        public uint Unknown7;
        public uint Unknown8;
        public uint Unknown9;
        public uint Unknown10;
        public uint Unknown11;
        public uint Unknown12;
        public uint Unknown13;
        public uint Unknown14;

        #endregion

        public MapKPR()
        {
            XData.Version = new byte[] { 2, 0 };
            XData.Endianness = Endianness.Little;
        }

        public MapKPR(int width, int height)
        {
            XData.Version = new byte[] { 2, 0 };
            XData.Endianness = Endianness.Little;

            Collision = new CollisionTile[width, height];
            FLand = new DecorationTile[width, height];
            MLand = new DecorationTile[width, height];
            BLand = new DecorationTile[width, height];
            Blocks = new short[width, height];
        }

        public MapKPR(EndianBinaryReader reader)
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
            uint carryItemSection = reader.ReadUInt32();
            uint collisionSection = reader.ReadUInt32();
            uint decorSection = reader.ReadUInt32();
            uint generalSection = reader.ReadUInt32();
            uint gimmickSection = reader.ReadUInt32();
            uint itemSection = reader.ReadUInt32();
            uint bossSection = reader.ReadUInt32();
            uint enemySection = reader.ReadUInt32();
            uint shootingEnemySection = reader.ReadUInt32();

            uint headerEnd = reader.ReadUInt32();
            if (headerEnd != HEADER_END)
                throw new InvalidDataException($"Expected header to end with 0x{HEADER_END:X8}, got 0x{headerEnd:X8}");

            reader.BaseStream.Position = blockSection;
            Blocks = ReadBlocks(reader);

            reader.BaseStream.Position = carryItemSection;
            CarryItems = ReadYamlSection(reader);

            reader.BaseStream.Position = collisionSection;
            reader.BaseStream.Position = reader.ReadUInt32();
            Collision = ReadCollisionShuffled(reader);

            reader.BaseStream.Position = decorSection;
            Background = reader.ReadStringOffset();
            DecorSet = reader.ReadStringOffset();

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
            BGM = reader.ReadStringOffset();
            LightSet = reader.ReadStringOffset();
            Unknown1 = reader.ReadUInt32();
            Unknown2 = reader.ReadUInt32();
            Unknown3 = reader.ReadUInt32();
            Unknown4 = reader.ReadInt32();
            Unknown5 = reader.ReadUInt32();
            Unknown6 = reader.ReadUInt32();
            Unknown7 = reader.ReadUInt32();
            Unknown8 = reader.ReadUInt32();
            Unknown9 = reader.ReadUInt32();
            Unknown10 = reader.ReadUInt32();
            Unknown11 = reader.ReadUInt32();
            Unknown12 = reader.ReadUInt32();
            Unknown13 = reader.ReadUInt32();
            Unknown14 = reader.ReadUInt32();

            reader.BaseStream.Position = gimmickSection;
            Gimmicks = ReadYamlSection(reader);

            reader.BaseStream.Position = itemSection;
            Items = ReadYamlSection(reader);

            reader.BaseStream.Position = bossSection;
            Bosses = ReadYamlSection(reader);

            reader.BaseStream.Position = enemySection;
            Enemies = ReadYamlSection(reader);

            reader.BaseStream.Position = shootingEnemySection;
            ShootingEnemies = ReadYamlSection(reader);
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
            writer.Write(-1);
            writer.Write(-1);
            writer.Write(-1);
            writer.Write(-1);
            writer.Write(HEADER_END);
            writer.WritePadding(0x10);

            writer.WritePositionAt(headerStart + 0x4);
            WriteBlocks(writer, Blocks);

            writer.WritePositionAt(headerStart + 0x8);
            WriteYamlSection(writer, CarryItems);

            writer.WritePositionAt(headerStart + 0xC);
            writer.Write((uint)writer.BaseStream.Position + 0x4);
            WriteCollisionShuffled(writer, Collision);

            long decorSection = writer.BaseStream.Position;
            writer.WritePositionAt(headerStart + 0x10);
            strings.Add(writer.BaseStream.Position, Background);
            writer.Write(-1);
            strings.Add(writer.BaseStream.Position, DecorSet);
            writer.Write(-1);

            writer.Write(-1);
            writer.Write(-1);
            writer.Write(-1);

            writer.WritePadding(0x10);

            writer.WritePositionAt(decorSection + 0x8);
            WriteDecorLayer(writer, FLand);

            writer.WritePositionAt(decorSection + 0xC);
            WriteDecorLayer(writer, MLand);

            writer.WritePositionAt(decorSection + 0x10);
            WriteDecorLayer(writer, BLand);

            writer.WritePositionAt(headerStart + 0x14);
            strings.Add(writer.BaseStream.Position, BGM);
            writer.Write(-1);
            strings.Add(writer.BaseStream.Position, LightSet);
            writer.Write(-1);
            writer.Write(Unknown1);
            writer.Write(Unknown2);
            writer.Write(Unknown3);
            writer.Write(Unknown4);
            writer.Write(Unknown5);
            writer.Write(Unknown6);
            writer.Write(Unknown7);
            writer.Write(Unknown8);
            writer.Write(Unknown9);
            writer.Write(Unknown10);
            writer.Write(Unknown11);
            writer.Write(Unknown12);
            writer.Write(Unknown13);
            writer.Write(Unknown14);

            writer.WritePositionAt(headerStart + 0x18);
            WriteYamlSection(writer, Gimmicks);

            writer.WritePositionAt(headerStart + 0x1C);
            WriteYamlSection(writer, Items);

            writer.WritePositionAt(headerStart + 0x20);
            WriteYamlSection(writer, Bosses);

            writer.WritePositionAt(headerStart + 0x24);
            WriteYamlSection(writer, Enemies);

            writer.WritePositionAt(headerStart + 0x28);
            WriteYamlSection(writer, ShootingEnemies);

            strings.WriteAll(writer);

            XData.WriteFilesize(writer);
            XData.WriteFooter(writer);
        }
    }
}
