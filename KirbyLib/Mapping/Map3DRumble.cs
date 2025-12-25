using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using KirbyLib.IO;

namespace KirbyLib.Mapping
{
    /// <summary>
    /// A map file for the Sub-Game Kirby 3D Rumble found with Kirby: Planet Robobot.
    /// </summary>
    public class Map3DRumble
    {
        #region Enums

        public enum BinFoodKind : uint
        {
            DinnerCurry,
            DinnerOmelet,
            UNUSED_0,
            DrinkLemonJuice,
            UNUSED_1,
            FruitBanana,
            UNUSED_2,
            UNUSED_3,
            UNUSED_4,
            FruitWatermelon,
            UNUSED_5,
            JunkHumberger,
            UNUSED_6,
            UNUSED_7,
            JunkPotato,
            UNUSED_8,
            SweetsCupCake,
            SweetsHotCake,
            UNUSED_9,
            UNUSED_10,
            SweetsParfait,
            UNUSED_11,
            SweetsShortCake,
            UNUSED_12,
            UNUSED_13,
            UNUSED_14
        }

        public enum BinItemKind : uint
        {
            PointCoinG,
            PointCoinS,
            PointCoinB,
            Food
        }

        public enum ObjKind : uint
        {
            Kirby,
            Waddledee,
            Brontoburt,
            Broomhatter,
            Bouncy,
            SpearWaddledee,
            Grizzo,
            Masher,
            UNUSED_0,
            Kabu,
            Scarfy
        }

        public enum ObjType : uint
        {
            Wait,
            RoundTrip,
            Around,
            Circle,
            Tackle,
            Warp,
            WarpAttack,
            SleepWait,
            HappyWait,
            AngryAppear
        }

        #endregion

        #region Structs

        public struct Gimmick
        {
            public uint Kind;
            public uint Unknown0x4;
            public uint Unknown0x8;
            public float Angle;
            public Vector3 Position;
        }

        /// <summary>
        /// The starting position for Kirby.
        /// </summary>
        public struct StartPortal
        {
            public uint Unknown0x0;
            public uint Unknown0x4;
            public uint Unknown0x8;
            public float Angle;
            public Vector3 Position;
        }

        /// <summary>
        /// A Warp Star that appears after all the wave of enemies have been cleared. Unused in Blowout Blast.
        /// </summary>
        public struct WarpStar
        {
            public uint Variation;
            public Vector3 Position;
        }

        /// <summary>
        /// Represents a position in the 3D grid.
        /// </summary>
        public struct GridPos3D
        {
            public uint X;
            public uint Y;
            public uint Z;
        }

        /// <summary>
        /// A destructible block.
        /// </summary>
        public struct Block
        {
            public uint Kind;
            public GridPos3D GridPosition;
        }

        /// <summary>
        /// An item that can be picked up by running into it.
        /// </summary>
        public struct Item
        {
            public BinItemKind Kind;
            public uint Variation;
            public Vector3 Position;
        }

        /// <summary>
        /// An in-game object such as an enemy.
        /// </summary>
        public struct Enemy
        {
            public ObjKind Kind;
            public ObjType Type;
            public int Level;
            public float Angle;
            public int Pursuit;
            public List<Vector3> Markers;
        }

        #endregion

        public const uint MAGIC_NUMBER = 0x2;
        public const uint HEADER_END = 0x12345678;

        public const int ENEMY_MARKER_COUNT = 7;

        public XData XData { get; protected set; } = new XData();

        public Map3DCollision MapCollision = new Map3DCollision();
        public List<Gimmick> Gimmicks = new List<Gimmick>();
        public List<StartPortal> StartPortals = new List<StartPortal>();
        public List<WarpStar> WarpStars = new List<WarpStar>();
        public List<Block> Blocks = new List<Block>();
        public List<Item> Items = new List<Item>();
        public List<List<Enemy>> EnemyGroups = new List<List<Enemy>>();
        
        public Vector2 BlockSize = Vector2.One;
        public float OffsX = 1f;
        public float OffsZ = 1f;

        public Map3DRumble()
        {
            XData.Version = new byte[2] { 2, 0 };
            XData.Endianness = Endianness.Little;
        }

        public Map3DRumble(EndianBinaryReader reader)
        {
            Read(reader);
        }

