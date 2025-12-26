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

            // ✅ Layout: Position(3) + TexCoord(2) + Color(3) = 8 floats par vertex
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            // ✅ Attribut de couleur
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 5 * sizeof(float));
            GL.EnableVertexAttribArray(2);

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

            for (int i = 0; i < vertices.Length; i += 8)
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

        // ✅ Méthode avec couleur de biome
        public float[] GetFaceVerticesWithColor(BlockFace face, Vector3 pos, float uMin, float vMin, float uMax, float vMax, Vector3 color)
        {
            float x0 = pos.X;
            float y0 = pos.Y;
            float z0 = pos.Z;
            float x1 = pos.X + 1f;
            float y1 = pos.Y + 1f;
            float z1 = pos.Z + 1f;

            float r = color.X;
            float g = color.Y;
            float b = color.Z;

            switch (face)
            {
                case BlockFace.Front: // Z+
                    return new float[]
                    {
                        x0, y0, z1, uMin, vMin, r, g, b,
                        x1, y0, z1, uMax, vMin, r, g, b,
                        x1, y1, z1, uMax, vMax, r, g, b,
                        x0, y1, z1, uMin, vMax, r, g, b
                    };
                case BlockFace.Back: // Z-
                    return new float[]
                    {
                        x1, y0, z0, uMin, vMin, r, g, b,
                        x0, y0, z0, uMax, vMin, r, g, b,
                        x0, y1, z0, uMax, vMax, r, g, b,
                        x1, y1, z0, uMin, vMax, r, g, b
                    };
                case BlockFace.Left: // X-
                    return new float[]
                    {
                        x0, y0, z0, uMin, vMin, r, g, b,
                        x0, y0, z1, uMax, vMin, r, g, b,
                        x0, y1, z1, uMax, vMax, r, g, b,
                        x0, y1, z0, uMin, vMax, r, g, b
                    };
                case BlockFace.Right: // X+
                    return new float[]
                    {
                        x1, y0, z1, uMin, vMin, r, g, b,
                        x1, y0, z0, uMax, vMin, r, g, b,
                        x1, y1, z0, uMax, vMax, r, g, b,
                        x1, y1, z1, uMin, vMax, r, g, b
                    };
                case BlockFace.Top: // Y+
                    return new float[]
                    {
                        x0, y1, z1, uMin, vMin, r, g, b,
                        x1, y1, z1, uMax, vMin, r, g, b,
                        x1, y1, z0, uMax, vMax, r, g, b,
                        x0, y1, z0, uMin, vMax, r, g, b
                    };
                case BlockFace.Bottom: // Y-
                    return new float[]
                    {
                        x0, y0, z0, uMin, vMin, r, g, b,
                        x1, y0, z0, uMax, vMin, r, g, b,
                        x1, y0, z1, uMax, vMax, r, g, b,
                        x0, y0, z1, uMin, vMax, r, g, b
                    };
                default:
                    return new float[0];
            }
        }

        // Ancienne méthode sans couleur (pour compatibilité)
        public float[] GetFaceVertices(BlockFace face, Vector3 pos, float uMin, float vMin, float uMax, float vMax)
        {
            return GetFaceVerticesWithColor(face, pos, uMin, vMin, uMax, vMax, Vector3.One);
        }

        public void Delete()
        {
            Vao.Delete();
            Vbo.Delete();
            Ebo.Delete();
        }
    }
}