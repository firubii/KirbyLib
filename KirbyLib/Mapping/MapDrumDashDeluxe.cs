using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KirbyLib.IO;

namespace KirbyLib.Mapping
{
    /// <summary>
    /// A map file for Dedede's Drum Dash Deluxe.
    /// </summary>
    public class MapDrumDashDeluxe
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
            BlowDrum2,
            Wing3dDrumL,
            Wing3dDrumS,
            SwitchWingDrumL,
            SwitchWingDrumS,
            SpineDrumL
        }

        public enum DrumVariation
        {
            Normal,
            Move,
            NormalInfoJumpM,
            NormalInfoJumpL,
            MoveX2FromCenter,
            MoveX2FromEdge
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
            Scarfy,
            SpinyRing,
            Gong,
            Beetlie,
            Funnyrobats,
            Como,
            Blind,
            Club,
            Waddledee,
            Switchwingroute,
            Bumper,
            BlindMahoroa,
            RecoveryRing,
            Halfpoint
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
            StarL,
            NumberCoin
        }

        public enum ItemVariation
        {
            Normal,
            GoalActive,
            GongActive
        }

        #endregion

        #region Structs

        public struct Drum
        {
            public DrumKind Kind;
            public DrumVariation Variation;
            public DrumDirType Dir;
            public float Param;
            public uint ParamExA;
            public uint ParamExB;
            public uint ParamExC;
            public uint ParamExD;
            public GridPos X;
            public GridPos Y;
        }

        public struct Gimmick
        {
            public GimmickKind Kind;
            public uint Variation;
            public GimmickDirType Dir;
            public float Param;
            public uint IdentNo;
            public uint ParamExA;
            public uint ParamExB;
            public uint ParamExC;
            public uint ParamExD;
            public GridPos X;
            public GridPos Y;
        }

        public struct Item
        {
            public ItemKind Kind;
            public ItemVariation Variation;
            public int IdentNo;
            public int GroupNo;
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

        public uint Decoration = 5;
        public uint TimeLimit = 120;
        public bool Unknown = false;
        public string BGM = "NormalStage1";

        public MapDrumDashDeluxe()
        {
            XData.Version = new byte[] { 2, 0 };
            XData.Endianness = Endianness.Little;
        }

        public MapDrumDashDeluxe(EndianBinaryReader reader)
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
            uint drumSection = reader.ReadUInt32();
            uint listSection2 = reader.ReadUInt32();
            uint listSection3 = reader.ReadUInt32();

            uint headerEnd = reader.ReadUInt32();
            if (headerEnd != HEADER_END)
                throw new InvalidDataException($"Expected header to end with 0x{HEADER_END:X8}, got 0x{headerEnd:X8}");

            reader.BaseStream.Position = generalSection;
            Decoration = reader.ReadUInt32();
            TimeLimit = reader.ReadUInt32();
            Unknown = reader.ReadUInt32() != 0;
            BGM = reader.ReadStringOffset();

            reader.BaseStream.Position = drumSection;
            Drums = new List<Drum>();
            uint o1Count = reader.ReadUInt32();
            for (int i = 0; i < o1Count; i++)
            {
                Drum obj = new Drum();
                obj.Kind = (DrumKind)reader.ReadUInt32();
                obj.Variation = (DrumVariation)reader.ReadUInt32();
                obj.Dir = (DrumDirType)reader.ReadUInt32();
                obj.Param = reader.ReadSingle();
                obj.ParamExA = reader.ReadUInt32();
                obj.ParamExB = reader.ReadUInt32();
                obj.ParamExC = reader.ReadUInt32();
                obj.ParamExD = reader.ReadUInt32();
                obj.X = reader.ReadUInt32();
                obj.Y = reader.ReadUInt32();
                Drums.Add(obj);
            }

            reader.BaseStream.Position = listSection2;
            Gimmicks = new List<Gimmick>();
            uint o2Count = reader.ReadUInt32();
            for (int i = 0; i < o2Count; i++)
            {
                Gimmick obj = new Gimmick();
                obj.Kind = (GimmickKind)reader.ReadUInt32();
                obj.Variation = reader.ReadUInt32();
                obj.Dir = (GimmickDirType)reader.ReadUInt32();
                obj.Param = reader.ReadSingle();
                obj.IdentNo = reader.ReadUInt32();
                obj.ParamExA = reader.ReadUInt32();
                obj.ParamExB = reader.ReadUInt32();
                obj.ParamExC = reader.ReadUInt32();
                obj.ParamExD = reader.ReadUInt32();
                obj.X = reader.ReadUInt32();
                obj.Y = reader.ReadUInt32();
                Gimmicks.Add(obj);
            }

            reader.BaseStream.Position = listSection3;
            Items = new List<Item>();
            uint o3Count = reader.ReadUInt32();
            for (int i = 0; i < o3Count; i++)
            {
                Item obj = new Item();
                obj.Kind = (ItemKind)reader.ReadUInt32();
                obj.Variation = (ItemVariation)reader.ReadUInt32();
                obj.IdentNo = reader.ReadInt32();
                obj.GroupNo = reader.ReadInt32();
                obj.X = reader.ReadUInt32();
                obj.Y = reader.ReadUInt32();
                Items.Add(obj);
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
            writer.Write(Decoration);
            writer.Write(TimeLimit);
            writer.Write(Unknown ? 1 : 0);
            strings.Add(writer.BaseStream.Position, BGM);
            writer.Write(-1);
            writer.WritePadding(0x20);

            writer.WritePositionAt(headerStart + 0x8);
            writer.Write(Drums.Count);
            for (int i = 0; i < Drums.Count; i++)
            {
                var obj = Drums[i];
                writer.Write((uint)obj.Kind);
                writer.Write((uint)obj.Variation);
                writer.Write((uint)obj.Dir);
                writer.Write(obj.Param);
                writer.Write(obj.ParamExA);
                writer.Write(obj.ParamExB);
                writer.Write(obj.ParamExC);
                writer.Write(obj.ParamExD);
                writer.Write(obj.X);
                writer.Write(obj.Y);
            }
            writer.WritePadding(0x10);

            writer.WritePositionAt(headerStart + 0xC);
            writer.Write(Gimmicks.Count);
            for (int i = 0; i < Gimmicks.Count; i++)
            {
                var obj = Gimmicks[i];
                writer.Write((uint)obj.Kind);
                writer.Write(obj.Variation);
                writer.Write((uint)obj.Dir);
                writer.Write(obj.Param);
                writer.Write(obj.IdentNo);
                writer.Write(obj.ParamExA);
                writer.Write(obj.ParamExB);
                writer.Write(obj.ParamExC);
                writer.Write(obj.ParamExD);
                writer.Write(obj.X);
                writer.Write(obj.Y);
            }
            writer.WritePadding(0x10);

            writer.WritePositionAt(headerStart + 0x10);
            writer.Write(Items.Count);
            for (int i = 0; i < Items.Count; i++)
            {
                var obj = Items[i];
                writer.Write((uint)obj.Kind);
                writer.Write((uint)obj.Variation);
                writer.Write(obj.IdentNo);
                writer.Write(obj.GroupNo);
                writer.Write(obj.X);
                writer.Write(obj.Y);
            }

            strings.WriteAll(writer);

            XData.WriteFilesize(writer);
            XData.WriteFooter(writer);
        }
    }
}
