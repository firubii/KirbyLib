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

        public enum BinObjKind : uint
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

        public enum BinObjType : uint
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
            public uint Unknown0x0;
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
        public class Object
        {
            public BinObjKind Kind;
            public BinObjType Type;
            public uint Param;
            public float Angle;
            public uint Unknown0x10;
            public List<Vector3> Waypoints = new List<Vector3>();
        }

        #endregion

        public const uint HEADER_END = 0x12345678;

        public const uint MAGIC_NUMBER = 0x2;

        public const uint MAX_WAYPOINT_COUNT = 7; // included

        public float Scale = 1.95f;

        public float XOffset = 9.75f;

        public float ZOffset = 7.8f;

        public List<Gimmick> Gimmicks { get; set; } = new List<Gimmick>();

        public List<StartPortal> StartPortals { get; set; } = new List<StartPortal>();

        public List<WarpStar> WarpStars { get; set; } = new List<WarpStar>();

        public XData XData { get; protected set; } = new XData();

        public Map3DCollision MapCollision { get; set; } = new Map3DCollision();

        public List<Block> Blocks { get; set; } = new List<Block>();

        public List<Item> Items { get; set; } = new List<Item>();

        public List<List<Object>> Objects { get; set; } = new List<List<Object>>();
        
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
            uint waveCount = reader.ReadUInt32();
            
            List<(uint, uint)> objWaves = new List<(uint, uint)>();
            for (int i = 0; i < waveCount; i++)
            {
                Objects.Add(new List<Object>());
                uint objCount = reader.ReadUInt32();
                uint objSection = reader.ReadUInt32();
                objWaves.Add((objCount, objSection));
            }

            uint headerEnd = reader.ReadUInt32();
            if (headerEnd != HEADER_END)
                throw new InvalidDataException($"Expected header to end with {HEADER_END}, got {headerEnd}");

            reader.BaseStream.Position = stageSettingsSection;
            Scale = reader.ReadSingle() * reader.ReadSingle();
            XOffset = reader.ReadSingle();
            ZOffset = reader.ReadSingle();

            reader.BaseStream.Position = vertexTableSection;
            MapCollision.ReadVertexTable(reader, vertexTableCount);

            reader.BaseStream.Position = collisionQuadsSection;
            MapCollision.ReadCollisionQuads(reader, collisionQuadCount);

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

            reader.BaseStream.Position = startPortalSection;
            for (int i = 0; i < startPortalCount; i++)
            {
                StartPortal startPortal = new StartPortal();
                startPortal.Unknown0x0 = reader.ReadUInt32();
                startPortal.Unknown0x4 = reader.ReadUInt32();
                startPortal.Unknown0x8 = reader.ReadUInt32();
                startPortal.Angle = reader.ReadSingle();
                Vector3 pos = new Vector3();
                pos.X = reader.ReadSingle();
                pos.Y = reader.ReadSingle();
                pos.Z = reader.ReadSingle();
                startPortal.Position = pos;
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
                warpStar.Unknown0x0 = reader.ReadUInt32();
                Vector3 pos = new Vector3();
                pos.X = reader.ReadSingle();
                pos.Y = reader.ReadSingle();
                pos.Z = reader.ReadSingle();
                warpStar.Position = pos;
                WarpStars.Add(warpStar);
            }

            reader.BaseStream.Position = itemSection;
            for (int i = 0; i < itemCount; i++)
            {
                Item item = new Item();
                item.Kind = (BinItemKind)reader.ReadUInt32();
                item.Variation = reader.ReadUInt32();
                Vector3 pos = new Vector3();
                pos.X = reader.ReadSingle();
                pos.Y = reader.ReadSingle();
                pos.Z = reader.ReadSingle();
                item.Position = pos;
                Items.Add(item);
            }

            for (int i = 0; i < waveCount; i++)
            {
                reader.BaseStream.Position = objWaves[i].Item2;
                for (int j = 0; j < objWaves[i].Item1; j++)
                {
                    Object obj = new Object();
                    obj.Kind = (BinObjKind)reader.ReadUInt32();
                    obj.Type = (BinObjType)reader.ReadUInt32();
                    obj.Param = reader.ReadUInt32();
                    obj.Angle = reader.ReadSingle();
                    obj.Unknown0x10 = reader.ReadUInt32();
                    uint waypointCount = reader.ReadUInt32();
                    for (int k = 0; k < waypointCount; k++)
                    {
                        Vector3 pos = new Vector3();
                        pos.X = reader.ReadSingle();
                        pos.Y = reader.ReadSingle();
                        pos.Z = reader.ReadSingle();
                        obj.Waypoints.Add(pos);
                    }
                    for (int k = 0; k < MAX_WAYPOINT_COUNT - waypointCount; k++)
                    {
                        reader.ReadUInt32();
                        reader.ReadUInt32();
                        reader.ReadUInt32();
                    }
                    Objects[i].Add(obj);
                }
            }
        }

        public void Write(EndianBinaryWriter writer)
        {
            XData.WriteHeader(writer);

            writer.Write(MAGIC_NUMBER);
            writer.Write(-1);
            writer.Write(MapCollision.VertexTable.Count);
            writer.Write(-1);
            writer.Write(MapCollision.CollisionQuads.Count);
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
            writer.Write(Objects.Count);

            List<long> positions = new List<long>();
            for (int i = 0; i < Objects.Count; i++)
            {
                writer.Write(Objects[i].Count);
                positions.Add(writer.BaseStream.Position);
                writer.Write(-1);
            }

            writer.Write(HEADER_END);

            writer.WritePositionAt(0x14);
            writer.Write(1.0f);
            writer.Write(Scale);
            writer.Write(XOffset);
            writer.Write(ZOffset);

            MapCollision.WriteVertexTable(writer);

            MapCollision.WriteCollisionQuads(writer);

            writer.WritePositionAt(0x2C);
            foreach (Gimmick gimmick in Gimmicks)
            {
                writer.Write(gimmick.Kind);
                writer.Write(gimmick.Unknown0x4);
                writer.Write(gimmick.Unknown0x8);
                writer.Write(gimmick.Angle);
                writer.Write(gimmick.Position.X);
                writer.Write(gimmick.Position.Y);
                writer.Write(gimmick.Position.Z);
            }

            writer.WritePositionAt(0x34);
            foreach (StartPortal portal in StartPortals)
            {
                writer.Write(portal.Unknown0x0);
                writer.Write(portal.Unknown0x4);
                writer.Write(portal.Unknown0x8);
                writer.Write(portal.Angle);
                writer.Write(portal.Position.X);
                writer.Write(portal.Position.Y);
                writer.Write(portal.Position.Z);
            }

            for (int i = 0; i < Objects.Count; i++)
            {
                writer.WritePositionAt(positions[i]);
                foreach (Object obj in Objects[i])
                {
                    writer.Write((uint)obj.Kind);
                    writer.Write((uint)obj.Type);
                    writer.Write(obj.Param);
                    writer.Write(obj.Angle);
                    writer.Write(obj.Unknown0x10);
                    writer.Write(obj.Waypoints.Count);
                    for (int j = 0; j < obj.Waypoints.Count && j < MAX_WAYPOINT_COUNT; j++)
                    {
                        writer.Write(obj.Waypoints[j].X);
                        writer.Write(obj.Waypoints[j].Y);
                        writer.Write(obj.Waypoints[j].Z);
                    }

                    for (int j = 0; j < MAX_WAYPOINT_COUNT - obj.Waypoints.Count; j++)
                    {
                        writer.Write(0.0f);
                        writer.Write(0.0f);
                        writer.Write(0.0f);
                    }
                }
            }

            writer.WritePositionAt(0x3C);
            foreach (Block block in Blocks)
            {
                writer.Write(block.Kind);
                writer.Write(block.GridPosition.X);
                writer.Write(block.GridPosition.Y);
                writer.Write(block.GridPosition.Z);
            }

            writer.WritePositionAt(0x44);

            foreach (WarpStar warpStar in WarpStars)
            {
                writer.Write(warpStar.Unknown0x0);
                writer.Write(warpStar.Position.X);
                writer.Write(warpStar.Position.Y);
                writer.Write(warpStar.Position.Z);
            }

            writer.WritePositionAt(0x4C);
            foreach (Item item in Items)
            {
                writer.Write((uint)item.Kind);
                writer.Write(item.Variation);
                writer.Write(item.Position.X);
                writer.Write(item.Position.Y);
                writer.Write(item.Position.Z);
            }

            XData.WriteFilesize(writer);
        }
    }
}
