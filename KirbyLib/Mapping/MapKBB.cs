using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using KirbyLib.IO;

namespace KirbyLib.Mapping
{
    public class MapKBB
    {
        #region Enums

        public enum BinFoodKind : uint
        {
            DinnerCurry,
            DinnerOmelet,
            UNUSED_0,
            UNUSED_1,
            DrinkCreamSoda,
            UNUSED_2,
            FruitCherry,
            UNUSED_3,
            FruitPineApple,
            UNUSED_4,
            UNUSED_5,
            UNUSED_6,
            UNUSED_7,
            UNUSED_8,
            UNUSED_9,
            UNUSED_10,
            UNUSED_11,
            UNUSED_12,
            UNUSED_13,
            UNUSED_14,
            UNUSED_15,
            SweetsPudding,
            SweetsShortCake,
            UNUSED_16,
            VegetableCarrot,
            UNUSED_17,
            SweetsChocolate
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
            Gordo,
            Kabu,
            Scarfy,
            Dedede,
            Nruff,
            Cappy,
            Mumbies,
            Wonkey,
            Lololo,
            Lalala,
            Glunk,
            Kracko,
            Soarar,
            Chip,
            Babut,
            Squishy,
            Sectledee,
            Sectraburt,
            Shotzo,
            KrackoJr,
            SpearSectledee
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
            AngryAppear,
            WaitTackle,
            StraightTackle,
            AlwaysPursuit,
            Escape,
            EscapeStraight,
            EscapeRange,
            EscapeStraightRange,
            JumpPursuit,
            JumpRoundTrip
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
        /// A destructible block.
        /// </summary>
        public struct Block
        {
            public uint Kind;
            public Vector3 Position;
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

        public class Object
        {
            public BinObjKind Kind;
            public BinObjType Type;
            public uint Param;
            public float Angle;
            public int Unknown0x10;
            public uint Unknown0x14;
            public uint Unknown0x18;
            public uint Unknown0x1C;
            public int Unknown0x20;
            public uint Unknown0x24;
            public List<Vector3> Waypoints = new List<Vector3>();
        }

        #endregion

        public const uint MAGIC_NUMBER = 0x8;

        public const uint MAX_WAYPOINT_COUNT = 15; // included

        public const uint HEADER_END = 0x12345678;

        public float Scale = 1.95f;

        public float XOffset = 9.75f;

        public float ZOffset = 7.8f;

        public float Unknown0x8 = 11.7f;

        public float Unknown0xC = -54.6f;

        public float Unknown0x10 = 1.95f;

        public XData XData { get; protected set; } = new XData();

        public Map3DCollision MapCollision { get; set; } = new Map3DCollision();

        /// <summary>
        /// An unused section.
        /// </summary>
        public List<byte[]> UnknownSection { get; set; } = new List<byte[]>();

        public List<Block> Blocks { get; set; } = new List<Block>();

        public List<Item> Items { get; set; } = new List<Item>();

        public List<Gimmick> Gimmicks { get; set; } = new List<Gimmick>();

        public List<StartPortal> StartPortals { get; set; } = new List<StartPortal>();

        public List<WarpStar> WarpStars { get; set; } = new List<WarpStar>();

        /// <summary>
        /// A collection of objects that are not in room guarders.
        /// </summary>
        public List<Object> FreeObjects { get; set; } = new List<Object>();

        /// <summary>
        /// The position Kirby will be moved to at the end of the level. Some levels like 2-4
        /// leave it null, to replicate this, set the vector3 to null.
        /// </summary>
        public Vector3? EndPosition { get; set; } = new Vector3();

        public List<Yaml> YamlFiles { get; set; } = new List<Yaml>();

        public List<Object> RoomGuardObjects { get; set; } = new List<Object>();

        // Not too sure about this one, might need some double checking when we have more
        // sophisticated tools for KBB level editing.
        public Vector3 MaxCameraOffset { get; set; } = new Vector3();

        /// <summary>
        /// A collection of all the items that should only appear once a room guard ends.
        /// </summary>
        public List<Item> RoomGuardEndItems { get; set; } = new List<Item>();

        public uint Unknown = 1;

        public MapKBB()
        {
            XData.Endianness = Endianness.Little;
            XData.Version = new byte[] { 2, 0 };
        }

