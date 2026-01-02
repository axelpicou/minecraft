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

            // Position
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // UV
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            // Couleur (AO + biome)
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 5 * sizeof(float));
            GL.EnableVertexAttribArray(2);

            Vao.Unbind();
        }

        // ============================================================
        // 🌱 MÉTHODE EXISTANTE (COULEUR UNIFORME)
        // ============================================================
        public float[] GetFaceVerticesWithColor(
            BlockFace face,
            Vector3 pos,
            float uMin,
            float vMin,
            float uMax,
            float vMax,
            Vector3 color)
        {
            return GetFaceVerticesWithColor(
                face, pos, uMin, vMin, uMax, vMax,
                new[] { color, color, color, color }
            );
        }

        // ============================================================
        // 🔥 NOUVELLE MÉTHODE : AO PAR SOMMET (4 COULEURS)
        // ============================================================
        public float[] GetFaceVerticesWithColor(
            BlockFace face,
            Vector3 pos,
            float uMin,
            float vMin,
            float uMax,
            float vMax,
            Vector3[] colors)
        {
            float x0 = pos.X;
            float y0 = pos.Y;
            float z0 = pos.Z;
            float x1 = pos.X + 1f;
            float y1 = pos.Y + 1f;
            float z1 = pos.Z + 1f;

            Vector3 c0 = colors[0];
            Vector3 c1 = colors[1];
            Vector3 c2 = colors[2];
            Vector3 c3 = colors[3];

            switch (face)
            {
                case BlockFace.Front: // Z+
                    return new float[]
                    {
                        x0, y0, z1, uMin, vMin, c0.X, c0.Y, c0.Z,
                        x1, y0, z1, uMax, vMin, c1.X, c1.Y, c1.Z,
                        x1, y1, z1, uMax, vMax, c2.X, c2.Y, c2.Z,
                        x0, y1, z1, uMin, vMax, c3.X, c3.Y, c3.Z
                    };

                case BlockFace.Back: // Z-
                    return new float[]
                    {
                        x1, y0, z0, uMin, vMin, c0.X, c0.Y, c0.Z,
                        x0, y0, z0, uMax, vMin, c1.X, c1.Y, c1.Z,
                        x0, y1, z0, uMax, vMax, c2.X, c2.Y, c2.Z,
                        x1, y1, z0, uMin, vMax, c3.X, c3.Y, c3.Z
                    };

                case BlockFace.Left: // X-
                    return new float[]
                    {
                        x0, y0, z0, uMin, vMin, c0.X, c0.Y, c0.Z,
                        x0, y0, z1, uMax, vMin, c1.X, c1.Y, c1.Z,
                        x0, y1, z1, uMax, vMax, c2.X, c2.Y, c2.Z,
                        x0, y1, z0, uMin, vMax, c3.X, c3.Y, c3.Z
                    };

                case BlockFace.Right: // X+
                    return new float[]
                    {
                        x1, y0, z1, uMin, vMin, c0.X, c0.Y, c0.Z,
                        x1, y0, z0, uMax, vMin, c1.X, c1.Y, c1.Z,
                        x1, y1, z0, uMax, vMax, c2.X, c2.Y, c2.Z,
                        x1, y1, z1, uMin, vMax, c3.X, c3.Y, c3.Z
                    };

                case BlockFace.Top: // Y+
                    return new float[]
                    {
                        x0, y1, z1, uMin, vMin, c0.X, c0.Y, c0.Z,
                        x1, y1, z1, uMax, vMin, c1.X, c1.Y, c1.Z,
                        x1, y1, z0, uMax, vMax, c2.X, c2.Y, c2.Z,
                        x0, y1, z0, uMin, vMax, c3.X, c3.Y, c3.Z
                    };

                case BlockFace.Bottom: // Y-
                    return new float[]
                    {
                        x0, y0, z0, uMin, vMin, c0.X, c0.Y, c0.Z,
                        x1, y0, z0, uMax, vMin, c1.X, c1.Y, c1.Z,
                        x1, y0, z1, uMax, vMax, c2.X, c2.Y, c2.Z,
                        x0, y0, z1, uMin, vMax, c3.X, c3.Y, c3.Z
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
