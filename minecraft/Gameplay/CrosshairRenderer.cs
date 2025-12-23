using System;
using System.IO;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using StbImageSharp;

namespace minecraft.Gameplay
{
    public class CrosshairRenderer
    {
        private int vao, vbo, ebo;
        private int shaderProgram;
        private int textureID;

        private readonly float[] vertices =
        {
            // positions (NDC)  // texcoords
            -0.02f,  0.02f,    0f, 1f, // top-left
             0.02f,  0.02f,    1f, 1f, // top-right
             0.02f, -0.02f,    1f, 0f, // bottom-right
            -0.02f, -0.02f,    0f, 0f  // bottom-left
        };

        private readonly uint[] indices = { 0, 1, 2, 2, 3, 0 };

        public void Init()
        {
            // Shader
            shaderProgram = GL.CreateProgram();
            int vert = CompileShader(ShaderType.VertexShader, File.ReadAllText("../../../Shaders/Default2D.vert"));
            int frag = CompileShader(ShaderType.FragmentShader, File.ReadAllText("../../../Shaders/Default2D.frag"));
            GL.AttachShader(shaderProgram, vert);
            GL.AttachShader(shaderProgram, frag);
            GL.LinkProgram(shaderProgram);
            GL.DeleteShader(vert);
            GL.DeleteShader(frag);

            // VBO / VAO / EBO
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

            // Texture PNG
            LoadTexture("../../../Texture/crosshair.png");

            // Active le blending
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }

        private void LoadTexture(string path)
        {
            using var stream = File.OpenRead(path);
            ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

            textureID = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, textureID);

            GL.TexImage2D(TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Rgba,
                image.Width,
                image.Height,
                0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                image.Data);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void Draw()
        {
            GL.UseProgram(shaderProgram);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureID);
            GL.Uniform1(GL.GetUniformLocation(shaderProgram, "ourTexture"), 0);

            GL.BindVertexArray(vao);
            GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);
        }

        private int CompileShader(ShaderType type, string source)
        {
            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                Console.WriteLine(GL.GetShaderInfoLog(shader));
            }
            return shader;
        }

        public void Delete()
        {
            GL.DeleteProgram(shaderProgram);
            GL.DeleteVertexArray(vao);
            GL.DeleteBuffer(vbo);
            GL.DeleteBuffer(ebo);
            GL.DeleteTexture(textureID);
        }
    }
}
