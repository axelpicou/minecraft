using OpenTK.Graphics.OpenGL4;

namespace minecraft.Gameplay
{
    public class CrosshairRenderer
    {
        private int vao, vbo;

        public void Init()
        {
            float[] verts = new float[]
            {
                -0.02f,0f, 0.02f,0f,
                0f,-0.02f, 0f,0.02f
            };

            vao = GL.GenVertexArray();
            vbo = GL.GenBuffer();

            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * verts.Length, verts, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
            GL.BindVertexArray(0);
        }

        public void Draw()
        {
            GL.Disable(EnableCap.DepthTest);
            GL.BindVertexArray(vao);
            GL.DrawArrays(PrimitiveType.Lines, 0, 4);
            GL.BindVertexArray(0);
            GL.Enable(EnableCap.DepthTest);
        }
    }
}