        public MapKBB(EndianBinaryReader reader)
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
            uint collisionQuadSection = reader.ReadUInt32();
            uint gimmickCount = reader.ReadUInt32();
            uint gimmickSection = reader.ReadUInt32();
            uint startPortalCount = reader.ReadUInt32();
            uint startPortalSection = reader.ReadUInt32();
            reader.ReadUInt32();
            reader.ReadUInt32();
            uint blockCount = reader.ReadUInt32();
            uint blockSection = reader.ReadUInt32();
            uint warpStarCount = reader.ReadUInt32();
            uint warpStarSection = reader.ReadUInt32();
            uint itemCount = reader.ReadUInt32();
            uint itemSection = reader.ReadUInt32();
            reader.ReadUInt32();
            uint freeObjectsCount = reader.ReadUInt32();
            uint freeObjectsSection = reader.ReadUInt32();
            uint endPosSection = reader.ReadUInt32();
            reader.ReadUInt32();
            uint yamlFilesCount = reader.ReadUInt32();
            uint yamlFilesSection = reader.ReadUInt32();
            uint roomGuardObjectsCount = reader.ReadUInt32();
            uint roomGuardObjectsSection = reader.ReadUInt32();
            uint camOffsetSection = reader.ReadUInt32();
            uint roomGuardEndItemsCount = reader.ReadUInt32();
            uint roomGuardEndItemsSection = reader.ReadUInt32();
            reader.ReadUInt32();
            uint headerEnd = reader.ReadUInt32();
            if (headerEnd != HEADER_END)
                throw new InvalidDataException($"Expected header to end with {HEADER_END}, got {headerEnd}");

            reader.BaseStream.Position = stageSettingsSection;
            Scale = reader.ReadSingle() * reader.ReadSingle();
            Unknown0x8 = reader.ReadSingle();
            Unknown0xC = reader.ReadSingle();
            Unknown0x10 = reader.ReadSingle();
            XOffset = reader.ReadSingle();
            ZOffset = reader.ReadSingle();

            reader.BaseStream.Position = vertexTableSection;
            MapCollision.ReadVertexTable(reader, vertexTableCount);

            reader.BaseStream.Position = collisionQuadSection;
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
                Vector3 pos = new Vector3(
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle());
                block.Position = pos;
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
                Items.Add(ReadItem(reader));

            reader.BaseStream.Position = freeObjectsSection;
            for (int i = 0; i < freeObjectsCount; i++)
                FreeObjects.Add(ReadObject(reader));

            if (endPosSection == 0xFFFFFFFF)
                EndPosition = null;
            else
            {
                reader.BaseStream.Position = endPosSection;
                EndPosition = new Vector3(
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle());
            }

            reader.BaseStream.Position = yamlFilesSection;
            for (int i = 0; i < yamlFilesCount; i++)
            {
                long pos = reader.BaseStream.Position;
                uint yamlFileSection = reader.ReadUInt32();
                reader.BaseStream.Position = yamlFileSection;
                byte[] yamlData = XData.ExtractFile(reader);

                Yaml yamlFile;
                using (MemoryStream yamlStream = new MemoryStream(yamlData))
                using (EndianBinaryReader yamlReader = new EndianBinaryReader(yamlStream))
                    yamlFile = new Yaml(yamlReader);
                YamlFiles.Add(yamlFile);

                reader.BaseStream.Position = pos;
                reader.ReadUInt32();
            }

            reader.BaseStream.Position = roomGuardObjectsSection;
            for (int i = 0; i < roomGuardObjectsCount; i++)
                RoomGuardObjects.Add(ReadObject(reader));

            reader.BaseStream.Position = camOffsetSection;
            MaxCameraOffset = new Vector3(
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle());

            reader.BaseStream.Position = roomGuardEndItemsSection;
            for (int i = 0; i < roomGuardEndItemsCount; i++)
                RoomGuardEndItems.Add(ReadItem(reader));
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
            writer.Write(UnknownSection.Count);
            writer.Write(-1);
            writer.Write(Blocks.Count);
            writer.Write(-1);
            writer.Write(WarpStars.Count);
            writer.Write(-1);
            writer.Write(Items.Count);
            writer.Write(-1);
            writer.Write(1);
            writer.Write(FreeObjects.Count);
            writer.Write(-1);
            writer.Write(-1);
            writer.Write(-1);
            writer.Write(YamlFiles.Count);
            writer.Write(-1);
            writer.Write(RoomGuardObjects.Count);
            writer.Write(-1);
            writer.Write(-1);
            writer.Write(RoomGuardEndItems.Count);
            writer.Write(-1);
            writer.Write(1);
            writer.Write(HEADER_END);

            writer.WritePositionAt(0x14);
            writer.Write(1.0f);
            writer.Write(Scale);
            writer.Write(Unknown0x8);
            writer.Write(Unknown0xC);
            writer.Write(Unknown0x10);
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

            writer.WritePositionAt(0x60);
            foreach (Object obj in FreeObjects)
                WriteObject(writer, obj);

            writer.WritePositionAt(0x78);
            foreach (Object obj in RoomGuardObjects)
                WriteObject(writer, obj);

            writer.WritePositionAt(0x3C);
            foreach (byte[] buffer in UnknownSection)
                writer.Write(buffer);

