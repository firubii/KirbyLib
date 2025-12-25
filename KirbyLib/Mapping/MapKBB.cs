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

        public enum FoodKind : uint
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

        public enum ItemKind : uint
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
            public uint Variation;
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
            public ItemKind Kind;
            public uint Variation;
            public Vector3 Position;
        }

        public struct Enemy
        {
            public ObjKind Kind;
            public ObjType Type;
            public int Level;
            public float Dir;
            public int Unknown;
            public int Pursuit;
            public int Big;
            public int AlwaysUpdateOutOfScreen;
            public int PosterioriEnemyListIdSeed;
            public int GroupId;
            public List<Vector3> Markers;
        }

        public struct Tree
        {
            public Vector3 Position;
            public int Unknown1;
            public int Unknown2;
            public int Unknown3;
            public int Unknown4;
        }

        #endregion

        public const uint MAGIC_NUMBER = 0x8;
        public const uint HEADER_END = 0x12345678;

        public const int ENEMY_MARKER_COUNT = 15;

        public XData XData { get; protected set; } = new XData();

        public Map3DCollision MapCollision = new Map3DCollision();

        public List<Gimmick> Gimmicks = new List<Gimmick>();
        public List<StartPortal> StartPortals = new List<StartPortal>();
        /// <summary>
        /// List of tree objects. Completely unused.
        /// </summary>
        public List<Tree> Trees = new List<Tree>();
        public List<Block> Blocks = new List<Block>();
        public List<WarpStar> WarpStars = new List<WarpStar>();
        public List<Item> Items = new List<Item>();
        public List<Enemy> Enemies = new List<Enemy>();
        /// <summary>
        /// The position Kirby will be moved to at the end of the level. Some levels like 2-4
        /// leave it null, to replicate this, set the vector3 to null.
        /// </summary>
        public Vector3? PlayerEndPos = null;
        public Vector3? PlayerEndPos2 = null;
        public List<Yaml> YamlGimmicks = new List<Yaml>();
        public List<Enemy> RoomGuardObjects = new List<Enemy>();
        public Vector3? AmiiboDoorPos = null;
        /// <summary>
        /// A collection of all the items that should only appear once a room guard ends.
        /// </summary>
        public List<Item> RoomGuardEndItems { get; set; } = new List<Item>();

        public Vector2 BlockSize = Vector2.One;
        public float Unknown0x8 = 1f;
        public float Unknown0xC = 1f;
        public float Unknown0x10 = 1f;
        public float OffsX = 1f;
        public float OffsZ = 1f;

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

            long headerStart = reader.BaseStream.Position;

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
            uint treeCount = reader.ReadUInt32();
            uint treeListSection = reader.ReadUInt32();
            uint blockCount = reader.ReadUInt32();
            uint blockSection = reader.ReadUInt32();
            uint warpStarCount = reader.ReadUInt32();
            uint warpStarSection = reader.ReadUInt32();
            uint itemCount = reader.ReadUInt32();
            uint itemSection = reader.ReadUInt32();
            uint enemyGroups = reader.ReadUInt32(); // unused group count
            uint enemyCount = reader.ReadUInt32();
            uint enemyListAddr = reader.ReadUInt32();
            int endPosAddr = reader.ReadInt32();
            int endPos2Addr = reader.ReadInt32();
            uint yamlFilesCount = reader.ReadUInt32();
            uint yamlFilesSection = reader.ReadUInt32();
            uint roomGuardObjectsCount = reader.ReadUInt32();
            uint roomGuardObjectsSection = reader.ReadUInt32();
            uint amiiboDoorPos = reader.ReadUInt32();
            uint roomGuardEndItemsCount = reader.ReadUInt32();
            uint roomGuardEndItemsSection = reader.ReadUInt32();
            reader.ReadUInt32();
            uint headerEnd = reader.ReadUInt32();
            if (headerEnd != HEADER_END)
                throw new InvalidDataException($"Expected header to end with {HEADER_END}, got {headerEnd}");

            reader.BaseStream.Position = stageSettingsSection;
            BlockSize = reader.ReadVector2();
            Unknown0x8 = reader.ReadSingle();
            Unknown0xC = reader.ReadSingle();
            Unknown0x10 = reader.ReadSingle();
            OffsX = reader.ReadSingle();
            OffsZ = reader.ReadSingle();

            reader.BaseStream.Position = vertexTableSection;
            MapCollision.ReadVertexTable(reader, vertexTableCount);

            reader.BaseStream.Position = collisionQuadSection;
            MapCollision.ReadCollisionQuads(reader, collisionQuadCount);

            reader.BaseStream.Position = gimmickSection;
            Gimmicks = new List<Gimmick>();
            for (int i = 0; i < gimmickCount; i++)
            {
                Gimmick gimmick = new Gimmick();
                gimmick.Kind = reader.ReadUInt32();
                gimmick.Unknown0x4 = reader.ReadUInt32();
                gimmick.Unknown0x8 = reader.ReadUInt32();
                gimmick.Angle = reader.ReadSingle();
                gimmick.Position = reader.ReadVector3();
                Gimmicks.Add(gimmick);
            }

            reader.BaseStream.Position = startPortalSection;
            StartPortals = new List<StartPortal>();
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

            reader.BaseStream.Position = treeListSection;
            Trees = new List<Tree>();
            for (int i = 0; i < treeCount; i++)
            {
                Tree tree = new Tree();
                tree.Position = reader.ReadVector3();
                tree.Unknown1 = reader.ReadInt32();
                tree.Unknown2 = reader.ReadInt32();
                tree.Unknown3 = reader.ReadInt32();
                tree.Unknown4 = reader.ReadInt32();
                Trees.Add(tree);
            }

            reader.BaseStream.Position = blockSection;
            Blocks = new List<Block>();
            for (int i = 0; i < blockCount; i++)
            {
                Block block = new Block();
                block.Kind = reader.ReadUInt32();
                block.Position = reader.ReadVector3();
                Blocks.Add(block);
            }

            reader.BaseStream.Position = warpStarSection;
            WarpStars = new List<WarpStar>();
            for (int i = 0; i < warpStarCount; i++)
            {
                WarpStar warpStar = new WarpStar();
                warpStar.Variation = reader.ReadUInt32();
                warpStar.Position = reader.ReadVector3();
                WarpStars.Add(warpStar);
            }

            reader.BaseStream.Position = itemSection;
            Items = new List<Item>();
            for (int i = 0; i < itemCount; i++)
                Items.Add(ReadItem(reader));

            reader.BaseStream.Position = enemyListAddr;
            Enemies = new List<Enemy>();
            for (int i = 0; i < enemyCount; i++)
                Enemies.Add(ReadEnemy(reader));

            PlayerEndPos = null;
            if (endPosAddr > -1)
            {
                reader.BaseStream.Position = endPosAddr;
                PlayerEndPos = reader.ReadVector3();
            }

            PlayerEndPos2 = null;
            if (endPos2Addr > -1)
            {
                reader.BaseStream.Position = endPos2Addr;
                PlayerEndPos2 = reader.ReadVector3();
            }

            reader.BaseStream.Position = yamlFilesSection;
            YamlGimmicks = new List<Yaml>();
            for (int i = 0; i < yamlFilesCount; i++)
            {
                reader.BaseStream.Position = yamlFilesSection + (i * 4);
                reader.BaseStream.Position = reader.ReadUInt32();

                byte[] yamlData = XData.ExtractFile(reader);

                using (MemoryStream yamlStream = new MemoryStream(yamlData))
                using (EndianBinaryReader yamlReader = new EndianBinaryReader(yamlStream))
                    YamlGimmicks.Add(new Yaml(yamlReader));
            }

            reader.BaseStream.Position = roomGuardObjectsSection;
            RoomGuardObjects = new List<Enemy>();
            for (int i = 0; i < roomGuardObjectsCount; i++)
                RoomGuardObjects.Add(ReadEnemy(reader));

            AmiiboDoorPos = null;
            if (amiiboDoorPos != 0)
            {
                reader.BaseStream.Position = amiiboDoorPos;
                AmiiboDoorPos = reader.ReadVector3();
            }

            reader.BaseStream.Position = roomGuardEndItemsSection;
            RoomGuardEndItems = new List<Item>();
            for (int i = 0; i < roomGuardEndItemsCount; i++)
                RoomGuardEndItems.Add(ReadItem(reader));
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
            writer.Write(Trees.Count);
            writer.Write(-1);
            writer.Write(Blocks.Count);
            writer.Write(-1);
            writer.Write(WarpStars.Count);
            writer.Write(-1);
            writer.Write(Items.Count);
            writer.Write(-1);
            writer.Write(1); // unused enemy group count
            writer.Write(Enemies.Count);
            writer.Write(-1);
            writer.Write(-1);
            writer.Write(-1);
            writer.Write(YamlGimmicks.Count);
            writer.Write(-1);
            writer.Write(RoomGuardObjects.Count);
            writer.Write(-1);
            writer.Write(0);
            writer.Write(RoomGuardEndItems.Count);
            writer.Write(-1);
            writer.Write(1);
            writer.Write(HEADER_END);

            writer.WritePositionAt(headerStart + 0x4);
            writer.Write(BlockSize);
            writer.Write(Unknown0x8);
            writer.Write(Unknown0xC);
            writer.Write(Unknown0x10);
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

            writer.WritePositionAt(headerStart + 0x2C);
            for (int i = 0; i < Trees.Count; i++)
            {
                var tree = Trees[i];
                writer.Write(tree.Position);
                writer.Write(tree.Unknown1);
                writer.Write(tree.Unknown2);
                writer.Write(tree.Unknown3);
                writer.Write(tree.Unknown4);
            }

            writer.WritePositionAt(headerStart + 0x34);
            for (int i = 0; i < Blocks.Count; i++)
            {
                var block = Blocks[i];
                writer.Write(block.Kind);
                writer.Write(block.Position);
            }

            writer.WritePositionAt(headerStart + 0x3C);
            for (int i = 0; i < WarpStars.Count; i++)
            {
                var star = WarpStars[i];
                writer.Write(star.Variation);
                writer.Write(star.Position);
            }

            writer.WritePositionAt(headerStart + 0x44);
            for (int i = 0; i < Items.Count; i++)
                WriteItem(writer, Items[i]);

            writer.WritePositionAt(headerStart + 0x50);
            for (int i = 0; i < Enemies.Count; i++)
                WriteEnemy(writer, Enemies[i]);

            if (PlayerEndPos.HasValue)
            {
                writer.WritePositionAt(headerStart + 0x54);
                writer.Write(PlayerEndPos.Value);
            }

            if (PlayerEndPos2.HasValue)
            {
                writer.WritePositionAt(headerStart + 0x58);
                writer.Write(PlayerEndPos2.Value);
            }

            writer.WritePositionAt(headerStart + 0x60);
            long yamlListStart = writer.BaseStream.Position;
            for (int i = 0; i < YamlGimmicks.Count; i++)
                writer.Write(-1);

            for (int i = 0; i < YamlGimmicks.Count; i++)
            {
                writer.WritePositionAt(yamlListStart + (i * 4));
                using (MemoryStream yamlStream = new MemoryStream())
                {
                    using (EndianBinaryWriter yamlWriter = new EndianBinaryWriter(yamlStream))
                        YamlGimmicks[i].Write(yamlWriter);
                    writer.Write(yamlStream.ToArray());
                }
            }

            writer.WritePositionAt(headerStart + 0x68);
            for (int i = 0; i < RoomGuardObjects.Count; i++)
                WriteEnemy(writer, RoomGuardObjects[i]);

            if (AmiiboDoorPos.HasValue)
            {
                writer.WritePositionAt(headerStart + 0x6C);
                writer.Write(AmiiboDoorPos.Value);
            }

            writer.WritePositionAt(headerStart + 0x74);
            for (int i = 0; i < RoomGuardEndItems.Count; i++)
                WriteItem(writer, RoomGuardEndItems[i]);

            XData.WriteFilesize(writer);
        }

        private static Enemy ReadEnemy(EndianBinaryReader reader)
        {
            Enemy obj = new Enemy();
            obj.Kind = (ObjKind)reader.ReadUInt32();
            obj.Type = (ObjType)reader.ReadUInt32();
            obj.Level = reader.ReadInt32();
            obj.Dir = reader.ReadSingle();
            obj.Unknown = reader.ReadInt32();
            obj.Pursuit = reader.ReadInt32();
            obj.Big = reader.ReadInt32();
            obj.AlwaysUpdateOutOfScreen = reader.ReadInt32();
            obj.PosterioriEnemyListIdSeed = reader.ReadInt32();
            obj.GroupId = reader.ReadInt32();
            obj.Markers = new List<Vector3>(ENEMY_MARKER_COUNT);
            uint posCount = reader.ReadUInt32();
            for (int i = 0; i < ENEMY_MARKER_COUNT; i++)
            {
                Vector3 v = reader.ReadVector3();
                if (i < posCount)
                    obj.Markers.Add(v);
            }

            return obj;
        }

        private static Item ReadItem(EndianBinaryReader reader)
        {
            Item item = new Item();
            item.Kind = (ItemKind)reader.ReadUInt32();
            item.Variation = reader.ReadUInt32();
            item.Position = reader.ReadVector3();
            return item;
        }

        private static void WriteEnemy(EndianBinaryWriter writer, Enemy obj)
        {
            writer.Write((uint)obj.Kind);
            writer.Write((uint)obj.Type);
            writer.Write(obj.Level);
            writer.Write(obj.Dir);
            writer.Write(obj.Unknown);
            writer.Write(obj.Pursuit);
            writer.Write(obj.Big);
            writer.Write(obj.AlwaysUpdateOutOfScreen);
            writer.Write(obj.PosterioriEnemyListIdSeed);
            writer.Write(obj.GroupId);
            writer.Write(obj.Markers.Count);
            for (int i = 0; i < ENEMY_MARKER_COUNT; i++)
                writer.Write(i < obj.Markers.Count ? obj.Markers[i] : Vector3.Zero);
        }

        private static void WriteItem(EndianBinaryWriter writer, Item item)
        {
            writer.Write((uint)item.Kind);
            writer.Write(item.Variation);
            writer.Write(item.Position);
        }
    }
}
