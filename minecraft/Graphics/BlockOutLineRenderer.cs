using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace minecraft.Gameplay
{
    public class BlockOutlineRenderer
    {
        private int vao, vbo;
        private int shader;

        // ✅ Cube CENTRÉ (comme Block)
        private readonly float[] cubeLines = new float[]
        {
            // Face avant
            -0.5f,-0.5f, 0.5f,  0.5f,-0.5f, 0.5f,
             0.5f,-0.5f, 0.5f,  0.5f, 0.5f, 0.5f,
             0.5f, 0.5f, 0.5f, -0.5f, 0.5f, 0.5f,
            -0.5f, 0.5f, 0.5f, -0.5f,-0.5f, 0.5f,

            // Face arrière
            -0.5f,-0.5f,-0.5f,  0.5f,-0.5f,-0.5f,
             0.5f,-0.5f,-0.5f,  0.5f, 0.5f,-0.5f,
             0.5f, 0.5f,-0.5f, -0.5f, 0.5f,-0.5f,
            -0.5f, 0.5f,-0.5f, -0.5f,-0.5f,-0.5f,

            // Arêtes verticales
            -0.5f,-0.5f,-0.5f, -0.5f,-0.5f, 0.5f,
             0.5f,-0.5f,-0.5f,  0.5f,-0.5f, 0.5f,
             0.5f, 0.5f,-0.5f,  0.5f, 0.5f, 0.5f,
            -0.5f, 0.5f,-0.5f, -0.5f, 0.5f, 0.5f
        };

        public void Init(int shaderProgram)
        {
            shader = shaderProgram;
            vao = GL.GenVertexArray();
            vbo = GL.GenBuffer();

            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer,
                cubeLines.Length * sizeof(float),
                cubeLines,
                BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.BindVertexArray(0);
        }

        public void Draw(Matrix4 view, Matrix4 projection, Vector3 blockCenter, float lineWidth, Vector3 color)
        {
            GL.UseProgram(shader);
            GL.Disable(EnableCap.DepthTest);
            GL.LineWidth(lineWidth);

            GL.Uniform3(GL.GetUniformLocation(shader, "uColor"), color);

            // ✅ CORRECTION : Juste la translation, pas de scale avec epsilon
            // Le cube outline fait déjà exactement 1x1x1 de -0.5 à +0.5
            Matrix4 model = Matrix4.CreateTranslation(blockCenter);

            GL.UniformMatrix4(GL.GetUniformLocation(shader, "model"), false, ref model);
            GL.UniformMatrix4(GL.GetUniformLocation(shader, "view"), false, ref view);
            GL.UniformMatrix4(GL.GetUniformLocation(shader, "projection"), false, ref projection);

            GL.BindVertexArray(vao);
            GL.DrawArrays(PrimitiveType.Lines, 0, 24);
            GL.BindVertexArray(0);

            GL.Enable(EnableCap.DepthTest);
        }

        public void Delete()
        {
            GL.DeleteVertexArray(vao);
            GL.DeleteBuffer(vbo);
        }
    }
}