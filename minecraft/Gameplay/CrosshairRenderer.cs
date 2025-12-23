using OpenTK.Graphics.OpenGL4;
using StbImageSharp;
using System.IO;

namespace minecraft.Gameplay
{
    public class CrosshairRenderer
    {
        private int vao, vbo, ebo;
        private int shader;
        private int texture;

        private readonly float[] vertices =
        {
            // pos      // uv
            -0.02f,  0.02f,  0f, 1f,
             0.02f,  0.02f,  1f, 1f,
             0.02f, -0.02f,  1f, 0f,
            -0.02f, -0.02f,  0f, 0f
        };

        private readonly uint[] indices = { 0, 1, 2, 2, 3, 0 };

        public void Init()
        {
            // Shader
            shader = GL.CreateProgram();
            int vs = Compile(ShaderType.VertexShader, File.ReadAllText("../../../Shaders/Default2D.vert"));
            int fs = Compile(ShaderType.FragmentShader, File.ReadAllText("../../../Shaders/Default2D.frag"));
            GL.AttachShader(shader, vs);
            GL.AttachShader(shader, fs);
            GL.LinkProgram(shader);
            GL.DeleteShader(vs);
            GL.DeleteShader(fs);

            // VAO HUD
            vao = GL.GenVertexArray();
            vbo = GL.GenBuffer();
            ebo = GL.GenBuffer();

            GL.BindVertexArray(vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            GL.BindVertexArray(0);

            LoadTexture("../../../Texture/crosshair.png");
        }

        public void Draw()
        {
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.UseProgram(shader);
            GL.BindVertexArray(vao);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.Uniform1(GL.GetUniformLocation(shader, "ourTexture"), 0);

            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);

            GL.BindVertexArray(0);
            GL.Enable(EnableCap.DepthTest);
        }

        private void LoadTexture(string path)
        {
            using var s = File.OpenRead(path);
            var img = ImageResult.FromStream(s, ColorComponents.RedGreenBlueAlpha);

            texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                img.Width, img.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, img.Data);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        }

        private int Compile(ShaderType t, string src)
        {
            int s = GL.CreateShader(t);
            GL.ShaderSource(s, src);
            GL.CompileShader(s);
            return s;
        }
    }
}
