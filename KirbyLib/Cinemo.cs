using KirbyLib.IO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace KirbyLib
{
    /// <summary>
    /// Handles reading and writing Cinemo Dynamics (CND) files found in Kirby Fighters 2 and onward.<br/><br/>
    /// These files contain rendering and object placement information for scenes,<br/>
    /// but are sometimes used for parameters as well.
    /// </summary>
    public class Cinemo
    {
        /// <summary>
        /// A Cinemo object.<br/>
        /// Contains its name, type, and structs.
        /// </summary>
        public class CinemoObject
        {
            public string Name;
            public string Type;
            public List<CinemoStruct> Structs = new List<CinemoStruct>();
        }

        /// <summary>
        /// A struct located in Cinemo objects.<br/>
        /// Contains its name and variables.
        /// </summary>
        public class CinemoStruct
        {
            public string Name;
            public List<CinemoVariable> Variables = new List<CinemoVariable>();
        }

        public const ulong MAGIC_NUMBER = 10100;
        public const uint MAGIC_NUMBER_2 = 0x24;

        public XData XData { get; private set; } = new XData()
        {
            Version = new byte[] { 4, 0 },
            Endianness = Endianness.Little
        };

        /// <summary>
        /// The internal name of the Cinemo file.
        /// </summary>
        public string Name;
        /// <summary>
        /// The internal type of the Cinemo file.
        /// </summary>
        public string Type;

        public List<CinemoObject> VisualSection = new List<CinemoObject>();
        public List<CinemoObject> RenderSection = new List<CinemoObject>();

        public Cinemo() { }

        public Cinemo(EndianBinaryReader reader)
        {
            Read(reader);
        }

        public void Read(EndianBinaryReader reader)
        {
            XData.Read(reader);

            ulong magic = reader.ReadUInt64();
            if (magic != MAGIC_NUMBER)
                throw new InvalidDataException($"Expected magic {MAGIC_NUMBER}, got {magic}");

            uint magic2 = reader.ReadUInt32();
            if (magic2 != MAGIC_NUMBER_2)
                throw new InvalidDataException($"Expected second magic {MAGIC_NUMBER_2}, got {magic2}");

            uint visualSection = reader.ReadUInt32();
            if (reader.ReadUInt32() != 0)
                Console.WriteLine($"Cinemo file has non-zero value at 0x{reader.BaseStream.Position - 4:X8}");

            Name = reader.ReadStringOffset();
            Type = reader.ReadStringOffset();
            uint renderSection = reader.ReadUInt32();

            reader.BaseStream.Position = renderSection;
            RenderSection = ReadObjectSection(reader);

            reader.BaseStream.Position = visualSection;
            VisualSection = ReadObjectSection(reader);
        }

        List<CinemoObject> ReadObjectSection(EndianBinaryReader reader)
        {
            long sectionStart = reader.BaseStream.Position;
            List<CinemoObject> objects = new List<CinemoObject>();

            uint objCount = reader.ReadUInt32();
            for (int i = 0; i < objCount; i++)
            {
                reader.BaseStream.Position = sectionStart + 4 + (i * 4);
                reader.BaseStream.Position = reader.ReadUInt32();

                CinemoObject obj = new CinemoObject();
                obj.Name = reader.ReadStringOffset();
                obj.Type = reader.ReadStringOffset();
                if (reader.ReadInt32() != 0)
                    Console.WriteLine($"CinemoObject {i} has non-zero value at 0x8!");
                reader.BaseStream.Position = reader.ReadUInt32();

                obj.Structs = new List<CinemoStruct>();

                long structListStart = reader.BaseStream.Position;
                uint structCount = reader.ReadUInt32();
                for (int s = 0; s < structCount; s++)
                {
                    reader.BaseStream.Position = structListStart + 4 + (s * 4);
                    reader.BaseStream.Position = reader.ReadUInt32();

                    CinemoStruct cStr = new CinemoStruct();
                    cStr.Name = reader.ReadStringOffset();
                    reader.BaseStream.Position = reader.ReadUInt32();

                    cStr.Variables = new List<CinemoVariable>();

                    long varListStart = reader.BaseStream.Position;
                    uint varCount = reader.ReadUInt32();
                    for (int v = 0; v < varCount; v++)
                    {
                        reader.BaseStream.Position = varListStart + 4 + (v * 4);
                        reader.BaseStream.Position = reader.ReadUInt32();

                        CinemoVariable var = new CinemoVariable(reader);
                        cStr.Variables.Add(var);
                    }

                    obj.Structs.Add(cStr);
                }

                objects.Add(obj);
            }

            return objects;
        }

        public void Write(EndianBinaryWriter writer)
        {
            XData.WriteHeader(writer);

            StringHelperContainer strings = new StringHelperContainer();

            long header = writer.BaseStream.Position;

            writer.Write(MAGIC_NUMBER);
            writer.Write(MAGIC_NUMBER_2);
            writer.Write(-1);
            writer.Write(0);
            strings.Add(writer.BaseStream.Position, Name);
            writer.Write(-1);
            strings.Add(writer.BaseStream.Position, Type);
            writer.Write(-1);
            writer.Write(-1);

            writer.WritePositionAt(header + 0x1C);
            WriteObjectSection(writer, RenderSection, strings);

            writer.WritePositionAt(header + 0xC);
            WriteObjectSection(writer, VisualSection, strings);

            strings.WriteAll(writer);

            XData.WriteFilesize(writer);
            XData.WriteFooter(writer);
        }

        void WriteObjectSection(EndianBinaryWriter writer, List<CinemoObject> objects, StringHelperContainer strings)
        {
            long sectionStart = writer.BaseStream.Position;
            writer.Write(objects.Count);
            for (int i = 0; i < objects.Count; i++)
                writer.Write(-1);

            for (int i = 0; i < objects.Count; i++)
            {
                var cObj = objects[i];

                writer.WritePositionAt(sectionStart + 4 + (i * 4));

                strings.Add(writer.BaseStream.Position, cObj.Name);
                writer.Write(-1);
                strings.Add(writer.BaseStream.Position, cObj.Type);
                writer.Write(-1);
                writer.Write(0);
                writer.Write(writer.BaseStream.Position + 4);

                long structListStart = writer.BaseStream.Position;
                writer.Write(cObj.Structs.Count);
                for (int s = 0; s < cObj.Structs.Count; s++)
                    writer.Write(-1);

                for (int s = 0; s < cObj.Structs.Count; s++)
                {
                    var cStruct = cObj.Structs[s];

                    writer.WritePositionAt(structListStart + 4 + (s * 4));

                    strings.Add(writer.BaseStream.Position, cStruct.Name);
                    writer.Write(-1);
                    writer.Write(writer.BaseStream.Position + 4);

                    long varListStart = writer.BaseStream.Position;
                    writer.Write(cStruct.Variables.Count);
                    for (int v = 0; v < cStruct.Variables.Count; v++)
                        writer.Write(-1);

                    for (int v = 0; v < cStruct.Variables.Count; v++)
                    {
                        var var = cStruct.Variables[v];

                        writer.WritePositionAt(varListStart + 4 + (v * 4));

                        strings.Add(writer.BaseStream.Position, var.Name);
                        writer.Write(-1);
                        strings.Add(writer.BaseStream.Position, var.Type.ToString());
                        writer.Write(-1);
                        writer.Write(0);

                        var.WriteData(writer);
                    }
                }
            }
        }
    }

    public enum CinemoType : int
    {
        Int,
        Float,
        Bool,
        Color4,
        Vec3,
        String
    }

    /// <summary>
    /// A variable object found in a Cinemo file.<br/>
    /// Contains its type information as well as its data.
    /// </summary>
    public class CinemoVariable
    {
        public string Name;
        private CinemoType _type;

        private object _data;

        public CinemoType Type { get => _type; }

        public CinemoVariable(EndianBinaryReader reader)
        {
            Read(reader);
        }

        public CinemoVariable(object data)
        {
            SetValue(data);
        }

        public void Read(EndianBinaryReader reader)
        {
            Name = reader.ReadStringOffset();
            string typeString = reader.ReadStringOffset();
            if (!Enum.TryParse(typeString, out CinemoType type))
                throw new NotImplementedException($"Unimplemented CND Variable type \"{typeString}\"");

            _type = type;
            if (reader.ReadInt32() != 0)
                Console.WriteLine($"CinemoVariable {Name} has non-zero value at 0x8!");

            //We don't really need this
            int dataSize = reader.ReadInt32();

            switch (_type)
            {
                case CinemoType.Int:
                    _data = reader.ReadInt32();
                    break;
                case CinemoType.Float:
                    _data = reader.ReadSingle();
                    break;
                case CinemoType.Bool:
                    _data = reader.ReadInt32() != 0;
                    break;
                case CinemoType.Color4:
                    byte r = reader.ReadByte();
                    byte g = reader.ReadByte();
                    byte b = reader.ReadByte();
                    byte a = reader.ReadByte();
                    _data = Color.FromArgb(a, r, g, b);
                    break;
                case CinemoType.Vec3:
                    _data = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    break;
                case CinemoType.String:
                    _data = reader.ReadStringHAL();
                    break;
            }
        }

        public void WriteData(EndianBinaryWriter writer)
        {
            switch (_type)
            {
                default:
                    writer.Write(0);
                    break;
                case CinemoType.Int:
                    writer.Write(4);
                    writer.Write(AsInt());
                    break;
                case CinemoType.Float:
                    writer.Write(4);
                    writer.Write(AsFloat());
                    break;
                case CinemoType.Bool:
                    writer.Write(4);
                    writer.Write(AsBool() ? 1 : 0);
                    break;
                case CinemoType.Color4:
                    writer.Write(4);
                    Color color = AsColor4();
                    writer.Write(color.R);
                    writer.Write(color.G);
                    writer.Write(color.B);
                    writer.Write(color.A);
                    break;
                case CinemoType.Vec3:
                    writer.Write(0xC);
                    writer.Write(AsVec3());
                    break;
                case CinemoType.String:
                    writer.Write(0);
                    writer.WriteStringHAL(AsString());
                    break;
            }
        }

        public void SetValue(object data)
        {
            Type t = data.GetType();
            if (t == typeof(int))
                _type = CinemoType.Int;
            else if (t == typeof(float))
                _type = CinemoType.Float;
            else if (t == typeof(bool))
                _type = CinemoType.Bool;
            else if (t == typeof(Color))
                _type = CinemoType.Color4;
            else if (t == typeof(Vector3))
                _type = CinemoType.Vec3;
            else if (t == typeof(string))
                _type = CinemoType.String;
            else
                return;

            _data = data;
        }

        /// <summary>
        /// Returns the variable's value as a .NET object.
        /// </summary>
        public object GetValue()
        {
            return _data;
        }

        /// <summary>
        /// Returns the signed 32-bit integer value of the variable.<br/>If the type is incorrect, 0 is returned.
        /// </summary>
        public int AsInt()
        {
            if (_type == CinemoType.Int)
                return (int)_data;

            return 0;
        }

        /// <summary>
        /// Returns the signed 32-bit float value of the variable.<br/>If the type is incorrect, 0 is returned.
        /// </summary>
        public float AsFloat()
        {
            if (_type == CinemoType.Float)
                return (float)_data;

            return 0f;
        }

        /// <summary>
        /// Returns the boolean value of the variable.<br/>If the type is incorrect, false is returned.
        /// </summary>
        public bool AsBool()
        {
            if (_type == CinemoType.Bool)
                return (bool)_data;

            return false;
        }

        /// <summary>
        /// Returns the Color value of the variable.<br/>If the type is incorrect, a transparent black color is returned.
        /// </summary>
        public Color AsColor4()
        {
            if (_type == CinemoType.Color4)
                return (Color)_data;

            return Color.Transparent;
        }

        /// <summary>
        /// Returns the Vector3 value of the variable.<br/>If the type is incorrect, (0, 0, 0) is returned.
        /// </summary>
        public Vector3 AsVec3()
        {
            if (_type == CinemoType.Vec3)
                return (Vector3)_data;

            return Vector3.Zero;
        }

        /// <summary>
        /// Returns the string value of the variable.<br/>If the type is incorrect, an empty string is returned.
        /// </summary>
        public string AsString()
        {
            if (_type == CinemoType.String)
                return (string)_data;

            return "";
        }

        #region Casts

        public static explicit operator int(CinemoVariable variable)
        {
            if (variable.Type == CinemoType.Int)
                return variable.AsInt();

            throw new InvalidCastException("CinemoVariable type is not Int");
        }

        public static explicit operator float(CinemoVariable variable)
        {
            if (variable.Type == CinemoType.Float)
                return variable.AsFloat();

            throw new InvalidCastException("CinemoVariable type is not Float");
        }

        public static explicit operator bool(CinemoVariable variable)
        {
            if (variable.Type == CinemoType.Bool)
                return variable.AsBool();

            throw new InvalidCastException("CinemoVariable type is not Bool");
        }

        public static explicit operator string(CinemoVariable variable)
        {
            if (variable.Type == CinemoType.String)
                return variable.AsString();

            throw new InvalidCastException("CinemoVariable type is not String");
        }

        public static explicit operator Color(CinemoVariable variable)
        {
            if (variable.Type == CinemoType.Color4)
                return variable.AsColor4();

            throw new InvalidCastException("CinemoVariable type is not Color4");
        }

        public static explicit operator Vector3(CinemoVariable variable)
        {
            if (variable.Type == CinemoType.Vec3)
                return variable.AsVec3();

            throw new InvalidCastException("CinemoVariable type is not Vec3");
        }

        public static implicit operator CinemoVariable(int value)
        {
            return new CinemoVariable(value);
        }

        public static implicit operator CinemoVariable(float value)
        {
            return new CinemoVariable(value);
        }

        public static implicit operator CinemoVariable(bool value)
        {
            return new CinemoVariable(value);
        }

        public static implicit operator CinemoVariable(string value)
        {
            return new CinemoVariable(value);
        }

        public static implicit operator CinemoVariable(Color value)
        {
            return new CinemoVariable(value);
        }

        public static implicit operator CinemoVariable(Vector3 value)
        {
            return new CinemoVariable(value);
        }

        #endregion
    }
}
