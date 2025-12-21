using OpenTK.Graphics.OpenGL4;

namespace minecraft.OpenGl
{
    public class VBO
    {
        public int Handle { get; private set; }

        public VBO(float[] data)
        {
            Handle = GL.GenBuffer();
            Bind();
            GL.BufferData(BufferTarget.ArrayBuffer, data.Length * sizeof(float), data, BufferUsageHint.StaticDraw);
            Unbind();
        }

        public void Bind()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, Handle);
        }

        public void Unbind()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public void Delete()
        {
            GL.DeleteBuffer(Handle);
        }
    }
}
