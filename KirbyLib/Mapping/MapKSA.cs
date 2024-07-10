using KirbyLib.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static KirbyLib.Mapping.MapRtDL;

namespace KirbyLib.Mapping
{
    /// <summary>
    /// A map file for Kirby Star Allies.
    /// </summary>
    public class MapKSA : Map2D
    {
        #region Structs

        /// <summary>
        /// Represents information about a moving collision group.<br/>
        /// Visuals are defined by setting the Group field of decoration tiles to the corresponding MoveGrid index.
        /// </summary>
        public struct MoveGrid
        {
            /// <summary>
            /// If false, this grid is not written to the level data.
            /// </summary>
            public bool IsValid;

            /// <summary>
            /// The width of the moving collision, in tiles.
            /// </summary>
            public int Width => Collision.GetLength(0);
            /// <summary>
            /// The height of the moving collision, in tiles.
            /// </summary>
            public int Height => Collision.GetLength(1);

            /// <summary>
            /// The horizontal position the terrain is located.
            /// </summary>
            public uint X;
            /// <summary>
            /// The vertical position the terrain is located.
            /// </summary>
            public uint Y;
            public CollisionTile[,] Collision;
        }

        #endregion

        public const uint MAGIC_NUMBER = 0x0C;

        public override int Width => Collision.GetLength(0);

        public override int Height => Collision.GetLength(1);

        /// <summary>
        /// Fixed collision tiles.
        /// </summary>
        public CollisionTile[,] Collision;

        /// <summary>
        /// First Decoration tile layer.
        /// </summary>
        public DecorationTile[,] DecorationLayer1;
        /// <summary>
        /// Second Decoration tile layer.
        /// </summary>
        public DecorationTile[,] DecorationLayer2;
        /// <summary>
        /// Third Decoration tile layer.
        /// </summary>
        public DecorationTile[,] DecorationLayer3;
        /// <summary>
        /// Fourth Decoration tile layer.
        /// </summary>
        public DecorationTile[,] DecorationLayer4;

        /// <summary>
        /// Interactable blocks. If -1, there is no block.
        /// </summary>
        public short[,] Blocks;

        /// <summary>
        /// Moving collision groups.<br/>There is a hardcoded limit of 16.
        /// </summary>
        public MoveGrid[] CollisionMoveGroups { get; private set; } = new MoveGrid[16];

        /// <summary>
        /// The background set to load for the map.<br/>
        /// This string is used to load a BFRES file containing a skeleton and<br/>
        /// Cinemo file that specify how and where to place background objects.
        /// </summary>
        public string Background = "Grass_Bg01";
        /// <summary>
        /// The tileset set to load for the map.
        /// </summary>
        public string Tileset = "Grass";

        /// <summary>
        /// A list of carryable items found in the map.
        /// </summary>
        public List<Yaml> CarryItems = new List<Yaml>();
        /// <summary>
        /// A list of Gimmick objects found in the map.
        /// </summary>
        public List<Yaml> Gimmicks = new List<Yaml>();
        /// <summary>
        /// A list of items found only during Guest Star in the map.
        /// </summary>
        public List<Yaml> HelperGoItems = new List<Yaml>();
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

        #region General

        public uint Unknown1;
        public uint Unknown2;
        /// <summary>
        /// The light set to load for the map.<br/>
        /// This string is used to load a BFRES file that contains cubemaps and<br/>
        /// its Cinemo file with lighting information.
        /// </summary>
        public string LightSet = "Grass_01";
        public Vector3 Unknown3;
        public Vector3 Unknown4;
        public Vector3 Unknown5;
        public float Unknown6;
        public float Unknown7;
        public float Unknown8;
        public float Unknown9;
        public float Unknown10;
        public float Unknown11;
        public float Unknown12;
        public float Unknown13;

        #endregion

        public MapKSA()
        {
            XData.Version = new byte[] { 4, 0 };
            XData.Endianness = Endianness.Little;
        }

