using KirbyLib.Crypto;
using KirbyLib.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KirbyLib
{
    /// <summary>
    /// File format for scene preload information.
    /// </summary>
    public class FDG
    {
        /// <summary>
        /// Information about a single scene that contains information on which assets to load.
        /// </summary>
        public struct Scene
        {
            /// <summary>
            /// The name of the scene.
            /// </summary>
            public string Name;
            /// <summary>
            /// A list of scenes that this scene will also load.
            /// </summary>
            public List<string> Dependencies;
            /// <summary>
            /// The list of assets that this scene will load.
            /// </summary>
            public List<string> Assets;
        }

        public const uint FDG_MAGIC = 0x46444748; //"FDGH" in hex; unlike other formats, this can't be a char array

        public XData XData { get; private set; } = new XData();

        public List<Scene> Scenes = new List<Scene>();
        public List<string> Files = new List<string>();

        /// <summary>
        /// FDG Version.<br/>
        /// <list type="bullet">
        ///     <item><b>2</b>: 32-bit string offsets.<br/>Used until Kirby Star Allies.</item>
        ///     <item><b>3</b>: 64-bit string offsets. Strings are also hashed with FNV-1a.<br/>Used starting with Kirby and the Forgotten Land and later.</item>
        /// </list>
        /// </summary>
        public int Version = 2;

        public FDG() { }

        public FDG(EndianBinaryReader reader)
        {
            Read(reader);
        }

        public void Read(EndianBinaryReader reader)
        {
            XData.Read(reader);

            uint magic = reader.ReadUInt32();
            if (magic != FDG_MAGIC)
                throw new InvalidDataException("FDG magic \"FDGH\" not found!");

            Version = reader.ReadInt32();

            uint fileSection = reader.ReadUInt32();
            uint sceneSection = reader.ReadUInt32();
            uint stringSection = reader.ReadUInt32();

            reader.BaseStream.Position = stringSection;
            List<string> strings = new List<string>();
            if (Version > 2)
            {
                uint count = reader.ReadUInt32();
                for (uint i = 0; i < count; i++)
                {
                    //The first entry is the string's FNV-1a hash, we don't need it when reading because we can just calculate it later
                    reader.BaseStream.Position = stringSection + 8 + (i * 0x10) + 8;
                    strings.Add(reader.ReadStringOffset());
                }
            }
            else
            {
                int count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    reader.BaseStream.Position = stringSection + 4 + (i * 4);
                    strings.Add(reader.ReadStringOffset());
                }
            }

            reader.BaseStream.Position = fileSection;
            Files = new List<string>();
            uint fileCount = reader.ReadUInt32();
            for (uint i = 0; i < fileCount; i++)
                Files.Add(strings[reader.ReadInt32()]);

            reader.BaseStream.Position = sceneSection;
            Scenes = new List<Scene>();
            uint sceneCount = reader.ReadUInt32();
            for (uint i = 0; i < sceneCount; i++)
            {
                Scene scene = new Scene();

                reader.BaseStream.Position = sceneSection + 4 + (i * 0xC);

                scene.Name = reader.ReadStringOffset();
                uint deps = reader.ReadUInt32();
                uint assets = reader.ReadUInt32();

                reader.BaseStream.Position = deps;
                scene.Dependencies = new List<string>();
                uint depCount = reader.ReadUInt32();
                for (uint d = 0; d < depCount; d++)
                {
                    reader.BaseStream.Position = deps + 4 + (d * 4);

                    uint depIdx = reader.ReadUInt32();
                    // Dependency references are stored as indexes into the scenes section
                    reader.BaseStream.Position = sceneSection + 4 + (depIdx * 0xC);
                    scene.Dependencies.Add(reader.ReadStringOffset());
                }

                reader.BaseStream.Position = assets;
                scene.Assets = new List<string>();
                uint assetCount = reader.ReadUInt32();
                for (uint a = 0; a < assetCount; a++)
                {
                    // Asset references are stored as indexes into the string section
                    scene.Assets.Add(strings[reader.ReadInt32()]);
                }

                Scenes.Add(scene);
            }
        }

        public void Write(EndianBinaryWriter writer)
        {
            XData.WriteHeader(writer);

            StringHelperContainer strings = new StringHelperContainer();

            long headerStart = writer.BaseStream.Position;

            writer.Write(FDG_MAGIC);
            writer.Write(Version);
            writer.Write(-1);
            writer.Write(-1);
            writer.Write(-1);

            List<string> stringList = new List<string>();
            for (int i = 0; i < Scenes.Count; i++)
            {
                foreach (string str in Scenes[i].Dependencies)
                {
                    if (!stringList.Contains(str))
                        stringList.Add(str);
                }
                foreach (string str in Scenes[i].Assets)
                {
                    if (!stringList.Contains(str))
                        stringList.Add(str);
                }
            }

            for (int i = 0; i < Files.Count; i++)
            {
                if (!stringList.Contains(Files[i]))
                    stringList.Add(Files[i]);
            }

            writer.WritePositionAt(headerStart + 0x8);
            writer.Write(Files.Count);
            for (int i = 0; i < Files.Count; i++)
                writer.Write(stringList.IndexOf(Files[i]));

            long sceneSection = writer.BaseStream.Position;
            writer.WritePositionAt(headerStart + 0xC);
            writer.Write(Scenes.Count);
            for (int i = 0; i < Scenes.Count; i++)
            {
                writer.Write(-1);
                writer.Write(-1);
                writer.Write(-1);
            }

            for (int i = 0; i < Scenes.Count; i++)
            {
                var scene = Scenes[i];

                long scenePos = sceneSection + 4 + (i * 0xC);
                writer.WritePositionAt(scenePos);
                writer.WriteStringHAL(scene.Name);

                writer.WritePositionAt(scenePos + 0x4);
                writer.Write(scene.Dependencies.Count);
                for (int d = 0; d < scene.Dependencies.Count; d++)
                {
                    int idx = Scenes.FindIndex(x => x.Name == scene.Dependencies[d]);
                    if (idx < 0)
                        throw new KeyNotFoundException($"Scene {scene.Name} has dependency on scene {scene.Dependencies[d]}, but it does not exist!");
                    writer.Write(idx);
                }

                writer.WritePositionAt(scenePos + 0x8);
                writer.Write(scene.Assets.Count);
                for (int a = 0; a < scene.Assets.Count; a++)
                    writer.Write(stringList.IndexOf(scene.Assets[a]));
            }

            long stringSection = writer.BaseStream.Position;
            writer.WritePositionAt(headerStart + 0x10);
            writer.Write(stringList.Count);
            if (Version > 2)
            {
                for (int i = 0; i < stringList.Count; i++)
                {
                    writer.Write(0);
                    writer.Write(FNV1a.Calculate(Encoding.UTF8.GetBytes(stringList[i])));
                    strings.Add(writer.BaseStream.Position, stringList[i]);
                    writer.Write(-1);
                }
            }
            else
            {
                for (int i = 0; i < stringList.Count; i++)
                {
                    strings.Add(writer.BaseStream.Position, stringList[i]);
                    writer.Write(-1);
                }
            }

            strings.WriteAll(writer);

            XData.WriteFilesize(writer);
            XData.WriteFooter(writer);
        }
    }
}
