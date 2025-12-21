using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace minecraft.worldgen
{
    public class Block
    {
        public VAO Vao { get; private set; }
        public VBO Vbo { get; private set; }
        public EBO Ebo { get; private set; }
        public int Texture { get; private set; }
        public int VertexCount { get; private set; }

        public Block(float[] vertices, uint[] indices, int texture)
        {
            Texture = texture;
            VertexCount = indices.Length;

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

        public void Draw(Vector3 position, int shaderProgram)
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, Texture);
            GL.Uniform1(GL.GetUniformLocation(shaderProgram, "ourTexture"), 0);

            Matrix4 model = Matrix4.CreateTranslation(position);
            GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "model"), false, ref model);

            Vao.Bind();
            GL.DrawElements(PrimitiveType.Triangles, VertexCount, DrawElementsType.UnsignedInt, 0);
            Vao.Unbind();
        }

        public void Delete()
        {
            Vao.Delete();
            Vbo.Delete();
            Ebo.Delete();
        }
    }
}
