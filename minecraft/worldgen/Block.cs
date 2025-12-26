using minecraft.OpenGl;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace minecraft.worldgen
{
    public enum BlockFace
    {
        Front,
        Back,
        Left,
        Right,
        Top,
        Bottom
    }

    public class Block
    {
        public VAO Vao { get; private set; }
        public VBO Vbo { get; private set; }
        public EBO Ebo { get; private set; }

        public int Texture { get; private set; }
        public int VertexCount { get; private set; }

        private readonly float[] baseVertices;
        private readonly int atlasTiles;

        public Block(float[] vertices, uint[] indices, int texture, int atlasTiles = 16)
        {
            this.atlasTiles = atlasTiles;
            Texture = texture;
            VertexCount = indices.Length;

            baseVertices = vertices;

            Vao = new VAO();
            Vbo = new VBO(vertices);
            Ebo = new EBO(indices);

            Vao.Bind();
            Vbo.Bind();
            Ebo.Bind();

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            Vao.Unbind();
        }

        private void UpdateUVs(int textureIndex)
        {
            float tileSize = 1f / atlasTiles;

            int x = textureIndex % atlasTiles;
            int y = textureIndex / atlasTiles;

            float uMin = x * tileSize;
            float vMin = y * tileSize;

            float[] vertices = (float[])baseVertices.Clone();

            for (int i = 0; i < vertices.Length; i += 5)
            {
                float u = baseVertices[i + 3];
                float v = baseVertices[i + 4];

                vertices[i + 3] = uMin + u * tileSize;
                vertices[i + 4] = vMin + v * tileSize;
            }

            Vbo.Bind();
            GL.BufferSubData(BufferTarget.ArrayBuffer, 0, vertices.Length * sizeof(float), vertices);
        }

        public void DrawFace(Vector3 position, int shaderProgram, int textureIndex, BlockFace face)
        {
            GL.UseProgram(shaderProgram);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, Texture);
            GL.Uniform1(GL.GetUniformLocation(shaderProgram, "ourTexture"), 0);

            UpdateUVs(textureIndex);

            Matrix4 model = Matrix4.Identity;
            GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "model"), false, ref model);

            Vao.Bind();
            int offset = (int)face * 6;
            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, offset * sizeof(uint));
            Vao.Unbind();
        }

        // ✅ CORRECTION : Les blocs occupent maintenant l'espace de (X,Y,Z) à (X+1,Y+1,Z+1)
        public float[] GetFaceVertices(BlockFace face, Vector3 pos, float uMin, float vMin, float uMax, float vMax)
        {
            // pos représente le coin inférieur gauche arrière du bloc
            float x0 = pos.X;
            float y0 = pos.Y;
            float z0 = pos.Z;
            float x1 = pos.X + 1f;
            float y1 = pos.Y + 1f;
            float z1 = pos.Z + 1f;

            switch (face)
            {
                case BlockFace.Front: // Z+
                    return new float[]
                    {
                        x0, y0, z1, uMin, vMin,
                        x1, y0, z1, uMax, vMin,
                        x1, y1, z1, uMax, vMax,
                        x0, y1, z1, uMin, vMax
                    };
                case BlockFace.Back: // Z-
                    return new float[]
                    {
                        x1, y0, z0, uMin, vMin,
                        x0, y0, z0, uMax, vMin,
                        x0, y1, z0, uMax, vMax,
                        x1, y1, z0, uMin, vMax
                    };
                case BlockFace.Left: // X-
                    return new float[]
                    {
                        x0, y0, z0, uMin, vMin,
                        x0, y0, z1, uMax, vMin,
                        x0, y1, z1, uMax, vMax,
                        x0, y1, z0, uMin, vMax
                    };
                case BlockFace.Right: // X+
                    return new float[]
                    {
                        x1, y0, z1, uMin, vMin,
                        x1, y0, z0, uMax, vMin,
                        x1, y1, z0, uMax, vMax,
                        x1, y1, z1, uMin, vMax
                    };
                case BlockFace.Top: // Y+
                    return new float[]
                    {
                        x0, y1, z1, uMin, vMin,
                        x1, y1, z1, uMax, vMin,
                        x1, y1, z0, uMax, vMax,
                        x0, y1, z0, uMin, vMax
                    };
                case BlockFace.Bottom: // Y-
                    return new float[]
                    {
                        x0, y0, z0, uMin, vMin,
                        x1, y0, z0, uMax, vMin,
                        x1, y0, z1, uMax, vMax,
                        x0, y0, z1, uMin, vMax
                    };
                default:
                    return new float[0];
            }
        }

        public void Delete()
        {
            Vao.Delete();
            Vbo.Delete();
            Ebo.Delete();
        }
    }
}