using KirbyLib.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace KirbyLib.Mapping
{
    /// <summary>
    /// A map file for the following games:
    /// <list type="bullet">
    ///     <item>Kirby's Return to Dream Land</item>
    ///     <item>Kirby's Dream Collection</item>
    ///     <item>Kirby's Return to Dream Land Deluxe<br/><b>Note:</b> XData must be version 5.0 and Little Endian</item>
    /// </list>
    /// </summary>
    public class MapRtDL : Map2D
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
            public ushort X;
            /// <summary>
            /// The vertical position the terrain is located.
            /// </summary>
            public ushort Y;
            public CollisionTile[,] Collision;

            /// <summary>
            /// The action tied to this MoveGrid.<br/>Defines the movement it will take.
            /// </summary>
            public MoveGridAction Action;
        }

        public struct MoveGridAction
        {
            /// <summary>
            /// If false, this action is not written to the level data.
            /// </summary>
            public bool IsValid;

            /// <summary>
            /// The signal event this action is tied to and will activate when receiving the ID.<br/>
            /// Used for tying terrain movement to switches.<br/>
            /// If set to -1, this action cannot be triggered by a signal.
            /// </summary>
            public sbyte Event;
            public byte Param1;
            public byte Param2;
            /// <summary>
            /// If true, this action will start the moment the MoveGrid is loaded.
            /// </summary>
            public bool StartImmediately;

            public List<MoveGridEvent> Events;

            public MoveGridAction()
            {
                IsValid = false;
                Event = -1;
                Param1 = 0;
                Param2 = 0;
                StartImmediately = false;
                Events = new List<MoveGridEvent>();
            }
        }

        public struct MoveGridEvent
        {
            public MoveGridDirection Direction;
            public byte Distance;
            public ushort Delay;
            public short DXUnknown1;

            public short Unknown1;

            public short DXUnknown2;

            public ushort Time;

            public short DXUnknown3;

            public bool IsEnd;
            public sbyte Unknown2;

            public byte Unknown3;
            public byte Unknown4;
            public byte Unknown5;
            public byte Unknown6;
            public byte Unknown7;
            public byte Unknown8;
            public byte Unknown9;
            public byte Unknown10;
            public byte Unknown11;
            public byte Unknown12;

            public byte AccelType;
            public byte AccelTime;
            public ushort Unknown13;

            public MoveGridEvent()
            {
                Direction = MoveGridDirection.Up;
                Distance = 0;
                Delay = 0;

                DXUnknown1 = -1;

                Unknown1 = 0;

                DXUnknown2 = -1;

                Time = 0;

                DXUnknown3 = -1;

                IsEnd = false;
                Unknown2 = 0;

                Unknown3 = 0;
                Unknown4 = 0;
                Unknown5 = 0;
                Unknown6 = 0;
                Unknown7 = 0;
                Unknown8 = 0;
                Unknown9 = 0;
                Unknown10 = 0;
                Unknown11 = 0;
                Unknown12 = 0;

                AccelType = 0;
                AccelTime = 0;
                Unknown13 = 0;
            }
        }

        public enum MoveGridDirection : uint
        {
            Up,
            Right,
            Down,
            Left,
            None
        }

        public struct Boss
        {
            public uint Kind;
            public uint SubKind;
            public uint Level;
            public int TerrainGroup;
            public bool HasSuperAbility;
            public GridPos X;
            public GridPos Y;
            public uint Unknown;
        }

        public struct CarryItem
        {
            public uint Kind;
            public uint AppearGroup;
            public bool CanRespawn;
            public int TerrainGroup;
            public GridPos X;
            public GridPos Y;
        }

        public struct Enemy
        {
            public uint Kind;
            public uint Variation;
            public uint Level;
            public uint Direction;
            public uint AnotherDimensionSize;
            public uint ExtraModeSize;
            public int TerrainGroup;
            public bool HasSuperAbility;
            public GridPos X;
            public GridPos Y;
        }

        public struct Gimmick
        {
            public uint Kind;
            public GridPos X;
            public GridPos Y;
            public uint Param1;
            public uint Param2;
            public uint Param3;
            public uint Param4;
            public uint Param5;
            public uint Param6;
            public uint Param7;
            public uint Param8;
            public uint Param9;
            public uint Param10;
            public uint Param11;
            public uint Param12;
            public uint Unknown;
        }

        public struct Item
        {
            public uint Kind;
            public uint Variation;
            public uint Level;
            public int TerrainGroup;
            public GridPos X;
            public GridPos Y;
        }

        public enum BinDecoObjectKind : uint
        {
            AreaLightDirectional,
            AreaLightPoint,
            AreaLightAmbient
        }

        public struct DecorObject
        {
            public BinDecoObjectKind Kind;
            public GridPos X1;
            public GridPos Y1;
            public GridPos X2;
            public GridPos Y2;
            public GridPos X3;
            public GridPos Y3;
            public uint Unknown;
            public int TerrainGroup;
            public uint Param1;
            public uint Param2;
            public uint Param3;
            public uint Param4;
            public uint Param5;
            public uint Param6;
            public uint Param7;
            public uint Param8;
        }

        public enum SFXKind : uint
        {
            None,
            Darkness,
            Monotone
        }

        public enum MapType : uint
        {
            Normal,
            /// <summary>
            /// Disallows Mid-bosses from locking the camera and spawning health bars
            /// </summary>
            MidbossRush,
            Shooting,
            /// <summary>
            /// The most complicated map type<br/>
            /// <list type="bullet">
            ///     <item>Doubles the amount of memory a Boss's Heap uses (0x20000 -> 0x40000 bytes)</item>
            ///     <item>Limits the amount of loaded Bosses from 4 to 1</item>
            ///     <item>Increases the amount of memory a Weapon's Heap uses (0x4000 -> 0x4C00 bytes)</item>
            ///     <item>Limits the amount of loaded Weapons from 96 to 80</item>
            ///     <item>Enables the swirling ground particle effects from the final battle</item>
            /// </list>
            /// </summary>
            FinalBattle,
            LevelMap
        }

        #endregion

        public const uint MAGIC_NUMBER = 0x22;

        public override int Width => Collision[0].GetLength(0);

        public override int Height => Collision[0].GetLength(1);

        /// <summary>
        /// Each layer of fixed collision. Multiple collision layers can be stored in a map, but only 1 is ever used.
        /// </summary>
        public List<CollisionTile[,]> Collision = new List<CollisionTile[,]>();
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
        /// Decoration objects that define light sources.
        /// </summary>
        public List<DecorObject> DecorationObjects = new List<DecorObject>();

        /// <summary>
        /// Interactable blocks. If -1, there is no block.
        /// </summary>
        public short[,] Blocks;

        /// <summary>
        /// Moving collision groups.<br/>There is a hardcoded limit of 16.
        /// </summary>
        public MoveGrid[] CollisionMoveGroups { get; private set; } = new MoveGrid[16];

        /// <summary>
        /// The background ID the map will use.
        /// </summary>
        public uint Background = 6;
        /// <summary>
        /// The decoration set ID the map will use.<br/>This controls which models to load for the tileset, background objects, and foreground objects.
        /// </summary>
        public uint DecorSet = 6;

        public List<Boss> Bosses = new List<Boss>();
        public List<CarryItem> CarryItems = new List<CarryItem>();
        public List<Enemy> Enemies = new List<Enemy>();
        public List<Gimmick> Gimmicks = new List<Gimmick>();
        public List<Item> Items = new List<Item>();

        #region General Section

        /// <summary>
        /// The BGM ID to play in the map.<br/>
        /// References stream names in the BRSAR.
        /// </summary>
        public string BGM = "BGM_LP_PLANTS1";
        /// <summary>
        /// Defines the starting position for the background camera.
        /// </summary>
        public Vector3 BGCameraPos = new Vector3(33f, 34.5f, 106.48f);
        /// <summary>
        /// Defines the rotation for the background camera.
        /// </summary>
        public Vector3 BGCameraRot = new Vector3(-1.8f, 3f, 0f);
        /// <summary>
        /// Defines the vertical field of view (FOV) for the background camera.
        /// </summary>
        public float BGCameraFOV = 15f;
        /// <summary>
        /// Defines how the background camera moves when moving horizontally.
        /// </summary>
        public Vector3 BGCameraMoveRateH = new Vector3(15f, 0.5f, 0f);
        /// <summary>
        /// Defines how the background camera moves when moving vertically.
        /// </summary>
        public Vector3 BGCameraMoveRateV = new Vector3(-0.6f, 0f, 1f);
        /// <summary>
        /// The screen effect to use for the map.
        /// </summary>
        public SFXKind SFX = SFXKind.None;
        /// <summary>
        /// Determines special handling of certain things, notably Bosses.
        /// </summary>
        public MapType Type = MapType.Normal;
        /// <summary>
        /// If set, RespawnStartPortal and RespawnStepShift are used to determine where to place Kirby after dying.
        /// </summary>
        public bool CustomRespawn = false;
        /// <summary>
        /// How many rooms to move Kirby after dying.
        /// </summary>
        public int RespawnStepShift = 0;
        /// <summary>
        /// The StartPortal ID to place Kirby at after dying.
        /// </summary>
        public int RespawnStartPortal = 0;

        #endregion

        public MapRtDL()
        {
            XData.Version = new byte[] { 2, 0 };
            XData.Endianness = Endianness.Big;
        }

        public MapRtDL(int width, int height)
        {
            XData.Version = new byte[] { 2, 0 };
            XData.Endianness = Endianness.Big;

            Collision = new List<CollisionTile[,]>()
                {
                    new CollisionTile[width, height]
                };
            BLand = new DecorationTile[width, height];
            MLand = new DecorationTile[width, height];
            FLand = new DecorationTile[width, height];
            Blocks = new short[width, height];
        }

        public MapRtDL(EndianBinaryReader reader)
        {
            Read(reader);
        }

        public override void Read(EndianBinaryReader reader)
        {
            XData.Read(reader);

            uint magic = reader.ReadUInt32();
            if (magic != MAGIC_NUMBER)
                throw new InvalidDataException($"Expected magic {MAGIC_NUMBER}, got {magic}");

            uint bossSection = reader.ReadUInt32();
            uint carryItemSection = reader.ReadUInt32();
            uint collisionSection = reader.ReadUInt32();
            uint movingTerrainSection = reader.ReadUInt32();
            uint decorSection = reader.ReadUInt32();
            uint enemySection = reader.ReadUInt32();
            uint generalSection = reader.ReadUInt32();
            uint gimmickSection = reader.ReadUInt32();
            uint itemSection = reader.ReadUInt32();

            uint headerEnd = reader.ReadUInt32();
            if (headerEnd != HEADER_END)
                throw new InvalidDataException($"Expected header to end with 0x{HEADER_END:X8}, got 0x{headerEnd:X8}");

            reader.BaseStream.Position = bossSection;
            uint bossCount = reader.ReadUInt32();
            Bosses = new List<Boss>();
            for (int i = 0; i < bossCount; i++)
            {
                Boss boss = new Boss();
                boss.Kind = reader.ReadUInt32();
                boss.SubKind = reader.ReadUInt32();
                boss.Level = reader.ReadUInt32();
                boss.TerrainGroup = reader.ReadInt32();
                boss.HasSuperAbility = reader.ReadInt32() != 0;
                boss.X = reader.ReadInt32();
                boss.Y = reader.ReadInt32();
                boss.Unknown = reader.ReadUInt32();

                Bosses.Add(boss);
            }

            reader.BaseStream.Position = carryItemSection;
            uint carryItemCount = reader.ReadUInt32();
            CarryItems = new List<CarryItem>();
            for (int i = 0; i < carryItemCount; i++)
            {
                CarryItem item = new CarryItem();
                item.Kind = reader.ReadUInt32();
                item.AppearGroup = reader.ReadUInt32();
                item.CanRespawn = reader.ReadInt32() != 0;
                item.TerrainGroup = reader.ReadInt32();
                item.X = reader.ReadInt32();
                item.Y = reader.ReadInt32();

                CarryItems.Add(item);
            }

            reader.BaseStream.Position = collisionSection;
            uint collCount = reader.ReadUInt32();
            Collision = new List<CollisionTile[,]>();
            for (int i = 0; i < collCount; i++)
            {
                reader.BaseStream.Position = collisionSection + 4 + (i * 4);
                reader.BaseStream.Position = reader.ReadUInt32();

                Collision.Add(ReadCollision(reader));
            }

            reader.BaseStream.Position = movingTerrainSection;
            uint validTerrain = reader.ReadUInt32();
            // There are only ever 16 collision move groups at any given time. This is a hardcoded limitation.
            CollisionMoveGroups = new MoveGrid[0x10];
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

                    grid.X = reader.ReadUInt16();
                    grid.Y = reader.ReadUInt16();

                    ushort width = reader.ReadUInt16();
                    ushort height = reader.ReadUInt16();
                    grid.Collision = new CollisionTile[width, height];

                    uint tileCount = reader.ReadUInt32();

                    uint x2 = reader.ReadUInt32();
                    if (x2 != grid.X)
                        Console.WriteLine($"Warning: 16 and 32 bit X positions do not match in moving collision grid {i}! {grid.X} != {x2}");

                    uint y2 = reader.ReadUInt32();
                    if (y2 != grid.Y)
                        Console.WriteLine($"Warning: 16 and 32 bit Y positions do not match in moving collision grid {i}! {grid.Y} != {y2}");

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

            reader.BaseStream.Position = decorSection + 0x14;
            reader.BaseStream.Position = reader.ReadUInt32();
            uint lightCount = reader.ReadUInt32();
            DecorationObjects = new List<DecorObject>();
            for (int i = 0; i < lightCount; i++)
            {
                DecorObject obj = new DecorObject();
                obj.Kind = (BinDecoObjectKind)reader.ReadUInt32();
                obj.X1 = reader.ReadUInt32();
                obj.Y1 = reader.ReadUInt32();
                obj.X2 = reader.ReadUInt32();
                obj.Y2 = reader.ReadUInt32();
                obj.X3 = reader.ReadUInt32();
                obj.Y3 = reader.ReadUInt32();
                obj.Unknown = reader.ReadUInt32();
                obj.TerrainGroup = reader.ReadInt32();
                obj.Param1 = reader.ReadUInt32();
                obj.Param2 = reader.ReadUInt32();
                obj.Param3 = reader.ReadUInt32();
                obj.Param4 = reader.ReadUInt32();
                obj.Param5 = reader.ReadUInt32();
                obj.Param6 = reader.ReadUInt32();
                obj.Param7 = reader.ReadUInt32();
                obj.Param8 = reader.ReadUInt32();

                DecorationObjects.Add(obj);
            }

            reader.BaseStream.Position = enemySection;
            uint enemyCount = reader.ReadUInt32();
            Enemies = new List<Enemy>();
            for (int i = 0; i < enemyCount; i++)
            {
                Enemy enemy = new Enemy();
                enemy.Kind = reader.ReadUInt32();
                enemy.Variation = reader.ReadUInt32();
                enemy.Level = reader.ReadUInt32();
                enemy.Direction = reader.ReadUInt32();
                enemy.AnotherDimensionSize = reader.ReadUInt32();
                enemy.ExtraModeSize = reader.ReadUInt32();
                enemy.TerrainGroup = reader.ReadInt32();
                enemy.HasSuperAbility = reader.ReadInt32() != 0;
                enemy.X = reader.ReadUInt32();
                enemy.Y = reader.ReadUInt32();

                Enemies.Add(enemy);
            }

            reader.BaseStream.Position = generalSection;
            BGM = reader.ReadStringOffset();
            BGCameraPos = reader.ReadVector3();
            BGCameraRot = reader.ReadVector3();
            BGCameraFOV = reader.ReadSingle();
            BGCameraMoveRateH = reader.ReadVector3();
            BGCameraMoveRateV = reader.ReadVector3();
            SFX = (SFXKind)reader.ReadUInt32();
            Type = (MapType)reader.ReadUInt32();
            CustomRespawn = reader.ReadUInt32() != 0;
            RespawnStepShift = reader.ReadInt32();
            RespawnStartPortal = reader.ReadInt32();

            reader.BaseStream.Position = gimmickSection;
            reader.BaseStream.Position = reader.ReadUInt32();
            Blocks = ReadBlocks(reader);

            reader.BaseStream.Position = gimmickSection + 0x4;
            reader.BaseStream.Position = reader.ReadUInt32();
            uint gimmickCount = reader.ReadUInt32();
            Gimmicks = new List<Gimmick>();
            for (int i = 0; i < gimmickCount; i++)
            {
                Gimmick gimmick = new Gimmick();
                gimmick.Kind = reader.ReadUInt32();
                gimmick.X = reader.ReadUInt32();
                gimmick.Y = reader.ReadUInt32();
                gimmick.Param1 = reader.ReadUInt32();
                gimmick.Param2 = reader.ReadUInt32();
                gimmick.Param3 = reader.ReadUInt32();
                gimmick.Param4 = reader.ReadUInt32();
                gimmick.Param5 = reader.ReadUInt32();
                gimmick.Param6 = reader.ReadUInt32();
                gimmick.Param7 = reader.ReadUInt32();
                gimmick.Param8 = reader.ReadUInt32();
                gimmick.Param9 = reader.ReadUInt32();
                gimmick.Param10 = reader.ReadUInt32();
                gimmick.Param11 = reader.ReadUInt32();
                gimmick.Param12 = reader.ReadUInt32();
                gimmick.Unknown = reader.ReadUInt32();

                Gimmicks.Add(gimmick);
            }

            reader.BaseStream.Position = gimmickSection + 0x8;
            uint actionSection = reader.ReadUInt32();
            reader.BaseStream.Position = actionSection;
            uint validMovement = reader.ReadUInt32();
            for (int i = 0; i < 0x10; i++)
            {
                reader.BaseStream.Position = actionSection + 4 + (i * 4);
                reader.BaseStream.Position = reader.ReadUInt32();

                MoveGridAction action = new MoveGridAction();
                action.IsValid = false;

                if ((validMovement & (1 << i)) != 0)
                    action.IsValid = true;

                action.Event = reader.ReadSByte();
                action.Param1 = reader.ReadByte();
                action.Param2 = reader.ReadByte();

                byte eventCount = reader.ReadByte();
                action.Events = new List<MoveGridEvent>();

                action.StartImmediately = reader.ReadUInt32() != 0;
                
                reader.BaseStream.Position = reader.ReadUInt32();
                for (int e = 0; e < eventCount; e++)
                {
                    MoveGridEvent gEvent = new MoveGridEvent();
                    gEvent.Direction = (MoveGridDirection)reader.ReadByte();
                    gEvent.Distance = reader.ReadByte();
                    gEvent.Delay = reader.ReadUInt16();

                    if (XData.Version[0] == 5)
                        gEvent.DXUnknown1 = reader.ReadInt16();

                    gEvent.Unknown1 = reader.ReadInt16();

                    if (XData.Version[0] == 5)
                        gEvent.DXUnknown2 = reader.ReadInt16();

                    gEvent.Time = reader.ReadUInt16();

                    if (XData.Version[0] == 5)
                        gEvent.DXUnknown3 = reader.ReadInt16();

                    gEvent.IsEnd = reader.ReadByte() != 0;
                    gEvent.Unknown2 = reader.ReadSByte();

                    gEvent.Unknown3 = reader.ReadByte();
                    gEvent.Unknown4 = reader.ReadByte();
                    gEvent.Unknown5 = reader.ReadByte();
                    gEvent.Unknown6 = reader.ReadByte();
                    gEvent.Unknown7 = reader.ReadByte();
                    gEvent.Unknown8 = reader.ReadByte();
                    gEvent.Unknown9 = reader.ReadByte();
                    gEvent.Unknown10 = reader.ReadByte();
                    gEvent.Unknown11 = reader.ReadByte();
                    gEvent.Unknown12 = reader.ReadByte();

                    gEvent.AccelType = reader.ReadByte();
                    gEvent.AccelTime = reader.ReadByte();
                    gEvent.Unknown13 = reader.ReadUInt16();

                    action.Events.Add(gEvent);
                }

                CollisionMoveGroups[i].Action = action;
            }

            reader.BaseStream.Position = itemSection;
            uint itemCount = reader.ReadUInt32();
            Items = new List<Item>();
            for (int i = 0; i < itemCount; i++)
            {
                Item item = new Item();
                item.Kind = reader.ReadUInt32();
                item.Variation = reader.ReadUInt32();
                item.Level = reader.ReadUInt32();
                item.TerrainGroup = reader.ReadInt32();
                item.X = reader.ReadUInt32();
                item.Y = reader.ReadUInt32();

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
            writer.Write(Bosses.Count);
            for (int i = 0; i < Bosses.Count; i++)
            {
                var boss = Bosses[i];
                writer.Write(boss.Kind);
                writer.Write(boss.SubKind);
                writer.Write(boss.Level);
                writer.Write(boss.TerrainGroup);
                writer.Write(boss.HasSuperAbility ? 1 : 0);
                writer.Write(boss.X);
                writer.Write(boss.Y);
                writer.Write(boss.Unknown);
            }

            writer.WritePositionAt(headerStart + 0x8);
            writer.Write(CarryItems.Count);
            for (int i = 0; i < CarryItems.Count; i++)
            {
                var item = CarryItems[i];
                writer.Write(item.Kind);
                writer.Write(item.AppearGroup);
                writer.Write(item.CanRespawn ? 1 : 0);
                writer.Write(item.TerrainGroup);
                writer.Write(item.X);
                writer.Write(item.Y);
            }

            uint collisionSection = (uint)writer.BaseStream.Position;
            writer.WritePositionAt(headerStart + 0xC);
            writer.Write(Collision.Count);
            for (int i = 0; i < Collision.Count; i++)
                writer.Write(-1);

            for (int i = 0; i < Collision.Count; i++)
            {
                writer.WritePositionAt(collisionSection + 4 + (i * 4));
                WriteCollision(writer, Collision[i]);
            }

            uint movingTerrainSection = (uint)writer.BaseStream.Position;
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

                    writer.Write((int)group.X);
                    writer.Write((int)group.Y);

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

            uint decorSection = (uint)writer.BaseStream.Position;
            writer.WritePositionAt(headerStart + 0x14);
            writer.Write(Background);
            writer.Write(DecorSet);
            writer.Write(-1);
            writer.Write(-1);
            writer.Write(-1);
            writer.Write(-1);

            writer.WritePositionAt(decorSection + 0x8);
            WriteDecorLayer(writer, FLand);

            writer.WritePositionAt(decorSection + 0xC);
            WriteDecorLayer(writer, MLand);

            writer.WritePositionAt(decorSection + 0x10);
            WriteDecorLayer(writer, BLand);

            writer.WritePositionAt(decorSection + 0x14);
            writer.Write(DecorationObjects.Count);
            for (int i = 0; i < DecorationObjects.Count; i++)
            {
                var obj = DecorationObjects[i];
                writer.Write((uint)obj.Kind);
                writer.Write(obj.X1);
                writer.Write(obj.Y1);
                writer.Write(obj.X2);
                writer.Write(obj.Y2);
                writer.Write(obj.X3);
                writer.Write(obj.Y3);
                writer.Write(obj.Unknown);
                writer.Write(obj.TerrainGroup);
                writer.Write(obj.Param1);
                writer.Write(obj.Param2);
                writer.Write(obj.Param3);
                writer.Write(obj.Param4);
                writer.Write(obj.Param5);
                writer.Write(obj.Param6);
                writer.Write(obj.Param7);
                writer.Write(obj.Param8);
            }

            writer.WritePositionAt(headerStart + 0x18);
            writer.Write(Enemies.Count);
            for (int i = 0; i < Enemies.Count; i++)
            {
                var enemy = Enemies[i];
                writer.Write(enemy.Kind);
                writer.Write(enemy.Variation);
                writer.Write(enemy.Level);
                writer.Write(enemy.Direction);
                writer.Write(enemy.AnotherDimensionSize);
                writer.Write(enemy.ExtraModeSize);
                writer.Write(enemy.TerrainGroup);
                writer.Write(enemy.HasSuperAbility ? 1 : 0);
                writer.Write(enemy.X);
                writer.Write(enemy.Y);
            }

            writer.WritePositionAt(headerStart + 0x1C);
            strings.Add(writer.BaseStream.Position, BGM);
            writer.Write(-1);
            writer.Write(BGCameraPos);
            writer.Write(BGCameraRot);
            writer.Write(BGCameraFOV);
            writer.Write(BGCameraMoveRateH);
            writer.Write(BGCameraMoveRateV);
            writer.Write((uint)SFX);
            writer.Write((uint)Type);
            writer.Write(CustomRespawn ? 1 : 0);
            writer.Write(RespawnStepShift);
            writer.Write(RespawnStartPortal);

            uint gimmickSection = (uint)writer.BaseStream.Position;
            writer.WritePositionAt(headerStart + 0x20);
            writer.Write(-1);
            writer.Write(-1);
            writer.Write(-1);

            writer.WritePositionAt(gimmickSection);
            WriteBlocks(writer, Blocks);

            writer.WritePositionAt(gimmickSection + 0x4);
            writer.Write(Gimmicks.Count);
            for (int i = 0; i < Gimmicks.Count; i++)
            {
                var gimmick = Gimmicks[i];
                writer.Write(gimmick.Kind);
                writer.Write(gimmick.X);
                writer.Write(gimmick.Y);
                writer.Write(gimmick.Param1);
                writer.Write(gimmick.Param2);
                writer.Write(gimmick.Param3);
                writer.Write(gimmick.Param4);
                writer.Write(gimmick.Param5);
                writer.Write(gimmick.Param6);
                writer.Write(gimmick.Param7);
                writer.Write(gimmick.Param8);
                writer.Write(gimmick.Param9);
                writer.Write(gimmick.Param10);
                writer.Write(gimmick.Param11);
                writer.Write(gimmick.Param12);
                writer.Write(gimmick.Unknown);
            }

            uint terrainActions = (uint)writer.BaseStream.Position;
            writer.WritePositionAt(gimmickSection + 0x8);
            uint validActions = 0;
            for (int i = 0; i < CollisionMoveGroups.Length; i++)
            {
                if (CollisionMoveGroups[i].Action.IsValid)
                    validActions |= (uint)(1 << i);
            }

            writer.Write(validGroups);
            for (int i = 0; i < CollisionMoveGroups.Length; i++)
                writer.Write(-1);

            for (int i = 0; i < CollisionMoveGroups.Length; i++)
            {
                writer.WritePositionAt(terrainActions + 4 + (i * 4));

                var action = CollisionMoveGroups[i].Action;
                writer.Write(action.Event);
                writer.Write(action.Param1);
                writer.Write(action.Param2);
                writer.Write((byte)action.Events.Count);
                writer.Write(action.StartImmediately ? 1 : 0);

                writer.Write((uint)writer.BaseStream.Position + 4);

                for (int e = 0; e < action.Events.Count; e++)
                {
                    var aEvent = action.Events[e];
                    writer.Write((byte)aEvent.Direction);
                    writer.Write(aEvent.Distance);
                    writer.Write(aEvent.Delay);

                    if (XData.Version[0] == 5)
                        writer.Write(aEvent.DXUnknown1);

                    writer.Write(aEvent.Unknown1);

                    if (XData.Version[0] == 5)
                        writer.Write(aEvent.DXUnknown2);

                    writer.Write(aEvent.Time);

                    if (XData.Version[0] == 5)
                        writer.Write(aEvent.DXUnknown3);

                    writer.Write(aEvent.IsEnd ? (byte)1 : (byte)0);
                    writer.Write(aEvent.Unknown2);

                    writer.Write(aEvent.Unknown3);
                    writer.Write(aEvent.Unknown4);
                    writer.Write(aEvent.Unknown5);
                    writer.Write(aEvent.Unknown6);
                    writer.Write(aEvent.Unknown7);
                    writer.Write(aEvent.Unknown8);
                    writer.Write(aEvent.Unknown9);
                    writer.Write(aEvent.Unknown10);
                    writer.Write(aEvent.Unknown11);
                    writer.Write(aEvent.Unknown12);

                    writer.Write(aEvent.AccelType);
                    writer.Write(aEvent.AccelTime);

                    writer.Write(aEvent.Unknown13);
                }
            }

            writer.WritePositionAt(headerStart + 0x24);
            writer.Write(Items.Count);
            for (int i = 0; i < Items.Count; i++)
            {
                var item = Items[i];
                writer.Write(item.Kind);
                writer.Write(item.Variation);
                writer.Write(item.Level);
                writer.Write(item.TerrainGroup);
                writer.Write(item.X);
                writer.Write(item.Y);
            }

            strings.WriteAll(writer);

            XData.WriteFilesize(writer);
            XData.WriteFooter(writer);
        }
    }
}
