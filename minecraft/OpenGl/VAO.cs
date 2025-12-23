using OpenTK.Graphics.OpenGL4;

namespace minecraft.OpenGl
{
    public class VAO
    {
        public int Handle { get; private set; }

        public VAO()
        {
            Handle = GL.GenVertexArray();
        }

        public void Bind()
        {
            GL.BindVertexArray(Handle);
        }

        public void Unbind()
        {
            GL.BindVertexArray(0);
        }

        public void Delete()
        {
            GL.DeleteVertexArray(Handle);
        }
    }
}