            writer.WritePositionAt(0x44);
            foreach (Block block in Blocks)
            {
                writer.Write(block.Kind);
                writer.Write(block.Position.X);
                writer.Write(block.Position.Y);
                writer.Write(block.Position.Z);
            }

            writer.WritePositionAt(0x4C);
            foreach (WarpStar star in WarpStars)
            {
                writer.Write(star.Unknown0x0);
                writer.Write(star.Position.X);
                writer.Write(star.Position.Y);
                writer.Write(star.Position.Z);
            }

            writer.WritePositionAt(0x54);
            foreach (Item item in Items)
                WriteItem(writer, item);

            writer.WritePositionAt(0x84);
            foreach (Item item in RoomGuardEndItems)
                WriteItem(writer, item);

            if (EndPosition.HasValue)
            {
                writer.WritePositionAt(0x64);
                writer.Write(EndPosition.Value.X);
                writer.Write(EndPosition.Value.Y);
                writer.Write(EndPosition.Value.Z);
            }

            writer.WritePositionAt(0x70);
            List<long> yamlPos = new List<long>();
            for (int i = 0; i < YamlFiles.Count; i++)
            {
                yamlPos.Add(writer.BaseStream.Position);
                writer.Write(-1);
            }
            for (int i = 0; i < YamlFiles.Count; i++)
            {
                writer.WritePositionAt(yamlPos[i]);
                using (MemoryStream yamlStream = new MemoryStream())
                {
                    using (EndianBinaryWriter yamlWriter = new EndianBinaryWriter(yamlStream))
                        YamlFiles[i].Write(yamlWriter);
                    writer.Write(yamlStream.ToArray());
                }
                while (writer.BaseStream.Position % 4 != 0)
                    writer.Write((byte)0);
            }

            writer.WritePositionAt(0x7C);
            writer.Write(MaxCameraOffset.X);
            writer.Write(MaxCameraOffset.Y);
            writer.Write(MaxCameraOffset.Z);

            XData.WriteFilesize(writer);
        }

        private static Object ReadObject(EndianBinaryReader reader)
        {
            Object obj = new Object();
            obj.Kind = (BinObjKind)reader.ReadUInt32();
            obj.Type = (BinObjType)reader.ReadUInt32();
            obj.Param = reader.ReadUInt32();
            obj.Angle = reader.ReadSingle();
            obj.Unknown0x10 = reader.ReadInt32();
            obj.Unknown0x14 = reader.ReadUInt32();
            obj.Unknown0x18 = reader.ReadUInt32();
            obj.Unknown0x1C = reader.ReadUInt32();
            obj.Unknown0x20 = reader.ReadInt32();
            obj.Unknown0x24 = reader.ReadUInt32();
            uint waypointCount = reader.ReadUInt32();
            for (int i = 0; i < waypointCount; i++)
                obj.Waypoints.Add(new Vector3(
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle()));
            for (int i = 0; i < MAX_WAYPOINT_COUNT - waypointCount; i++)
            {
                reader.ReadSingle();
                reader.ReadSingle();
                reader.ReadSingle();
            }
            return obj;
        }

        private static Item ReadItem(EndianBinaryReader reader)
        {
            Item item = new Item();
            item.Kind = (BinItemKind)reader.ReadUInt32();
            item.Variation = reader.ReadUInt32();
            Vector3 pos = new Vector3(
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle());
            item.Position = pos;
            return item;
        }

        private static void WriteObject(EndianBinaryWriter writer, Object obj)
        {
            writer.Write((uint)obj.Kind);
            writer.Write((uint)obj.Type);
            writer.Write(obj.Param);
            writer.Write(obj.Angle);
            writer.Write(obj.Unknown0x10);
            writer.Write(obj.Unknown0x14);
            writer.Write(obj.Unknown0x18);
            writer.Write(obj.Unknown0x1C);
            writer.Write(obj.Unknown0x20);
            writer.Write(obj.Unknown0x24);
            writer.Write(obj.Waypoints.Count);
            for (int i = 0; i < obj.Waypoints.Count && i < MAX_WAYPOINT_COUNT; i++)
            {
                writer.Write(obj.Waypoints[i].X);
                writer.Write(obj.Waypoints[i].Y);
                writer.Write(obj.Waypoints[i].Z);
            }
            for (int i = 0; i < MAX_WAYPOINT_COUNT - obj.Waypoints.Count; i++)
            {
                writer.Write(0.0f);
                writer.Write(0.0f);
                writer.Write(0.0f);
            }
        }

        private static void WriteItem(EndianBinaryWriter writer, Item item)
        {
            writer.Write((uint)item.Kind);
            writer.Write(item.Variation);
            writer.Write(item.Position.X);
            writer.Write(item.Position.Y);
            writer.Write(item.Position.Z);
        }
    }
}
