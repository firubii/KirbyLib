using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KirbyLib.IO;

namespace KirbyLib.Mapping
{
    /// <summary>
    /// A map file for the Sub-Game Dedede's Drum Dash found in Kirby Triple Deluxe.
    /// </summary>
    public class MapDrumDash
    {
        #region Enums

        public enum DrumKind
        {
            StartDrum,
            NormalDrum,
            NormalDrumS,
            GoalDrum,
            NormalDrumB,
            StartDrumS,
            StartDrumL,
            BlowDrum1,
            BlowDrum2
        }

        public enum DrumVariation
        {
            Normal,
            Move,
            NormalInfoJumpM,
            NormalInfoJumpL
        }

        public enum DrumDirType
        {
            L,
            R
        }

        public enum GimmickKind
        {
            Gordo,
            GordoBig,
            Cloud,
            Bouncy,
            Brontoburt,
            Soarar,
            Sodory,
            Scarfy
        }

        public enum GimmickDirType
        {
            U,
            L,
            D,
            R
        }

        public enum ItemKind
        {
            StarS,
            StarM,
            StarL
        }

        #endregion

        #region Structs

        public struct Drum
        {
            public DrumKind Kind;
            public DrumVariation Variation;
            public DrumDirType Dir;
            public float Param;
            public GridPos X;
            public GridPos Y;
        }

        public struct Gimmick
        {
            public GimmickKind Kind;
            public uint Variation;
            public GimmickDirType Dir;
            public float Param;
            public GridPos X;
            public GridPos Y;
        }

        public struct Item
        {
            public ItemKind Kind;
            public uint Variation;
            public GridPos X;
            public GridPos Y;
        }

        #endregion

        public const uint MAGIC_NUMBER = 0x22;
        public const uint HEADER_END = 0x12345678;

        public XData XData { get; private set; } = new XData();

        public List<Drum> Drums = new List<Drum>();
        public List<Gimmick> Gimmicks = new List<Gimmick>();
        public List<Item> Items = new List<Item>();

        public string BGM = "BGM_LP_GBOLD1";
        public uint TimeLimit = 120;
        public bool Unknown = false;

        public MapDrumDash()
        {
            XData.Version = new byte[] { 2, 0 };
            XData.Endianness = Endianness.Little;
        }

        public MapDrumDash(EndianBinaryReader reader)
        {
            Read(reader);
        }

        public void Read(EndianBinaryReader reader)
        {
            XData.Read(reader);

            uint magic = reader.ReadUInt32();
            if (magic != MAGIC_NUMBER)
                throw new InvalidDataException($"Expected magic {MAGIC_NUMBER}, got {magic}");

            uint generalSection = reader.ReadUInt32();
            uint drumSectiom = reader.ReadUInt32();
            uint gimmickSection = reader.ReadUInt32();
            uint itemSection = reader.ReadUInt32();

            uint headerEnd = reader.ReadUInt32();
            if (headerEnd != HEADER_END)
                throw new InvalidDataException($"Expected header to end with 0x{HEADER_END:X8}, got 0x{headerEnd:X8}");

            reader.BaseStream.Position = generalSection;
            BGM = reader.ReadStringOffset();
            TimeLimit = reader.ReadUInt32();
            Unknown = reader.ReadUInt32() != 0;

            reader.BaseStream.Position = drumSectiom;
            Drums = new List<Drum>();
            uint drumCount = reader.ReadUInt32();
            for (int i = 0; i < drumCount; i++)
            {
                Drum drum = new Drum();
                drum.Kind = (DrumKind)reader.ReadUInt32();
                drum.Variation = (DrumVariation)reader.ReadUInt32();
                drum.Dir = (DrumDirType)reader.ReadUInt32();
                drum.Param = reader.ReadSingle();
                drum.X = reader.ReadUInt32();
                drum.Y = reader.ReadUInt32();
                Drums.Add(drum);
            }

            reader.BaseStream.Position = gimmickSection;
            Gimmicks = new List<Gimmick>();
            uint gimmickCount = reader.ReadUInt32();
            for (int i = 0; i < gimmickCount; i++)
            {
                Gimmick gimmick = new Gimmick();
                gimmick.Kind = (GimmickKind)reader.ReadUInt32();
                gimmick.Variation = reader.ReadUInt32();
                gimmick.Dir = (GimmickDirType)reader.ReadUInt32();
                gimmick.Param = reader.ReadSingle();
                gimmick.X = reader.ReadUInt32();
                gimmick.Y = reader.ReadUInt32();
                Gimmicks.Add(gimmick);
            }

            reader.BaseStream.Position = itemSection;
            Items = new List<Item>();
            uint itemCount = reader.ReadUInt32();
            for (int i = 0; i < itemCount; i++)
            {
                Item item = new Item();
                item.Kind = (ItemKind)reader.ReadUInt32();
                item.Variation = reader.ReadUInt32();
                item.X = reader.ReadUInt32();
                item.Y = reader.ReadUInt32();
                Items.Add(item);
            }
        }

        public void Write(EndianBinaryWriter writer)
        {
            XData.WriteHeader(writer);

            StringHelperContainer strings = new StringHelperContainer();

            long headerStart = writer.BaseStream.Position;

            writer.Write(MAGIC_NUMBER);
            writer.Write(-1);
            writer.Write(-1);
            writer.Write(-1);
            writer.Write(-1);
            writer.Write(HEADER_END);
            writer.WritePadding(0x20);

            writer.WritePositionAt(headerStart + 0x4);
            strings.Add(writer.BaseStream.Position, BGM);
            writer.Write(-1);
            writer.Write(TimeLimit);
            writer.Write(Unknown ? 1 : 0);
            writer.WritePadding(0x20);

            writer.WritePositionAt(headerStart + 0x8);
            writer.Write(Drums.Count);
            for (int i = 0; i < Drums.Count; i++)
            {
                Drum drum = Drums[i];
                writer.Write((uint)drum.Kind);
                writer.Write((uint)drum.Variation);
                writer.Write((uint)drum.Dir);
                writer.Write(drum.Param);
                writer.Write(drum.X);
                writer.Write(drum.Y);
            }
            writer.WritePadding(0x10);

            writer.WritePositionAt(headerStart + 0xC);
            writer.Write(Gimmicks.Count);
            for (int i = 0; i < Gimmicks.Count; i++)
            {
                Gimmick gimmick = Gimmicks[i];
                writer.Write((uint)gimmick.Kind);
                writer.Write(gimmick.Variation);
                writer.Write((uint)gimmick.Dir);
                writer.Write(gimmick.Param);
                writer.Write(gimmick.X);
                writer.Write(gimmick.Y);
            }
            writer.WritePadding(0x10);

            writer.WritePositionAt(headerStart + 0x10);
            writer.Write(Items.Count);
            for (int i = 0; i < Items.Count; i++)
            {
                Item item = Items[i];
                writer.Write((uint)item.Kind);
                writer.Write(item.Variation);
                writer.Write(item.X);
                writer.Write(item.Y);
            }

            strings.WriteAll(writer);

            XData.WriteFilesize(writer);
            XData.WriteFooter(writer);
        }
    }
}
