using KirbyLib.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace KirbyLib
{
    /// <summary>
    /// A binary Yaml file.<br/>
    /// Used both within Map data and as standalone parameter files.
    /// </summary>
    public class Yaml
    {
        public const string YAML_MAGIC = "YAML";

        public XData XData { get; private set; } = new XData();

        /// <summary>
        /// Yaml file version. Determines how the data is saved and read.
        /// </summary>
        public uint Version;

        /// <summary>
        /// The initial root node of the Yaml file.
        /// </summary>
        public YamlNode Root;

        public Yaml() { }

        public Yaml(EndianBinaryReader reader)
        {
            Read(reader);
        }

        public void Read(EndianBinaryReader reader)
        {
            XData.Read(reader);

            string magic = Encoding.UTF8.GetString(reader.ReadBytes(4));
            if (magic != YAML_MAGIC)
                throw new InvalidDataException("Yaml magic \"YAML\" not found!");

            Version = reader.ReadUInt32();

            Root = new YamlNode(reader, Version);
        }

        public void Write(EndianBinaryWriter writer)
        {
            new YamlWriter(this, writer).Write();
        }

        public override string ToString()
        {
            return Root.ToString();
        }
    }

    public enum YamlType : int
    {
        Invalid,
        Int,
        Float,
        Bool,
        String,
        Hash,
        Array
    }

    /// <summary>
    /// A singular Yaml node. Contains its Type information as well as its data.
    /// </summary>
    public class YamlNode : IEnumerable<YamlNode>, IEnumerable<string>, IEnumerable<KeyValuePair<string, YamlNode>>
    {
        private YamlType _type;
        private object? data;

        public YamlType Type { get => _type; }

        /// <summary>
        /// The amount of children of the node.
        /// </summary>
        public int Length
        {
            get
            {
                if (Type == YamlType.Array)
                    return AsList().Count;
                if (Type == YamlType.Hash)
                    return AsHash().Count;

                return 0;
            }
        }

        public YamlNode()
        {
            _type = YamlType.Invalid;
            data = null;
        }

        public YamlNode(EndianBinaryReader reader, uint version)
        {
            Read(reader, version);
        }

        public YamlNode(object data)
        {
            SetValue(data);
        }

        public void Read(EndianBinaryReader reader, uint version)
        {
            _type = (YamlType)reader.ReadInt32();
            switch (Type)
            {
                case YamlType.Invalid:
                    data = null;
                    break;
                case YamlType.Int:
                    data = reader.ReadInt32();
                    break;
                case YamlType.Float:
                    data = reader.ReadSingle();
                    break;
                case YamlType.Bool:
                    data = reader.ReadInt32() != 0;
                    break;
                case YamlType.String:
                    {
                        uint offs = 0;
                        if (version >= 5)
                            offs = (uint)reader.BaseStream.Position;

                        data = reader.ReadStringOffset(offs);
                        break;
                    }
                case YamlType.Hash:
                    {
                        int count = reader.ReadInt32();
                        long listStart = reader.BaseStream.Position;

                        int[] readOrder = new int[count];
                        if (version >= 4)
                        {
                            reader.BaseStream.Position = listStart + (count * 8);
                            for (int i = 0; i < count; i++)
                                readOrder[i] = reader.ReadInt32();
                        }
                        else
                        {
                            for (int i = 0; i < count; i++)
                                readOrder[i] = i;
                        }

                        reader.BaseStream.Position = listStart;
                        Dictionary<string, YamlNode> nodes = new Dictionary<string, YamlNode>();
                        for (int i = 0; i < count; i++)
                        {
                            reader.BaseStream.Position = listStart + (readOrder[i] * 8);

                            uint offs = 0;
                            if (version >= 5)
                                offs = (uint)reader.BaseStream.Position;

                            string name = reader.ReadStringOffset(offs);

                            if (version >= 5)
                                offs = (uint)reader.BaseStream.Position;

                            uint dataOffset = reader.ReadUInt32() + offs;

                            reader.BaseStream.Position = dataOffset;
                            nodes.Add(name, new YamlNode(reader, version));
                        }

                        reader.BaseStream.Position = listStart + (count * 8) + (count * 4);

                        data = nodes;
                        break;
                    }
                case YamlType.Array:
                    {
                        int count = reader.ReadInt32();

                        long listStart = reader.BaseStream.Position;
                        List<YamlNode> nodes = new List<YamlNode>();
                        for (int i = 0; i < count; i++)
                        {
                            reader.BaseStream.Position = listStart + (i * 4);

                            uint offs = 0;
                            if (version >= 5)
                                offs = (uint)reader.BaseStream.Position;

                            uint dataOffset = reader.ReadUInt32() + offs;

                            reader.BaseStream.Position = dataOffset;
                            nodes.Add(new YamlNode(reader, version));
                        }

                        reader.BaseStream.Position = listStart + (count * 4);

                        data = nodes;
                        break;
                    }
            }
        }

        /// <summary>
        /// Returns the value of the node as a .NET object.
        /// </summary>
        public object? GetValue()
        {
            return data;
        }

        /// <summary>
        /// Sets the value of the Yaml node.<br/>
        /// </summary>
        public void SetValue(object data)
        {
            Type t = data.GetType();
            if (t == typeof(int))
                _type = YamlType.Int;
            else if (t == typeof(float))
                _type = YamlType.Float;
            else if (t == typeof(bool))
                _type = YamlType.Bool;
            else if (t == typeof(string))
                _type = YamlType.String;
            else if (t == typeof(Dictionary<string, YamlNode>))
                _type = YamlType.Hash;
            else if (t == typeof(List<YamlNode>))
                _type = YamlType.Array;
            else
            {
                _type = YamlType.Invalid;
                this.data = null;
                return;
            }

            this.data = data;
        }

        /// <summary>
        /// Returns the signed 32-bit integer value of the node.<br/>If the type is incorrect, 0 is returned.
        /// </summary>
        public int AsInt()
        {
            if (Type == YamlType.Int)
                return (int)data;

            return 0;
        }

        /// <summary>
        /// Returns the 32-bit float value of the node.<br/>If the type is incorrect, 0.0 is returned.
        /// </summary>
        public float AsFloat()
        {
            if (Type == YamlType.Float)
                return (float)data;

            return 0;
        }

        /// <summary>
        /// Returns the boolean value of the node.<br/>If the type is incorrect, false is returned.
        /// </summary>
        public bool AsBool()
        {
            if (Type == YamlType.Bool)
                return (bool)data;

            return false;
        }

        /// <summary>
        /// Returns the string value of the node.<br/>If the type is incorrect, an empty string is returned.
        /// </summary>
        public string AsString()
        {
            if (Type == YamlType.String)
                return (string)data;

            return "";
        }

        /// <summary>
        /// Returns the node's children as a Dictionary.<br/>If the type is incorrect, null is returned.
        /// </summary>
        public Dictionary<string, YamlNode> AsHash()
        {
            if (Type == YamlType.Hash)
                return data as Dictionary<string, YamlNode>;

            return null;
        }

        /// <summary>
        /// Returns the node's children as a List.<br/>If the type is incorrect, null is returned.
        /// </summary>
        public List<YamlNode> AsList()
        {
            if (Type == YamlType.Array)
                return data as List<YamlNode>;

            return null;
        }

        public void Add(YamlNode node)
        {
            if (Type == YamlType.Array)
            {
                var list = AsList();
                list.Add(node);
                data = list;
            }
        }

        public void Add(string key, YamlNode node)
        {
            if (Type == YamlType.Hash)
            {
                var dict = AsHash();
                dict.Add(key, node);
                data = dict;
            }
        }

        public void Remove(int index)
        {
            if (index >= 0 && index < Length)
            {
                if (Type == YamlType.Array)
                {
                    var list = AsList();
                    list.RemoveAt(index);
                    data = list;
                }
                else if (Type == YamlType.Hash)
                {
                    var dict = AsHash();
                    dict.Remove(Key(index));
                    data = dict;
                }
            }
        }

        public void Remove(string key)
        {
            if (Type == YamlType.Hash)
            {
                var dict = AsHash();
                if (dict.ContainsKey(key))
                {
                    dict.Remove(key);
                    data = dict;
                }
            }
        }

        public bool ContainsKey(string key)
        {
            if (Type == YamlType.Hash)
            {
                var dict = AsHash();
                return dict.ContainsKey(key);
            }

            return false;
        }

        public string Key(int index)
        {
            if (Type == YamlType.Hash && index >= 0 && index < Length)
                return AsHash().ElementAt(index).Key;

            return null;
        }

        /// <summary>
        /// Returns an array of the keys of the node's children.<br/>If the type is incorrect, null is returned.
        /// </summary>
        public string[] GetKeys()
        {
            if (Type == YamlType.Hash)
            {
                string[] keys = new string[Length];
                for (int i = 0; i < keys.Length; i++)
                    keys[i] = Key(i);

                return keys;
            }

            return null;
        }

        public override string ToString()
        {
            if (Type == YamlType.Hash)
            {
                string oStr = "{ ";
                using (var iterator = GetHashEnumerator())
                {
                    bool first = true;
                    while (iterator.MoveNext())
                    {
                        if (!first)
                            oStr += ", ";

                        var current = iterator.Current;
                        oStr += "\"";
                        oStr += current.Key;
                        oStr += "\": ";

                        if (current.Value.Type == YamlType.String)
                            oStr += "\"";

                        oStr += current.Value.ToString();

                        if (current.Value.Type == YamlType.String)
                            oStr += "\"";

                        first = false;
                    }
                }

                oStr += " }";

                return oStr;
            }
            else if (Type == YamlType.Array)
            {
                string oStr = "[ ";
                using (var iterator = GetEnumerator())
                {
                    bool first = true;
                    while (iterator.MoveNext())
                    {
                        if (!first)
                            oStr += ", ";

                        var current = iterator.Current;

                        if (current.Type == YamlType.String)
                            oStr += "\"";

                        oStr += current.ToString();

                        if (current.Type == YamlType.String)
                            oStr += "\"";

                        first = false;
                    }
                }

                oStr += " ]";

                return oStr;
            }

            return data.ToString();
        }

        /// <summary>
        /// Iterates through each child node, returning it and its name.
        /// </summary>
        public IEnumerator<KeyValuePair<string, YamlNode>> GetHashEnumerator()
        {
            if (Type == YamlType.Hash)
            {
                var dict = AsHash();
                for (int i = 0; i < Length; i++)
                    yield return dict.ElementAt(i);
            }

            yield break;
        }

        /// <summary>
        /// Iterates through each child node, returning its name.
        /// </summary>
        public IEnumerator<string> GetKeyEnumerator()
        {
            if (Type == YamlType.Hash)
            {
                for (int i = 0; i < Length; i++)
                    yield return Key(i);
            }

            yield break;
        }

        /// <summary>
        /// Iterates through each child node.
        /// </summary>
        public IEnumerator<YamlNode> GetEnumerator()
        {
            if (Type == YamlType.Hash || Type == YamlType.Array)
            {
                for (int i = 0; i < Length; i++)
                    yield return this[i];
            }

            yield break;
        }

        IEnumerator<string> IEnumerable<string>.GetEnumerator()
        {
            return GetKeyEnumerator();
        }

        IEnumerator<KeyValuePair<string, YamlNode>> IEnumerable<KeyValuePair<string, YamlNode>>.GetEnumerator()
        {
            return GetHashEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public YamlNode this[int index]
        {
            get
            {
                if (index >= 0 && index < Length)
                {
                    if (Type == YamlType.Array)
                        return AsList()[index];
                    if (Type == YamlType.Hash)
                        return AsHash().ElementAt(index).Value;
                }

                return null;
            }
            set
            {
                if (Type == YamlType.Array)
                    AsList()[index] = value;
            }
        }

        public YamlNode this[string key]
        {
            get
            {
                if (Type == YamlType.Hash)
                {
                    var dict = AsHash();
                    if (dict.ContainsKey(key))
                        return dict[key];
                }

                return null;
            }
            set
            {
                if (Type == YamlType.Hash)
                {
                    var dict = AsHash();
                    if (dict.ContainsKey(key))
                        dict[key] = value;
                    else
                        dict.Add(key, value);
                }
            }
        }

        #region Casts

        public static explicit operator int(YamlNode yaml)
        {
            if (yaml.Type == YamlType.Int)
                return yaml.AsInt();

            throw new InvalidCastException("YamlNode type is not Int");
        }

        public static explicit operator float(YamlNode yaml)
        {
            if (yaml.Type == YamlType.Float)
                return yaml.AsFloat();

            throw new InvalidCastException("YamlNode type is not Float");
        }

        public static explicit operator bool(YamlNode yaml)
        {
            if (yaml.Type == YamlType.Bool)
                return yaml.AsBool();

            throw new InvalidCastException("YamlNode type is not Bool");
        }

        public static explicit operator string(YamlNode yaml)
        {
            if (yaml.Type == YamlType.String)
                return yaml.AsString();

            throw new InvalidCastException("YamlNode type is not String");
        }

        public static explicit operator Dictionary<string, YamlNode>(YamlNode yaml)
        {
            if (yaml.Type == YamlType.Hash)
                return yaml.AsHash();

            throw new InvalidCastException("YamlNode type is not Hash");
        }

        public static explicit operator List<YamlNode>(YamlNode yaml)
        {
            if (yaml.Type == YamlType.Hash)
                return yaml.AsList();

            throw new InvalidCastException("YamlNode type is not Array");
        }

        public static implicit operator YamlNode(int value)
        {
            return new YamlNode(value);
        }

        public static implicit operator YamlNode(float value)
        {
            return new YamlNode(value);
        }

        public static implicit operator YamlNode(bool value)
        {
            return new YamlNode(value);
        }

        public static implicit operator YamlNode(string value)
        {
            return new YamlNode(value);
        }

        public static implicit operator YamlNode(Dictionary<string, YamlNode> value)
        {
            return new YamlNode(value);
        }

        public static implicit operator YamlNode(List<YamlNode> value)
        {
            return new YamlNode(value);
        }

        #endregion
    }
}
