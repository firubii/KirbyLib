using KirbyLib.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KirbyLib
{
    internal class YamlWriter
    {
        Yaml yaml;
        EndianBinaryWriter writer;
        StringHelperContainer strings = new StringHelperContainer();

        public YamlWriter(Yaml yaml, EndianBinaryWriter writer)
        {
            this.yaml = yaml;
            this.writer = writer;
        }

        // Why does HAL sort like this??? If this isn't sorted like this the game doesn't like the file and reads it wrong for some reason
        private static int StringCompare(string a, string b)
        {
            if (char.IsUpper(a[0]) && char.IsLower(b[0]))
                return -1;
            else if(char.IsUpper(b[0]) && char.IsLower(a[0]))
                return 1;

            return string.Compare(a, b);
        }

        public void Write()
        {
            yaml.XData.WriteHeader(writer);

            writer.Write(Yaml.YAML_MAGIC.ToCharArray());
            writer.Write(yaml.Version);
            WriteYamlNode(yaml.Root);

            if (yaml.Version >= 5)
                strings.WriteAllRelative(writer);
            else
                strings.WriteAll(writer);

            yaml.XData.WriteFilesize(writer);
            yaml.XData.WriteFooter(writer);
        }

        public void WriteYamlNode(YamlNode node)
        {
            writer.Write((int)node.Type);
            switch (node.Type)
            {
                case YamlType.Int:
                    writer.Write(node.AsInt());
                    break;
                case YamlType.Float:
                    writer.Write(node.AsFloat());
                    break;
                case YamlType.Bool:
                    writer.Write(node.AsBool() ? 1 : 0);
                    break;
                case YamlType.String:
                    strings.Add(new StringHelper(writer.BaseStream.Position, node.AsString()));
                    writer.Write(-1);
                    break;
                case YamlType.Hash:
                    var dict = node.AsHash();
                    writer.Write(dict.Count);
                    long listStart = writer.BaseStream.Position;

                    List<string> keyList = dict.Keys.ToList();
                    List<string> writeOrder = dict.Keys.ToList();
                    writeOrder.Sort(StringCompare);

                    // Prepare the entire block first before writing anything else
                    for (int i = 0; i < dict.Count; i++)
                    {
                        writer.Write(-1);
                        writer.Write(-1);
                    }

                    if (yaml.Version >= 4)
                    {
                        for (int i = 0; i < keyList.Count; i++)
                            writer.Write(writeOrder.IndexOf(keyList[i]));
                    }

                    for (int i = 0; i < writeOrder.Count; i++)
                    {
                        strings.Add(new StringHelper(listStart + (i * 8), writeOrder[i]));

                        long writeAddr = listStart + (i * 8) + 4;

                        // Offsets are relative in version 5 for some reason
                        if (yaml.Version >= 5)
                            writer.WriteRelativePositionAt(writeAddr);
                        else
                            writer.WritePositionAt(writeAddr);

                        WriteYamlNode(dict[writeOrder[i]]);
                    }

                    break;
                case YamlType.Array:
                    var array = node.AsList();
                    writer.Write(array.Count);

                    long arrayStart = writer.BaseStream.Position;

                    // Prepare the entire block first before writing anything else
                    for (int i = 0; i < array.Count; i++)
                        writer.Write(-1);

                    for (int i = 0; i < array.Count; i++)
                    {
                        long writeAddr = arrayStart + (i * 4);

                        // Offsets are relative in version 5 for some reason
                        if (yaml.Version >= 5)
                            writer.WriteRelativePositionAt(writeAddr);
                        else
                            writer.WritePositionAt(writeAddr);

                        WriteYamlNode(array[i]);
                    }

                    break;
            }
        }
    }
}
