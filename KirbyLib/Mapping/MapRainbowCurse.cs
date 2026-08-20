using KirbyLib.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace KirbyLib.Mapping
{
    /// <summary>
    /// A map file for Kirby and the Rainbow Curse.
    /// </summary>
    public partial class MapRainbowCurse
    {
        #region Structs

        public struct Actor
        {
            public ActorData Data;
            public ActorKind Kind;
            public GridPos X;
            public GridPos Y;
            public int Unk;
            public uint Wuid;
            public int Group;
            public int AutoCreateType;
            public bool IsAutoDelete;
            public bool IsUse;
        }

        #endregion

        public const uint MAGIC_NUMBER = 0x6;
        public const uint HEADER_END = 0x12345678;

        public XData XData { get; private set; } = new XData();

        public int Width => Collision.GetLength(0);
        public int Height => Collision.GetLength(1);

        public int[,] Collision;
        public byte[,] Kakusi = null;
        public int[,] Soil = null;
        public int[,] WaterFlowLayer = null;
        public byte[,] DecoMove = null;

        public List<Actor> Actors = new List<Actor>();

        public string Decoration = "Map00I01";

        public MapRainbowCurse()
        {
            XData.Version = new byte[] { 2, 0 };
            XData.Endianness = Endianness.Big;
        }

        public MapRainbowCurse(EndianBinaryReader reader)
        {
            Read(reader);
        }

        /// <summary>
        /// Creates an Actor struct from an ActorKind.<br/>
        /// If the Actor has a unique ActorData type, it is also created.
        /// </summary>
        public Actor CreateActor(ActorKind kind)
        {
            Actor actor = new Actor();
            actor.Kind = kind;

            if (ActorDataMap.ContainsKey(actor.Kind)
                && ActorDataMap[actor.Kind].BaseType == typeof(ActorData))
            {
                var type = ActorDataMap[actor.Kind];
                actor.Data = type.GetConstructor(Array.Empty<Type>()).Invoke(Array.Empty<object>()) as ActorData;
            }

            return actor;
        }

        public void Read(EndianBinaryReader reader)
        {
            XData.Read(reader);

            long headerStart = reader.BaseStream.Position;

            uint magic = reader.ReadUInt32();
            if (magic != MAGIC_NUMBER)
                throw new InvalidDataException($"Expected magic {MAGIC_NUMBER}, got {magic}");

            uint collisionSection = reader.ReadUInt32();
            reader.ReadUInt32(); // seemingly unused, 0 in header
            reader.ReadUInt32(); // seemingly unused, 0 in header
            uint actorEntrySection = reader.ReadUInt32();
            uint decorationSection = reader.ReadUInt32();
            uint section6 = reader.ReadUInt32(); // seemingly unused tilemap, but contains blank data in all maps
            uint kakusiSection = reader.ReadUInt32();
            uint soilSection = reader.ReadUInt32();
            uint waterFlowSection = reader.ReadUInt32();
            uint decoMoveSection = reader.ReadUInt32();
            reader.ReadUInt32(); // seemingly unused, 0 in header
            reader.ReadUInt32(); // seemingly unused, 0 in header
            reader.ReadUInt32(); // seemingly unused, 0 in header

            uint headerEnd = reader.ReadUInt32();
            if (headerEnd != HEADER_END)
                throw new InvalidDataException($"Expected header to end with 0x{HEADER_END:X8}, got 0x{headerEnd:X8}");

            reader.BaseStream.Position = collisionSection;
            Collision = ReadTilemap32(reader);

            reader.BaseStream.Position = actorEntrySection;
            Actors = new List<Actor>();
            uint actorCount = reader.ReadUInt32();
            for (int i = 0; i < actorCount; i++)
            {
                reader.BaseStream.Position = actorEntrySection + 4 + (i * 0x28);

                Actor actor = new Actor();
                uint data = reader.ReadUInt32();
                actor.Data = null;
                actor.Kind = (ActorKind)reader.ReadUInt32();
                actor.X = reader.ReadUInt32();
                actor.Y = reader.ReadUInt32();
                actor.Unk = reader.ReadInt32();
                actor.Wuid = reader.ReadUInt32();
                actor.Group = reader.ReadInt32();
                actor.AutoCreateType = reader.ReadInt32();
                actor.IsAutoDelete = reader.ReadUInt32() != 0;
                actor.IsUse = reader.ReadUInt32() != 0;

                if (data != 0)
                {
                    if (ActorDataMap.ContainsKey(actor.Kind)
                        && ActorDataMap[actor.Kind].BaseType == typeof(ActorData))
                    {
                        reader.BaseStream.Position = data;
                        var type = ActorDataMap[actor.Kind];
                        actor.Data = type.GetConstructor(Array.Empty<Type>()).Invoke(Array.Empty<object>()) as ActorData;
                        actor.Data.Read(reader);
                    }
                    else
                    {
                        Console.WriteLine($"Unimplemented Actor data for {actor.Kind}! Using generic class");

                        uint dataEnd = decorationSection;
                        for (int n = i + 1; n < actorCount; n++)
                        {
                            reader.BaseStream.Position = actorEntrySection + 4 + (n * 0x28);
                            uint nextData = reader.ReadUInt32();
                            if (nextData != 0)
                            {
                                dataEnd = nextData;
                                break;
                            }
                        }

                        reader.BaseStream.Position = data;
                        GenericActorData actorData = new GenericActorData();
                        actorData.Bytes = reader.ReadBytes((int)(dataEnd - data));
                        actor.Data = actorData;

                        Console.WriteLine($"- Size: 0x{actorData.Bytes.Length:X}");
                        Console.WriteLine("- Data:");
                        for (int b = 0; b < actorData.Bytes.Length; b++)
                        {
                            if (b % 0x10 == 0)
                                Console.Write('\t');
                            Console.Write(actorData.Bytes[b].ToString("X2") + " ");
                            if (b % 0x10 == 0xF)
                                Console.Write('\n');
                        }
                        Console.Write('\n');
                    }
                }

                Actors.Add(actor);
            }

            reader.BaseStream.Position = decorationSection;
            Decoration = reader.ReadStringOffset();

            reader.BaseStream.Position = section6;
            if (reader.ReadUInt32() != 0)
            {
                //Section 6 contains no data, but keeping this here just to be certain
                Console.WriteLine("Section 6 is valid!");
            }

            reader.BaseStream.Position = kakusiSection;
            Kakusi = reader.ReadUInt32() != 0 ? ReadTilemap8(reader) : null;

            reader.BaseStream.Position = soilSection;
            Soil = reader.ReadUInt32() != 0 ? ReadTilemap32(reader) : null;

            reader.BaseStream.Position = waterFlowSection;
            WaterFlowLayer = reader.ReadUInt32() != 0 ? ReadTilemap32(reader) : null;

            reader.BaseStream.Position = decoMoveSection;
            DecoMove = reader.ReadUInt32() != 0 ? ReadTilemap8(reader) : null;
        }

        public void Write(EndianBinaryWriter writer)
        {
            XData.WriteHeader(writer);

            StringHelperContainer strings = new StringHelperContainer();

            long headerStart = writer.BaseStream.Position;

            writer.Write(MAGIC_NUMBER);
            writer.Write(-1);
            writer.Write(0);
            writer.Write(0);
            writer.Write(-1);
            writer.Write(-1);
            writer.Write(-1);
            writer.Write(-1);
            writer.Write(-1);
            writer.Write(-1);
            writer.Write(-1);
            writer.Write(0);
            writer.Write(0);
            writer.Write(0);
            writer.Write(HEADER_END);

            writer.WritePositionAt(headerStart + 0x4);
            WriteTilemap(writer, Collision);

            long actorListStart = writer.BaseStream.Position;
            writer.WritePositionAt(headerStart + 0x10);
            writer.Write(Actors.Count);
            for (int i = 0; i < Actors.Count; i++)
            {
                var actor = Actors[i];
                writer.Write(0); //Data comes after
                writer.Write((uint)actor.Kind);
                writer.Write(actor.X);
                writer.Write(actor.Y);
                writer.Write(actor.Unk);
                writer.Write(actor.Wuid);
                writer.Write(actor.Group);
                writer.Write(actor.AutoCreateType);
                writer.Write(actor.IsAutoDelete ? 1 : 0);
                writer.Write(actor.IsUse ? 1 : 0);
            }

            // Write actor data
            for (int i = 0; i < Actors.Count; i++)
            {
                if (Actors[i].Data != null)
                {
                    writer.WritePositionAt(actorListStart + 4 + (i * 0x28));
                    Actors[i].Data.Write(writer, strings);
                }
            }

            writer.WritePositionAt(headerStart + 0x14);
            strings.Add(writer.BaseStream.Position, Decoration);
            writer.Write(-1);

            // Section 6
            writer.WritePositionAt(headerStart + 0x18);
            writer.Write(0);
            writer.Write(0);
            writer.Write(0);

            writer.WritePositionAt(headerStart + 0x1C);
            writer.Write(Kakusi != null ? 1 : 0);
            WriteTilemap(writer, Kakusi);

            writer.WritePositionAt(headerStart + 0x20);
            writer.Write(Soil != null ? 1 : 0);
            WriteTilemap(writer, Soil);

            writer.WritePositionAt(headerStart + 0x24);
            writer.Write(WaterFlowLayer != null ? 1 : 0);
            WriteTilemap(writer, WaterFlowLayer);

            writer.WritePositionAt(headerStart + 0x28);
            writer.Write(DecoMove != null ? 1 : 0);
            WriteTilemap(writer, DecoMove);

            strings.WriteAll(writer);

            XData.WriteFilesize(writer);
            XData.WriteFooter(writer);
        }

        byte[,] ReadTilemap8(EndianBinaryReader reader)
        {
            int width = reader.ReadInt32();
            int height = reader.ReadInt32();
            byte[,] tilemap = new byte[width, height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    tilemap[x, y] = reader.ReadByte();
                }
            }
            return tilemap;
        }

        int[,] ReadTilemap32(EndianBinaryReader reader)
        {
            int width = reader.ReadInt32();
            int height = reader.ReadInt32();
            int[,] tilemap = new int[width, height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    tilemap[x, y] = reader.ReadInt32();
                }
            }
            return tilemap;
        }

        void WriteTilemap(EndianBinaryWriter writer, byte[,] map)
        {
            if (map == null)
            {
                writer.Write(0);
                writer.Write(0);
                return;
            }

            int width = map.GetLength(0);
            int height = map.GetLength(1);
            writer.Write(width);
            writer.Write(height);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    writer.Write(map[x, y]);
                }
            }

            // Unlike HAL I'm going to actually 4-align this
            writer.WritePadding();
        }

        void WriteTilemap(EndianBinaryWriter writer, int[,] map)
        {
            if (map == null)
            {
                writer.Write(0);
                writer.Write(0);
                return;
            }

            int width = map.GetLength(0);
            int height = map.GetLength(1);
            writer.Write(width);
            writer.Write(height);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    writer.Write(map[x, y]);
                }
            }

            // Unlike HAL I'm going to actually 4-align this
            writer.WritePadding();
        }
    }
}
