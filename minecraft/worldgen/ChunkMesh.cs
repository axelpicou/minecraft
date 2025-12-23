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
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, Ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            // Position
            GL.VertexAttribPointer(0, 3, OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            // UV
            GL.VertexAttribPointer(1, 2, OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            GL.BindVertexArray(0);
        }

        public void Draw()
        {
            GL.BindVertexArray(Vao);
            GL.DrawElements(PrimitiveType.Triangles, IndexCount, OpenTK.Graphics.OpenGL4.DrawElementsType.UnsignedInt, 0);
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
