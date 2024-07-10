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
                    string[] writeOrder = dict.Keys.Order().ToArray();

                    // Prepare the entire block first before writing anything else
                    for (int i = 0; i < dict.Count; i++)
                    {
                        writer.Write(-1);
                        writer.Write(-1);
                    }

                    if (yaml.Version >= 4)
                    {
                        for (int i = 0; i < writeOrder.Length; i++)
                            writer.Write(keyList.IndexOf(writeOrder[i]));
                    }

                    for (int i = 0; i < writeOrder.Length; i++)
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