        public void Read(EndianBinaryReader reader)
        {
            XData.Read(reader);

            uint magic = reader.ReadUInt32();
            if (magic != MAGIC_NUMBER)
                throw new InvalidDataException($"Expected magic {MAGIC_NUMBER}, got {magic}");

            uint stageSettingsSection = reader.ReadUInt32();
            uint vertexTableCount = reader.ReadUInt32();
            uint vertexTableSection = reader.ReadUInt32();
            uint collisionQuadCount = reader.ReadUInt32();
            uint collisionQuadsSection = reader.ReadUInt32();
            uint gimmickCount = reader.ReadUInt32();
            uint gimmickSection = reader.ReadUInt32();
            uint startPortalCount = reader.ReadUInt32();
            uint startPortalSection = reader.ReadUInt32();
            uint blockCount = reader.ReadUInt32();
            uint blockSection = reader.ReadUInt32();
            uint warpStarCount = reader.ReadUInt32();
            uint warpStarSection = reader.ReadUInt32();
            uint itemCount = reader.ReadUInt32();
            uint itemSection = reader.ReadUInt32();

            long enemyGroupHeader = reader.BaseStream.Position;
            uint enemyGroupCount = reader.ReadUInt32();
            reader.BaseStream.Position += enemyGroupCount * 8;

            uint headerEnd = reader.ReadUInt32();
            if (headerEnd != HEADER_END)
                throw new InvalidDataException($"Expected header to end with {HEADER_END}, got {headerEnd}");

            reader.BaseStream.Position = stageSettingsSection;
            BlockSize = reader.ReadVector2();
            OffsX = reader.ReadSingle();
            OffsZ = reader.ReadSingle();

            MapCollision = new Map3DCollision();

            reader.BaseStream.Position = vertexTableSection;
            MapCollision.ReadVertexTable(reader, vertexTableCount);

            reader.BaseStream.Position = collisionQuadsSection;
            MapCollision.ReadCollisionQuads(reader, collisionQuadCount);

            Gimmicks = new List<Gimmick>();
            reader.BaseStream.Position = gimmickSection;
            for (int i = 0; i < gimmickCount; i++)
            {
                Gimmick gimmick = new Gimmick();
                gimmick.Kind = reader.ReadUInt32();
                gimmick.Unknown0x4 = reader.ReadUInt32();
                gimmick.Unknown0x8 = reader.ReadUInt32();
                gimmick.Angle = reader.ReadSingle();
                Vector3 pos = new Vector3();
                pos.X = reader.ReadSingle();
                pos.Y = reader.ReadSingle();
                pos.Z = reader.ReadSingle();
                gimmick.Position = pos;
                Gimmicks.Add(gimmick);
            }

            StartPortals = new List<StartPortal>();
            reader.BaseStream.Position = startPortalSection;
            for (int i = 0; i < startPortalCount; i++)
            {
                StartPortal startPortal = new StartPortal();
                startPortal.Unknown0x0 = reader.ReadUInt32();
                startPortal.Unknown0x4 = reader.ReadUInt32();
                startPortal.Unknown0x8 = reader.ReadUInt32();
                startPortal.Angle = reader.ReadSingle();
                startPortal.Position = reader.ReadVector3();
                StartPortals.Add(startPortal);
            }

            reader.BaseStream.Position = blockSection;
            for (int i = 0; i < blockCount; i++)
            {
                Block block = new Block();
                block.Kind = reader.ReadUInt32();
                GridPos3D pos = new GridPos3D();
                pos.X = reader.ReadUInt32();
                pos.Y = reader.ReadUInt32();
                pos.Z = reader.ReadUInt32();
                block.GridPosition = pos;
                Blocks.Add(block);
            }

            reader.BaseStream.Position = warpStarSection;
            for (int i = 0; i < warpStarCount; i++)
            {
                WarpStar warpStar = new WarpStar();
                warpStar.Variation = reader.ReadUInt32();
                warpStar.Position = reader.ReadVector3();
                WarpStars.Add(warpStar);
            }

            reader.BaseStream.Position = itemSection;
            for (int i = 0; i < itemCount; i++)
            {
                Item item = new Item();
                item.Kind = (BinItemKind)reader.ReadUInt32();
                item.Variation = reader.ReadUInt32();
                item.Position = reader.ReadVector3();
                Items.Add(item);
            }

            EnemyGroups = new List<List<Enemy>>();
            for (int i = 0; i < enemyGroupCount; i++)
            {
                reader.BaseStream.Position = enemyGroupHeader + 4 + (i * 8);

                List<Enemy> group = new List<Enemy>();
                uint enemyCount = reader.ReadUInt32();
                reader.BaseStream.Position = reader.ReadUInt32();
                for (int j = 0; j < enemyCount; j++)
                {
                    Enemy obj = new Enemy();
                    obj.Kind = (ObjKind)reader.ReadUInt32();
                    obj.Type = (ObjType)reader.ReadUInt32();
                    obj.Level = reader.ReadInt32();
                    obj.Angle = reader.ReadSingle();
                    obj.Pursuit = reader.ReadInt32();
                    obj.Markers = new List<Vector3>(ENEMY_MARKER_COUNT);
                    uint posCount = reader.ReadUInt32();
                    for (int k = 0; k < ENEMY_MARKER_COUNT; k++)
                    {
                        Vector3 v = reader.ReadVector3();
                        if (k < posCount)
                            obj.Markers.Add(v);
                    }
                    group.Add(obj);
                }

                EnemyGroups.Add(group);
            }
        }

