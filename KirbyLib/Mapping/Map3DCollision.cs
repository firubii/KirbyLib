using KirbyLib.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace KirbyLib.Mapping
{
    /// <summary>
    /// A collision model for Kirby 3D Rumble and Kirby: Blowout Blast.
    /// </summary>
    public class Map3DCollision
    {
        /// <summary>
        /// A solid quad collision. Each vertex is an index in the vertex table.
        /// </summary>
        public struct Quad
        {
            /// <summary>
            /// The type of collision.
            /// </summary>
            public uint Kind;
            public uint Vertex0;
            public uint Vertex1;
            public uint Vertex2;
            public uint Vertex3;
        }

        public List<Vector3> Vertices = new List<Vector3>();
        public List<Quad> Quads = new List<Quad>();

        public void ReadVertexTable(EndianBinaryReader reader, uint count)
        {
            Vertices = new List<Vector3>();
            for (int i = 0; i < count; i++)
                Vertices.Add(reader.ReadVector3());
        }

        public void ReadCollisionQuads(EndianBinaryReader reader, uint count)
        {
            Quads = new List<Quad>();
            for (int i = 0; i < count; i++)
            {
                Quad quad = new Quad();
                quad.Kind = reader.ReadUInt32();
                quad.Vertex0 = reader.ReadUInt32();
                quad.Vertex1 = reader.ReadUInt32();
                quad.Vertex2 = reader.ReadUInt32();
                quad.Vertex3 = reader.ReadUInt32();
                Quads.Add(quad);
            }
        }

        public void WriteVertexTable(EndianBinaryWriter writer)
        {
            foreach (Vector3 vertex in Vertices)
                writer.Write(vertex);
        }

        public void WriteCollisionQuads(EndianBinaryWriter writer)
        {
            foreach (Quad quad in Quads)
            {
                writer.Write(quad.Kind);
                writer.Write(quad.Vertex0);
                writer.Write(quad.Vertex1);
                writer.Write(quad.Vertex2);
                writer.Write(quad.Vertex3);
            }
        }
    }
}
