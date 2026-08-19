using KirbyLib.Crypto;
using KirbyLib.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KirbyLib.Mint
{
    /// <summary>
    /// A representation of a Mint Module as found in Mint Archives.
    /// </summary>
    public class Module
    {
        public XData XData { get; private set; } = new XData();

        /// <summary>
        /// The name of the Module.
        /// </summary>
        public string Name;

        /// <summary>
        /// A hash of unknown use. 0xFFFFFFFF if unset.<br/>
        /// Only exists in Basil.
        /// </summary>
        public uint UnkHash = 0xFFFFFFFF;

        /// <summary>
        /// The format this Module uses when writing.
        /// </summary>
        public ModuleFormat Format { get; set; } = ModuleFormat.Mint;

        /// <summary>
        /// Raw bytes making up static constant data accessible by functions in the Module's Objects.
        /// </summary>
        public List<byte> SData = new List<byte>();
        /// <summary>
        /// External references. Each is an inverted CRC-32C hash.<br/>
        /// <b>Warning:</b> The game will crash if this list contains an XRef hash that does not exist.
        /// </summary>
        public List<uint> XRef = new List<uint>();
        /// <summary>
        /// A list of Objects in this Module.
        /// </summary>
        public List<MintObject> Objects = new List<MintObject>();

        public Module() { }

        public Module(EndianBinaryReader reader)
        {
            Read(reader);
        }

        public void Read(EndianBinaryReader reader)
        {
            XData.Read(reader);

            // Need this for later
            uint nameAddr = reader.ReadUInt32();
            reader.BaseStream.Position -= 4;

            Name = reader.ReadStringOffset();
            if (Format >= ModuleFormat.BasilKatFL)
                UnkHash = reader.ReadUInt32();

            uint sdataAddr = reader.ReadUInt32();
            uint xrefAddr = reader.ReadUInt32();
            uint objAddr = reader.ReadUInt32();
            if (Format >= ModuleFormat.BasilKatFL)
            {
                // Placeholder until I find a module that has this set to anything other than 0
                uint unk = reader.ReadUInt32();
                if (unk != 0)
                    Console.WriteLine($"Module {Name} unk value is {unk}");
            }

            reader.BaseStream.Position = sdataAddr;
            SData = reader.ReadBytes(reader.ReadInt32()).ToList();

            reader.BaseStream.Position = xrefAddr;
            XRef = new List<uint>();
            uint xrefCount = reader.ReadUInt32();
            for (int i = 0; i < xrefCount; i++)
                XRef.Add(reader.ReadUInt32());

            reader.BaseStream.Position = objAddr;
            Objects = new List<MintObject>();
            uint objCount = reader.ReadUInt32();
            for (uint i = 0; i < objCount; i++)
            {
                reader.BaseStream.Position = objAddr + 4 + (i * 4);
                uint thisObjAddr = reader.ReadUInt32();
                // Used for reading the final function with relative ease
                uint objEndAddr = i < objCount - 1 ? reader.ReadUInt32() : nameAddr;

                reader.BaseStream.Position = thisObjAddr;

                MintObject obj = new MintObject();
                obj.Name = reader.ReadStringOffset();
                uint objHash = reader.ReadUInt32(); // Inverted CRC32-C Object name hash, don't really need this because we can just calculate it whenever
                /*
                if (objHash != CRC32C.Calculate(obj.Name, XData.Endianness == Endianness.Big))
                    Console.WriteLine($"Warning: Object {obj.Name} has incorrect hash of {objHash:X8}, expected {CRC32C.Calculate(obj.Name, XData.Endianness == Endianness.Big):X8}");
                */
                uint varAddr = reader.ReadUInt32();
                uint funcAddr = reader.ReadUInt32();
                uint enumAddr = reader.ReadUInt32();

                uint implAddr = Format >= ModuleFormat.MintOld ? reader.ReadUInt32() : 0;
                uint extAddr = Format >= ModuleFormat.BasilKatFL ? reader.ReadUInt32() : 0;
                obj.Type = (ObjectType)reader.ReadByte();
                obj.Flags = reader.ReadByte();

                // In Basil, sections that don't exist (by having no entries) have offsets of 0
                if (varAddr > 0)
                {
                    reader.BaseStream.Position = varAddr;
                    uint varCount = reader.ReadUInt32();
                    for (int v = 0; v < varCount; v++)
                    {
                        reader.BaseStream.Position = varAddr + 4 + (v * 4);
                        reader.BaseStream.Position = reader.ReadUInt32();

                        string name = reader.ReadStringOffset();
                        uint hash = reader.ReadUInt32(); // Inverted CRC32-C variable name hash, can be calculated whenever
                        /*
                        if (hash != CRC32C.Calculate($"{obj.Name}.{name}", XData.Endianness == Endianness.Big))
                            Console.WriteLine($"Warning: Variable {obj.Name}.{name} has incorrect hash of {hash:X8}, expected {CRC32C.Calculate($"{obj.Name}.{name}", XData.Endianness == Endianness.Big):X8}");
                        */
                        string type = reader.ReadStringOffset();

                        MintVariable variable = new MintVariable(type, name);
                        variable.Flags = reader.ReadByte();

                        obj.Variables.Add(variable);
                    }
                }

                if (funcAddr > 0)
                {
                    // Get end address of function data block for very easy reading
                    uint fallbackNextSection = enumAddr;
                    if (fallbackNextSection == 0)
                        fallbackNextSection = implAddr;
                    if (fallbackNextSection == 0)
                        fallbackNextSection = extAddr;
                    if (fallbackNextSection == 0)
                        fallbackNextSection = objEndAddr;

                    reader.BaseStream.Position = funcAddr;
                    uint funcCount = reader.ReadUInt32();
                    for (uint f = 0; f < funcCount; f++)
                    {
                        reader.BaseStream.Position = funcAddr + 4 + (f * 4);
                        uint addr = reader.ReadUInt32();

                        uint endAddr = f < funcCount - 1 ? reader.ReadUInt32() : fallbackNextSection;
                        reader.BaseStream.Position = addr;

                        string name = reader.ReadStringOffset();
                        MintFunction func = new MintFunction(name);
                        uint hash = reader.ReadUInt32(); // Inverted CRC32-C function name hash, can just be calculated whenever so again we don't really need it
                        /*
                        if (hash != CRC32C.Calculate($"{obj.Name}.{func.NameWithoutType()}", XData.Endianness == Endianness.Big))
                            Console.WriteLine($"Warning: Function {obj.Name}.{func.NameWithoutType()} has incorrect hash of {hash:X8}, expected {CRC32C.Calculate($"{obj.Name}.{func.NameWithoutType()}", XData.Endianness == Endianness.Big):X8}");
                        */

                        if (Format >= ModuleFormat.BasilKatFL)
                        {
                            func.Arguments = reader.ReadUInt32();
                            func.Registers = reader.ReadUInt32();
                        }

                        uint dataAddr = reader.ReadUInt32();
                        if (Format >= ModuleFormat.Mint)
                            func.Flags = reader.ReadUInt32();

                        reader.BaseStream.Position = dataAddr;
                        func.Data = reader.ReadBytes((int)(endAddr - dataAddr));

                        obj.Functions.Add(func);
                    }
                }

                if (enumAddr > 0)
                {
                    reader.BaseStream.Position = enumAddr;
                    uint enumCount = reader.ReadUInt32();
                    for (uint e = 0; e < enumCount; e++)
                    {
                        reader.BaseStream.Position = enumAddr + 4 + (e * 4);
                        reader.BaseStream.Position = reader.ReadUInt32();

                        string name = reader.ReadStringOffset();
                        int value = reader.ReadInt32();

                        MintEnum mintEnum = new MintEnum(name, value);

                        if (Format >= ModuleFormat.Basil)
                            mintEnum.Flags = reader.ReadUInt32();

                        obj.Enums.Add(mintEnum);
                    }
                }

                if (implAddr > 0)
                {
                    reader.BaseStream.Position = implAddr;
                    int implCount = reader.ReadInt32();
                    for (int m = 0; m < implCount; m++)
                    {
                        obj.Implements.Add(XRef[reader.ReadUInt16()]);
                    }
                }

                if (extAddr > 0)
                {
                    reader.BaseStream.Position = extAddr;
                    int extCount = reader.ReadInt32();
                    for (int e = 0; e < extCount; e++)
                    {
                        obj.Extends.Add(reader.ReadBytes(4));
                    }
                }

                Objects.Add(obj);
            }
        }

        public void Write(EndianBinaryWriter writer)
        {
            StringHelperContainer strings = new StringHelperContainer();

            XData.WriteHeader(writer);

            bool isBigEndian = XData.Endianness == Endianness.Big;

            strings.Add(writer.BaseStream.Position, Name);
            writer.Write(-1);
            if (Format >= ModuleFormat.BasilKatFL)
                writer.Write(UnkHash);

            long headerStart = writer.BaseStream.Position;
            writer.Write(-1);
            writer.Write(-1);
            writer.Write(-1);
            if (Format >= ModuleFormat.BasilKatFL)
                writer.Write(0);

            writer.WritePositionAt(headerStart);
            writer.Write(SData.Count);
            writer.Write(SData.ToArray());
            writer.WritePadding();

            writer.WritePositionAt(headerStart + 4);
            writer.Write(XRef.Count);
            for (int i = 0; i < XRef.Count; i++)
                writer.Write(XRef[i]);

            long objListStart = writer.BaseStream.Position;
            writer.WritePositionAt(headerStart + 8);
            writer.Write(Objects.Count);
            for (int i = 0; i < Objects.Count; i++)
                writer.Write(-1);

            for (int i = 0; i < Objects.Count; i++)
            {
                MintObject obj = Objects[i];

                long objPos = writer.BaseStream.Position;
                writer.WritePositionAt(objListStart + 4 + (i * 4));

                strings.Add(writer.BaseStream.Position, obj.Name);
                writer.Write(-1);
                writer.Write(CRC32C.Calculate(obj.Name, isBigEndian));
                writer.Write(0);
                writer.Write(0);
                writer.Write(0);
                if (Format >= ModuleFormat.MintOld)
                    writer.Write(0);
                if (Format >= ModuleFormat.BasilKatFL)
                    writer.Write(0);
                writer.Write((byte)obj.Type);
                writer.Write(obj.Flags);
                writer.Align();

                long varListStart = writer.BaseStream.Position;
                writer.WritePositionAt(objPos + 0x8);
                writer.Write(obj.Variables.Count);
                for (int j = 0; j < obj.Variables.Count; j++)
                    writer.Write(-1);

                for (int j = 0; j < obj.Variables.Count; j++)
                {
                    MintVariable var = obj.Variables[j];
                    var.Name = var.Name.Trim();
                    var.Type = var.Type.Trim();

                    writer.WritePositionAt(varListStart + 4 + (j * 4));

                    strings.Add(writer.BaseStream.Position, var.Name);
                    writer.Write(-1);
                    writer.Write(CRC32C.Calculate($"{obj.Name}.{var.Name}", isBigEndian));
                    strings.Add(writer.BaseStream.Position, var.Type);
                    writer.Write(-1);
                    writer.Write(var.Flags);
                    writer.Align();
                }

                long funcListStart = writer.BaseStream.Position;
                writer.WritePositionAt(objPos + 0xC);
                writer.Write(obj.Functions.Count);
                for (int j = 0; j < obj.Functions.Count; j++)
                    writer.Write(-1);

                for (int j = 0; j < obj.Functions.Count; j++)
                {
                    MintFunction func = obj.Functions[j];

                    writer.WritePositionAt(funcListStart + 4 + (j * 4));

                    strings.Add(writer.BaseStream.Position, func.Name);
                    writer.Write(-1);
                    writer.Write(CRC32C.Calculate($"{obj.Name}.{func.NameWithoutType()}", isBigEndian));
                    if (Format >= ModuleFormat.BasilKatFL)
                    {
                        writer.Write(func.Arguments);
                        writer.Write(func.Registers);
                    }
                    long dataAddr = writer.BaseStream.Position;
                    writer.Write(-1);
                    if (Format >= ModuleFormat.Mint)
                        writer.Write(func.Flags);

                    writer.WritePositionAt(dataAddr);
                    writer.Write(func.Data);
                    writer.WritePadding();
                }

                long enumListStart = writer.BaseStream.Position;
                writer.WritePositionAt(objPos + 0x10);
                writer.Write(obj.Enums.Count);
                for (int j = 0; j < obj.Enums.Count; j++)
                    writer.Write(-1);

                for (int j = 0; j < obj.Enums.Count; j++)
                {
                    MintEnum _enum = obj.Enums[j];

                    writer.WritePositionAt(enumListStart + 4 + (j * 4));

                    strings.Add(writer.BaseStream.Position, _enum.Name);
                    writer.Write(-1);
                    writer.Write(_enum.Value);

                    if (Format >= ModuleFormat.Basil)
                        writer.Write(_enum.Flags);
                }

                if (Format >= ModuleFormat.MintOld)
                {
                    writer.WritePositionAt(objPos + 0x14);
                    writer.Write(obj.Implements.Count);
                    for (int j = 0; j < obj.Implements.Count; j++)
                        writer.Write((ushort)XRef.IndexOf(obj.Implements[j]));
                    writer.WritePadding();
                }

                if (Format >= ModuleFormat.BasilKatFL)
                {
                    writer.WritePositionAt(objPos + 0x18);
                    writer.Write(obj.Extends.Count);
                    for (int j = 0; j < obj.Extends.Count; j++)
                        writer.Write(obj.Extends[j]);
                    writer.WritePadding();
                }
            }

            strings.WriteAll(writer);

            XData.WriteFilesize(writer);
            XData.WriteFooter(writer);
        }

        /// <summary>
        /// Returns true if the given Object exists.
        /// </summary>
        public bool ObjectExists(string name)
        {
            for (int i = 0; i < Objects.Count; i++)
            {
                if (Objects[i].Name == name)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Gets an Object by name. Returns null if it does not exist.
        /// </summary>
        public MintObject GetObject(string name)
        {
            for (int i = 0; i < Objects.Count; i++)
            {
                if (Objects[i].Name == name)
                    return Objects[i];
            }

            return null;
        }

        public MintObject this[int index] => Objects[index];

        public MintObject this[string name] => GetObject(name);
    }
}
