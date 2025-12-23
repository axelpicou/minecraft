using OpenTK.Graphics.OpenGL4;

namespace minecraft.OpenGl
{
    public class EBO
    {
        public int Handle { get; private set; }

        public EBO(uint[] data)
        {
            Handle = GL.GenBuffer();
            Bind();
            GL.BufferData(BufferTarget.ElementArrayBuffer, data.Length * sizeof(uint), data, BufferUsageHint.StaticDraw);
            Unbind();
        }

        public void Bind()
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, Handle);
        }

        public void Unbind()
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        public void Delete()
        {
            GL.DeleteBuffer(Handle);
        }
    }
}
