using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace minecraft.worldgen
{
    public class ChunkMesh
    {
        public int Vao { get; private set; }
        public int Vbo { get; private set; }
        public int Ebo { get; private set; }
        public int IndexCount { get; private set; }

        public ChunkMesh()
        {
            Vao = GL.GenVertexArray();
            Vbo = GL.GenBuffer();
            Ebo = GL.GenBuffer();
        }

        public void Build(float[] vertices, uint[] indices)
        {
            IndexCount = indices.Length;

            GL.BindVertexArray(Vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, Vbo);
            GL.BufferData(
                BufferTarget.ArrayBuffer,
                vertices.Length * sizeof(float),
                vertices,
                BufferUsageHint.StaticDraw
            );

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, Ebo);
            GL.BufferData(
                BufferTarget.ElementArrayBuffer,
                indices.Length * sizeof(uint),
                indices,
                BufferUsageHint.StaticDraw
            );

            // ✅ FORMAT CORRECT :
            // Position (3) + UV (2) + Color (3) = 8 floats
            int stride = 8 * sizeof(float);

            // Position
            GL.VertexAttribPointer(
                0,
                3,
                VertexAttribPointerType.Float,
                false,
                stride,
                0
            );
            GL.EnableVertexAttribArray(0);

            // UV
            GL.VertexAttribPointer(
                1,
                2,
                VertexAttribPointerType.Float,
                false,
                stride,
                3 * sizeof(float)
            );
            GL.EnableVertexAttribArray(1);

            // ✅ Couleur du biome
            GL.VertexAttribPointer(
                2,
                3,
                VertexAttribPointerType.Float,
                false,
                stride,
                5 * sizeof(float)
            );
            GL.EnableVertexAttribArray(2);

            GL.BindVertexArray(0);
        }

        public void Draw()
        {
            GL.BindVertexArray(Vao);
            GL.DrawElements(
                PrimitiveType.Triangles,
                IndexCount,
                DrawElementsType.UnsignedInt,
                0
            );
            GL.BindVertexArray(0);
        }

        public void Delete()
        {
            GL.DeleteBuffer(Vbo);
            GL.DeleteBuffer(Ebo);
            GL.DeleteVertexArray(Vao);
        }
    }
}
