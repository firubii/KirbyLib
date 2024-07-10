using KirbyLib.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace KirbyLib
{
    /// <summary>
    /// A 3D collision file for Kirby Star Allies's world maps.
    /// </summary>
    public class Map3D
    {
        public class Mesh
        {
            public string Name;
            public List<Triangle> Triangles = new List<Triangle>();
        }

        public struct Triangle
        {
            public Vector3 Vertex1;
            public Vector3 Vertex2;
            public Vector3 Vertex3;
            public Vector3 Normal;
        }

        public const uint MAGIC_NUMBER = 0x18;

        public XData XData { get; private set; } = new XData()
        {
            Version = new byte[] { 4, 0 },
            Endianness = Endianness.Little
        };

        public List<Mesh> Meshes = new List<Mesh>();

        public int Unknown1;
        public int Unknown2;
        public int Unknown3;
        public int Unknown4;
        public int Unknown5;
        public int Unknown6;

        public void Read(EndianBinaryReader reader)
        {
            XData.Read(reader);

            uint magic = reader.ReadUInt32();
            if (magic != MAGIC_NUMBER)
                throw new InvalidDataException($"Expected magic {MAGIC_NUMBER}, got {magic}");

            uint meshSection = reader.ReadUInt32();
            uint generalSection = reader.ReadUInt32();

            reader.BaseStream.Position = meshSection;
            Meshes = new List<Mesh>();
            uint meshCount = reader.ReadUInt32();
            for (uint i = 0; i < meshCount; i++)
            {
                Mesh mesh = new Mesh();

                reader.BaseStream.Position = meshSection + 4 + (i * 8);
                uint triSection = reader.ReadUInt32();
                mesh.Name = reader.ReadStringOffset();

                reader.BaseStream.Position = triSection;
                mesh.Triangles = new List<Triangle>();
                uint triCount = reader.ReadUInt32();
                for (int t = 0; t < triCount; t++)
                {
                    Triangle tri = new Triangle();
                    tri.Vertex1 = reader.ReadVector3();
                    tri.Vertex2 = reader.ReadVector3();
                    tri.Vertex3 = reader.ReadVector3();
                    tri.Normal = reader.ReadVector3();

                    mesh.Triangles.Add(tri);
                }
            }

            reader.BaseStream.Position = generalSection;
            Unknown1 = reader.ReadInt32();
            Unknown2 = reader.ReadInt32();
            Unknown3 = reader.ReadInt32();
            Unknown4 = reader.ReadInt32();
            Unknown5 = reader.ReadInt32();
            Unknown6 = reader.ReadInt32();
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

            long meshSection = writer.BaseStream.Position;
            writer.WritePositionAt(headerStart + 0x4);
            writer.Write(Meshes.Count);
            for (int i = 0; i < Meshes.Count; i++)
            {
                writer.Write(-1);
                strings.Add(writer.BaseStream.Position, Meshes[i].Name);
                writer.Write(-1);
            }

            writer.WritePositionAt(headerStart + 0x8);
            writer.Write(Unknown1);
            writer.Write(Unknown2);
            writer.Write(Unknown3);
            writer.Write(Unknown4);
            writer.Write(Unknown5);
            writer.Write(Unknown6);

            for (int i = 0; i < Meshes.Count; i++)
            {
                var mesh = Meshes[i];

                writer.WritePositionAt(meshSection + 4 + (i * 8));
                writer.Write(mesh.Triangles.Count);
                for (int t = 0; t < mesh.Triangles.Count; t++)
                {
                    writer.Write(mesh.Triangles[i].Vertex1);
                    writer.Write(mesh.Triangles[i].Vertex2);
                    writer.Write(mesh.Triangles[i].Vertex3);
                    writer.Write(mesh.Triangles[i].Normal);
                }
            }

            writer.WritePositionAt(headerStart + 0xC);
            writer.Write(0);
            writer.Write(0);
            writer.Write(0);
            writer.Write(writer.BaseStream.Position + 0x8);
            writer.Write(0);
            writer.Write(0);

            writer.Write(Meshes.Sum(x => x.Triangles.Count));
            for (int i = 0; i < Meshes.Count; i++)
            {
                for (int t = 0; t < Meshes[i].Triangles.Count; t++)
                {
                    writer.Write(i);
                    writer.Write(t);
                }
            }

            strings.WriteAll(writer);

            XData.WriteFilesize(writer);
            XData.WriteFooter(writer);
        }
    }
}
