using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace minecraft.Gameplay
{
    public class BlockOutlineRenderer
    {
        private int vao, vbo;
        private int shader;

        private readonly float[] cubeLines = new float[]
        {
            // 12 edges du cube, chaque ligne = 2 points
            0,0,0, 1,0,0,
            1,0,0, 1,1,0,
            1,1,0, 0,1,0,
            0,1,0, 0,0,0,

            0,0,1, 1,0,1,
            1,0,1, 1,1,1,
            1,1,1, 0,1,1,
            0,1,1, 0,0,1,

            0,0,0, 0,0,1,
            1,0,0, 1,0,1,
            1,1,0, 1,1,1,
            0,1,0, 0,1,1
        };

        public void Init(int shaderProgram)
        {
            shader = shaderProgram;

            vao = GL.GenVertexArray();
            vbo = GL.GenBuffer();

            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, cubeLines.Length * sizeof(float), cubeLines, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.BindVertexArray(0);
        }

        public void Draw(Matrix4 view, Matrix4 projection, Vector3 position, float lineWidth = 2f, Vector3 color = default)
        {
            GL.UseProgram(shader);

            GL.LineWidth(lineWidth);
            int colorLoc = GL.GetUniformLocation(shader, "uColor");
            GL.Uniform3(colorLoc, color);

            int modelLoc = GL.GetUniformLocation(shader, "model");
            Matrix4 model = Matrix4.CreateTranslation(position);
            GL.UniformMatrix4(modelLoc, false, ref model);

            int viewLoc = GL.GetUniformLocation(shader, "view");
            GL.UniformMatrix4(viewLoc, false, ref view);

            int projLoc = GL.GetUniformLocation(shader, "projection");
            GL.UniformMatrix4(projLoc, false, ref projection);

            GL.BindVertexArray(vao);
            GL.DrawArrays(PrimitiveType.Lines, 0, 24); // 12 edges * 2 points
            GL.BindVertexArray(0);
        }

        public void Delete()
        {
            GL.DeleteVertexArray(vao);
            GL.DeleteBuffer(vbo);
        }
    }
}
