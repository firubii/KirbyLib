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
    /// A map file for Kirby: Triple Deluxe.
    /// </summary>
    public class MapTDX : Map2D
    {
        #region Structs

        public struct CarryItem
        {
            public uint Kind;
            public uint Variation;
            public bool CanRespawn;
            public GridPos X;
            public GridPos Y;
        }

        public struct Enemy
        {
            public string Name;
            public string Variation;
            public int Param1;
            public int Param2;
            public int Param3;
            public GridPos X;
            public GridPos Y;
            public uint Unknown;
            public int TerrainGroup;
        }

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

        public struct Item
        {
            public int Kind;
            public int Variation;
            public int SubKind;
            public GridPos X;
            public GridPos Y;
            public int HideModeKind;
        }

        #endregion

        public const uint MAGIC_NUMBER = 0x24;

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
        public uint Background = 6;
        /// <summary>
        /// The decoration set ID the map will use.<br/>This controls which models to load for the tileset, background objects, and foreground objects.
        /// </summary>
        public uint DecorSet = 6;

        /// <summary>
        /// A list of carryable items found in the map.
        /// </summary>
        public List<CarryItem> CarryItems = new List<CarryItem>();
        /// <summary>
        /// A list of enemies found in the map.
        /// </summary>
        public List<Enemy> Enemies = new List<Enemy>();
        /// <summary>
        /// A list of Gimmick objects found in the map.
        /// </summary>
        public List<Gimmick> Gimmicks = new List<Gimmick>();
        /// <summary>
        /// A list of items found in the map.
        /// </summary>
        public List<Item> Items = new List<Item>();

        #region General Section

        /// <summary>
        /// The BGM ID to play in the map.<br/>
        /// References stream names in the BCSAR.
        /// </summary>
        public string BGM;
        public Vector3 Unknown1;
        public int Unknown2;
        public int Unknown3;
        public int Unknown4;
        public int Unknown5;
        public int Unknown6;
        public int Unknown7;
        public int Unknown8;
        public int Unknown9;
        public int Unknown10;
        public int Unknown11;
        public int Unknown12;
        public int Unknown13;

        #endregion

        public MapTDX()
        {
            XData.Version = new byte[] { 2, 0 };
            XData.Endianness = Endianness.Little;
        }

        public MapTDX(int width, int height)
        {
            XData.Version = new byte[] { 2, 0 };
            XData.Endianness = Endianness.Little;

            Collision = new CollisionTile[width, height];
            BLand = new DecorationTile[width, height];
            MLand = new DecorationTile[width, height];
            FLand = new DecorationTile[width, height];
            Blocks = new short[width, height];
        }

        public MapTDX(EndianBinaryReader reader)
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
            uint enemySection = reader.ReadUInt32();
            uint enemyStringSection = reader.ReadUInt32();
            uint generalSection = reader.ReadUInt32();
            uint gimmickSection = reader.ReadUInt32();
            uint itemSection = reader.ReadUInt32();

            uint headerEnd = reader.ReadUInt32();
            if (headerEnd != HEADER_END)
                throw new InvalidDataException($"Expected header to end with 0x{HEADER_END:X8}, got 0x{headerEnd:X8}");

            reader.BaseStream.Position = blockSection;
            Blocks = ReadBlocks(reader);

            reader.BaseStream.Position = carryItemSection;
            uint carryItemCount = reader.ReadUInt32();
            CarryItems = new List<CarryItem>();
            for (int i = 0; i < carryItemCount; i++)
            {
                CarryItem item = new CarryItem();
                item.Kind = reader.ReadUInt32();
                item.Variation = reader.ReadUInt32();
                item.CanRespawn = reader.ReadInt32() != 0;
                item.X = reader.ReadUInt32();
                item.Y = reader.ReadUInt32();

                CarryItems.Add(item);
            }

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

            reader.BaseStream.Position = enemyStringSection;
            uint enemyStringCount = reader.ReadUInt32();
            string[] enemyInfo = new string[enemyStringCount];
            for (int i = 0; i < enemyStringCount; i++)
            {
                enemyInfo[i] = reader.ReadStringOffset();
                //This second value is a duplicate of the variation string, we don't really need it since the exact same string is referenced in the enemy struct
                reader.BaseStream.Position += 4;
            }

            reader.BaseStream.Position = enemySection;
            uint enemyCount = reader.ReadUInt32();
            Enemies = new List<Enemy>();
            for (int i = 0; i < enemyCount; i++)
            {
                Enemy enemy = new Enemy();
                enemy.Variation = reader.ReadStringOffset();
                enemy.Param1 = reader.ReadInt32();
                enemy.Param2 = reader.ReadInt32();
                enemy.Param3 = reader.ReadInt32();

                int infoIdx = reader.ReadInt32();
                enemy.Name = enemyInfo[infoIdx];

                enemy.X = reader.ReadUInt32();
                enemy.Y = reader.ReadUInt32();
                enemy.Unknown = reader.ReadUInt32();
                enemy.TerrainGroup = reader.ReadInt32();

                Enemies.Add(enemy);
            }

            reader.BaseStream.Position = generalSection;
            BGM = reader.ReadStringOffset();
            Unknown1 = reader.ReadVector3();
            Unknown2 = reader.ReadInt32();
            Unknown3 = reader.ReadInt32();
            Unknown4 = reader.ReadInt32();
            Unknown5 = reader.ReadInt32();
            Unknown6 = reader.ReadInt32();
            Unknown7 = reader.ReadInt32();
            Unknown8 = reader.ReadInt32();
            Unknown9 = reader.ReadInt32();
            Unknown10 = reader.ReadInt32();
            Unknown11 = reader.ReadInt32();
            Unknown12 = reader.ReadInt32();
            Unknown13 = reader.ReadInt32();

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

            reader.BaseStream.Position = itemSection;
            uint itemCount = reader.ReadUInt32();
            Items = new List<Item>();
            for (int i = 0; i < itemCount; i++)
            {
                Item item = new Item();
                item.Kind = reader.ReadInt32();
                item.Variation = reader.ReadInt32();
                item.SubKind = reader.ReadInt32();
                item.X = reader.ReadUInt32();
                item.Y = reader.ReadUInt32();
                item.HideModeKind = reader.ReadInt32();

                Items.Add(item);
            }
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
            writer.Write(HEADER_END);
            writer.WritePadding(0x10);

            writer.WritePositionAt(headerStart + 0x4);
            WriteBlocks(writer, Blocks);
            writer.WritePadding(0x10);

            writer.WritePositionAt(headerStart + 0x8);
            writer.Write(CarryItems.Count);
            for (int i = 0; i < CarryItems.Count; i++)
            {
                var item = CarryItems[i];
                writer.Write(item.Kind);
                writer.Write(item.Variation);
                writer.Write(item.CanRespawn ? 1 : 0);
                writer.Write(item.X);
                writer.Write(item.Y);
            }
            writer.WritePadding(0x10);

            writer.WritePositionAt(headerStart + 0xC);
            writer.Write((uint)writer.BaseStream.Position + 4);
            WriteCollision(writer, Collision);
            writer.WritePadding(0x10);

            long decorSection = writer.BaseStream.Position;
            writer.WritePositionAt(headerStart + 0x10);
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

            List<string[]> enemyStrings = new List<string[]>();
            for (int i = 0; i < Enemies.Count; i++)
            {
                string[] strs = new string[]
                {
                    Enemies[i].Name,
                    Enemies[i].Variation
                };
                if (!enemyStrings.Contains(strs))
                    enemyStrings.Add(strs);
            }

            writer.WritePositionAt(headerStart + 0x14);
            writer.Write(Enemies.Count);
            for (int i = 0; i < Enemies.Count; i++)
            {
                var enemy = Enemies[i];

                strings.Add(writer.BaseStream.Position, enemy.Variation);
                writer.Write(-1);
                writer.Write(enemy.Param1);
                writer.Write(enemy.Param2);
                writer.Write(enemy.Param3);

                writer.Write(enemyStrings.FindIndex(x => x[0] == enemy.Name));
                writer.Write(enemy.X);
                writer.Write(enemy.Y);
                writer.Write(enemy.Unknown);
                writer.Write(enemy.TerrainGroup);
            }
            writer.WritePadding(0x10);

            writer.WritePositionAt(headerStart + 0x18);
            writer.Write(enemyStrings.Count);
            for (int i = 0; i < enemyStrings.Count; i++)
            {
                strings.Add(writer.BaseStream.Position, enemyStrings[i][0]);
                writer.Write(-1);
                strings.Add(writer.BaseStream.Position, enemyStrings[i][1]);
                writer.Write(-1);
            }
            writer.WritePadding(0x10);

            writer.WritePositionAt(headerStart + 0x1C);
            strings.Add(writer.BaseStream.Position, BGM);
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

            List<string> gimmickNames = new List<string>();
            for (int i = 0; i < Gimmicks.Count; i++)
            {
                if (!gimmickNames.Contains(Gimmicks[i].Name))
                    gimmickNames.Add(Gimmicks[i].Name);
            }

            long gimmickSection = writer.BaseStream.Position;
            writer.WritePositionAt(headerStart + 0x20);
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

            writer.WritePositionAt(headerStart + 0x24);
            writer.Write(Items.Count);
            for (int i = 0; i < Items.Count; i++)
            {
                var item = Items[i];

                writer.Write(item.Kind);
                writer.Write(item.Variation);
                writer.Write(item.SubKind);
                writer.Write(item.X);
                writer.Write(item.Y);
                writer.Write(item.HideModeKind);
            }

            strings.WriteAll(writer);

            XData.WriteFilesize(writer);
            XData.WriteFooter(writer);
        }
    }
}