        public MapKSA(int width, int height)
        {
            XData.Version = new byte[] { 4, 0 };
            XData.Endianness = Endianness.Little;

            Collision = new CollisionTile[width, height];
            DecorationLayer1 = new DecorationTile[width, height];
            DecorationLayer2 = new DecorationTile[width, height];
            DecorationLayer3 = new DecorationTile[width, height];
            DecorationLayer4 = new DecorationTile[width, height];
            Blocks = new short[width, height];
        }

        public MapKSA(EndianBinaryReader reader)
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
            uint movingTerrainSection = reader.ReadUInt32();
            uint decorSection = reader.ReadUInt32();
            uint generalSection = reader.ReadUInt32();
            uint gimmickSection = reader.ReadUInt32();
            uint helperGoItemSection = reader.ReadUInt32();
            uint itemSection = reader.ReadUInt32();
            uint bossSection = reader.ReadUInt32();
            uint enemySection = reader.ReadUInt32();

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

            reader.BaseStream.Position = movingTerrainSection;
            uint validTerrain = reader.ReadUInt32();
            for (int i = 0; i < 0x10; i++)
            {
                MoveGrid grid = new MoveGrid();
                grid.IsValid = false;

                // The first value in this section is a set of bit flags that tell which groups are valid, from the lowest to highest bit.
                // While not explicitly checked for, invalid groups typically have their address set to the decoration section.
                if ((validTerrain & (1 << i)) != 0)
                {
                    grid.IsValid = true;

                    reader.BaseStream.Position = movingTerrainSection + 4 + (i * 4);
                    reader.BaseStream.Position = reader.ReadUInt32();

                    grid.X = reader.ReadUInt32();
                    grid.Y = reader.ReadUInt32();

                    ushort width = reader.ReadUInt16();
                    ushort height = reader.ReadUInt16();
                    grid.Collision = new CollisionTile[width, height];

                    uint tileCount = reader.ReadUInt32();

                    uint x2 = reader.ReadUInt32();
                    if (x2 != grid.X)
                        Console.WriteLine($"Warning: X positions do not match in moving collision grid {i}! {grid.X} != {x2}");

                    uint y2 = reader.ReadUInt32();
                    if (y2 != grid.Y)
                        Console.WriteLine($"Warning: Y positions do not match in moving collision grid {i}! {grid.Y} != {y2}");

                    reader.BaseStream.Position = reader.ReadUInt32();
                    for (int t = 0; t < tileCount; t++)
                    {
                        byte x = reader.ReadByte();
                        byte y = reader.ReadByte();

                        CollisionTile tile = new CollisionTile();
                        tile.Shape = (LandGridShapeKind)reader.ReadByte();
                        tile.Material = reader.ReadByte();

                        grid.Collision[x, y] = tile;
                    }
                }

                CollisionMoveGroups[i] = grid;
            }

            reader.BaseStream.Position = decorSection;
            Background = reader.ReadStringOffset();
            Tileset = reader.ReadStringOffset();

            reader.BaseStream.Position = decorSection + 0x8;
            reader.BaseStream.Position = reader.ReadUInt32();
            DecorationLayer1 = ReadDecorLayer(reader);

            reader.BaseStream.Position = decorSection + 0xC;
            reader.BaseStream.Position = reader.ReadUInt32();
            DecorationLayer2 = ReadDecorLayer(reader);

            reader.BaseStream.Position = decorSection + 0x10;
            reader.BaseStream.Position = reader.ReadUInt32();
            DecorationLayer3 = ReadDecorLayer(reader);

            reader.BaseStream.Position = decorSection + 0x14;
            reader.BaseStream.Position = reader.ReadUInt32();
            DecorationLayer4 = ReadDecorLayer(reader);

            reader.BaseStream.Position = generalSection;
            Unknown1 = reader.ReadUInt32();
            Unknown2 = reader.ReadUInt32();
            LightSet = reader.ReadStringOffset();
            Unknown3 = reader.ReadVector3();
            Unknown4 = reader.ReadVector3();
            Unknown5 = reader.ReadVector3();
            Unknown6 = reader.ReadSingle();
            Unknown7 = reader.ReadSingle();
            Unknown8 = reader.ReadSingle();
            Unknown9 = reader.ReadSingle();
            Unknown10 = reader.ReadSingle();
            Unknown11 = reader.ReadSingle();
            Unknown12 = reader.ReadSingle();
            Unknown13 = reader.ReadSingle();

