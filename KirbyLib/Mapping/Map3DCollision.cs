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
        public struct CollisionQuad
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

        public List<Vector3> VertexTable { get; set; } = new List<Vector3>();

        public List<CollisionQuad> CollisionQuads { get; set; } = new List<CollisionQuad>();

        public void ReadVertexTable(EndianBinaryReader reader, uint count)
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 vertex = new Vector3();
                vertex.X = reader.ReadSingle();
                vertex.Y = reader.ReadSingle();
                vertex.Z = reader.ReadSingle();
                VertexTable.Add(vertex);
            }
        }

        public void ReadCollisionQuads(EndianBinaryReader reader, uint count)
        {
            for (int i = 0; i < count; i++)
            {
                CollisionQuad quad = new CollisionQuad();
                quad.Kind = reader.ReadUInt32();
                quad.Vertex0 = reader.ReadUInt32();
                quad.Vertex1 = reader.ReadUInt32();
                quad.Vertex2 = reader.ReadUInt32();
                quad.Vertex3 = reader.ReadUInt32();
                CollisionQuads.Add(quad);
            }
        }

        public void WriteVertexTable(EndianBinaryWriter writer)
        {
            writer.WritePositionAt(0x1C);
            foreach (Vector3 vertex in VertexTable)
            {
                writer.Write(vertex.X);
                writer.Write(vertex.Y);
                writer.Write(vertex.Z);
            }
        }

        public void WriteCollisionQuads(EndianBinaryWriter writer)
        {
            writer.WritePositionAt(0x24);
            foreach (CollisionQuad quad in CollisionQuads)
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