        public void Write(EndianBinaryWriter writer)
        {
            XData.WriteHeader(writer);

            long headerStart = writer.BaseStream.Position;

            writer.Write(MAGIC_NUMBER);
            writer.Write(-1);
            writer.Write(MapCollision.Vertices.Count);
            writer.Write(-1);
            writer.Write(MapCollision.Quads.Count);
            writer.Write(-1);
            writer.Write(Gimmicks.Count);
            writer.Write(-1);
            writer.Write(StartPortals.Count);
            writer.Write(-1);
            writer.Write(Blocks.Count);
            writer.Write(-1);
            writer.Write(WarpStars.Count);
            writer.Write(-1);
            writer.Write(Items.Count);
            writer.Write(-1);

            long enemyGroupListStart = writer.BaseStream.Position;
            writer.Write(EnemyGroups.Count);
            for (int i = 0; i < EnemyGroups.Count; i++)
            {
                writer.Write(EnemyGroups[i].Count);
                writer.Write(-1);
            }

            writer.Write(HEADER_END);

            writer.WritePositionAt(headerStart + 0x4);
            writer.Write(BlockSize);
            writer.Write(OffsX);
            writer.Write(OffsZ);

            writer.WritePositionAt(headerStart + 0xC);
            MapCollision.WriteVertexTable(writer);

            writer.WritePositionAt(headerStart + 0x14);
            MapCollision.WriteCollisionQuads(writer);

            writer.WritePositionAt(headerStart + 0x1C);
            for (int i = 0; i < Gimmicks.Count; i++)
            {
                var gimmick = Gimmicks[i];
                writer.Write(gimmick.Kind);
                writer.Write(gimmick.Unknown0x4);
                writer.Write(gimmick.Unknown0x8);
                writer.Write(gimmick.Angle);
                writer.Write(gimmick.Position);
            }

            writer.WritePositionAt(headerStart + 0x24);
            for (int i = 0; i < StartPortals.Count; i++)
            {
                var portal = StartPortals[i];
                writer.Write(portal.Unknown0x0);
                writer.Write(portal.Unknown0x4);
                writer.Write(portal.Unknown0x8);
                writer.Write(portal.Angle);
                writer.Write(portal.Position);
            }

            for (int i = 0; i < EnemyGroups.Count; i++)
            {
                var group = EnemyGroups[i];
                writer.WritePositionAt(enemyGroupListStart + 4 + (i * 8) + 4);
                for (int o = 0; o < group.Count; o++)
                {
                    var obj = group[o];
                    writer.Write((uint)obj.Kind);
                    writer.Write((uint)obj.Type);
                    writer.Write(obj.Level);
                    writer.Write(obj.Angle);
                    writer.Write(obj.Pursuit);
                    writer.Write(obj.Markers.Count);
                    for (int j = 0; j < ENEMY_MARKER_COUNT; j++)
                        writer.Write(j < obj.Markers.Count ? obj.Markers[j] : Vector3.Zero);
                }
            }

            writer.WritePositionAt(headerStart + 0x2C);
            for (int i = 0; i < Blocks.Count; i++)
            {
                var block = Blocks[i];
                writer.Write(block.Kind);
                writer.Write(block.GridPosition.X);
                writer.Write(block.GridPosition.Y);
                writer.Write(block.GridPosition.Z);
            }

            writer.WritePositionAt(headerStart + 0x34);
            for (int i = 0; i < WarpStars.Count; i++)
            {
                var warpStar = WarpStars[i];
                writer.Write(warpStar.Variation);
                writer.Write(warpStar.Position);
            }

            writer.WritePositionAt(headerStart + 0x3C);
            for (int i = 0; i < Items.Count; i++)
            {
                var item = Items[i];
                writer.Write((uint)item.Kind);
                writer.Write(item.Variation);
                writer.Write(item.Position);
            }

            XData.WriteFilesize(writer);
        }
    }
}