            reader.BaseStream.Position = gimmickSection;
            Gimmicks = ReadYamlSection(reader);

            reader.BaseStream.Position = helperGoItemSection;
            HelperGoItems = ReadYamlSection(reader);

            reader.BaseStream.Position = itemSection;
            Items = ReadYamlSection(reader);

            reader.BaseStream.Position = bossSection;
            Bosses = ReadYamlSection(reader);

            reader.BaseStream.Position = enemySection;
            Enemies = ReadYamlSection(reader);
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

            long movingTerrainSection = writer.BaseStream.Position;
            writer.WritePositionAt(headerStart + 0x10);
            uint validGroups = 0;
            for (int i = 0; i < CollisionMoveGroups.Length; i++)
            {
                if (CollisionMoveGroups[i].IsValid)
                    validGroups |= (uint)(1 << i);
            }

            writer.Write(validGroups);
            for (int i = 0; i < CollisionMoveGroups.Length; i++)
                writer.Write(-1);

            for (int i = 0; i < CollisionMoveGroups.Length; i++)
            {
                writer.WritePositionAt(movingTerrainSection + 4 + (i * 4));

                var group = CollisionMoveGroups[i];
                if (group.IsValid)
                {
                    writer.Write(group.X);
                    writer.Write(group.Y);

                    ushort width = (ushort)group.Width;
                    ushort height = (ushort)group.Height;
                    writer.Write(width);
                    writer.Write(height);

                    int tileCount = 0;
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            if (group.Collision[x, y].Shape != LandGridShapeKind.None)
                                tileCount++;
                        }
                    }
                    writer.Write(tileCount);

                    writer.Write(group.X);
                    writer.Write(group.Y);

                    writer.Write((uint)writer.BaseStream.Position + 4);
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            var tile = group.Collision[x, y];
                            if (tile.Shape != LandGridShapeKind.None)
                            {
                                writer.Write((byte)x);
                                writer.Write((byte)y);
                                writer.Write((byte)tile.Shape);
                                writer.Write(tile.Material);
                            }
                        }
                    }
                }
            }

            long decorSection = writer.BaseStream.Position;
            writer.WritePositionAt(headerStart + 0x14);

            strings.Add(writer.BaseStream.Position, Background);
            writer.Write(-1);

            strings.Add(writer.BaseStream.Position, Tileset);
            writer.Write(-1);

            writer.Write(-1);
            writer.Write(-1);
            writer.Write(-1);
            writer.Write(-1);
            writer.WritePadding(0x10);

            writer.WritePositionAt(decorSection + 0x8);
            WriteDecorLayer(writer, DecorationLayer1);

            writer.WritePositionAt(decorSection + 0xC);
            WriteDecorLayer(writer, DecorationLayer2);

            writer.WritePositionAt(decorSection + 0x10);
            WriteDecorLayer(writer, DecorationLayer3);

            writer.WritePositionAt(decorSection + 0x14);
            WriteDecorLayer(writer, DecorationLayer4);

            writer.WritePositionAt(headerStart + 0x18);
            writer.Write(Unknown1);
            writer.Write(Unknown2);

            strings.Add(writer.BaseStream.Position, LightSet);
            writer.Write(-1);

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

            writer.WritePositionAt(headerStart + 0x1C);
            WriteYamlSection(writer, Gimmicks);

            writer.WritePositionAt(headerStart + 0x20);
            WriteYamlSection(writer, HelperGoItems);

            writer.WritePositionAt(headerStart + 0x24);
            WriteYamlSection(writer, Items);

            writer.WritePositionAt(headerStart + 0x28);
            WriteYamlSection(writer, Bosses);

            writer.WritePositionAt(headerStart + 0x2C);
            WriteYamlSection(writer, Enemies);

            strings.WriteAll(writer);

            XData.WriteFilesize(writer);
            XData.WriteFooter(writer);
        }
    }
}
