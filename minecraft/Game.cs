using System;
using System.IO;
using StbImageSharp;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using minecraft.worldgen;
using minecraft.Gameplay;

namespace minecraft
{
    internal class Game : GameWindow
    {
        private World world;
        private Camera camera;
        private Block cubeBlock;
        private int shaderProgram;
        private float rotation = 0f;

        private readonly float[] vertices = new float[]
        {
            // positions          // texCoords
            // Front
            -0.5f,-0.5f, 0.5f, 0f,0f,
             0.5f,-0.5f, 0.5f, 1f,0f,
             0.5f, 0.5f, 0.5f, 1f,1f,
            -0.5f, 0.5f, 0.5f, 0f,1f,
            // Back
            -0.5f,-0.5f,-0.5f, 0f,0f,
             0.5f,-0.5f,-0.5f, 1f,0f,
             0.5f, 0.5f,-0.5f, 1f,1f,
            -0.5f, 0.5f,-0.5f, 0f,1f,
            // Left
            -0.5f,-0.5f,-0.5f,0f,0f,
            -0.5f,-0.5f, 0.5f,1f,0f,
            -0.5f, 0.5f, 0.5f,1f,1f,
            -0.5f, 0.5f,-0.5f,0f,1f,
            // Right
             0.5f,-0.5f,-0.5f,0f,0f,
             0.5f,-0.5f, 0.5f,1f,0f,
             0.5f, 0.5f, 0.5f,1f,1f,
             0.5f, 0.5f,-0.5f,0f,1f,
            // Top
            -0.5f,0.5f,0.5f,0f,0f,
             0.5f,0.5f,0.5f,1f,0f,
             0.5f,0.5f,-0.5f,1f,1f,
            -0.5f,0.5f,-0.5f,0f,1f,
            // Bottom
            -0.5f,-0.5f,0.5f,0f,0f,
             0.5f,-0.5f,0.5f,1f,0f,
             0.5f,-0.5f,-0.5f,1f,1f,
            -0.5f,-0.5f,-0.5f,0f,1f
        };

        private readonly uint[] indices = new uint[]
        {
            0,1,2,2,3,0,
            4,5,6,6,7,4,
            8,9,10,10,11,8,
            12,13,14,14,15,12,
            16,17,18,18,19,16,
            20,21,22,22,23,20
        };

        public Game(int width, int height)
            : base(GameWindowSettings.Default,
                  new NativeWindowSettings()
                  {
                      Size = new Vector2i(width, height),
                      Title = "Voxel World",
                      APIVersion = new Version(3, 3),
                      Profile = ContextProfile.Core
                  })
        { }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1f);
            GL.Enable(EnableCap.DepthTest);

            // Camera
            camera = new Camera(new Vector3(0f, 0f, 5f));
            CursorState = CursorState.Grabbed;

            // Shader
            shaderProgram = GL.CreateProgram();
            int vertexShader = CompileShader(OpenTK.Graphics.OpenGL4.ShaderType.VertexShader, LoadShaderSource("Default3D.vert"));
            int fragmentShader = CompileShader(OpenTK.Graphics.OpenGL4.ShaderType.FragmentShader, LoadShaderSource("Default3D.frag"));
            GL.AttachShader(shaderProgram, vertexShader);
            GL.AttachShader(shaderProgram, fragmentShader);
            GL.LinkProgram(shaderProgram);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            // Texture
            int textureID = GL.GenTexture();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureID);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            StbImage.stbi_set_flip_vertically_on_load(1);
            using var stream = File.OpenRead("../../../Texture/placeholder.png");
            var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            // Cube block
            cubeBlock = new Block(vertices, indices, textureID);

            // World
            world = new World();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            rotation += 50f * (float)args.Time;
            camera.ProcessKeyboard(KeyboardState, (float)args.Time);
            camera.ProcessMouse(MousePosition);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.UseProgram(shaderProgram);

            // Matrices projection & view
            Matrix4 view = camera.GetViewMatrix();
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f),
                Size.X / (float)Size.Y, 0.1f, 100f);

            GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "view"), false, ref view);
            GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "projection"), false, ref projection);

            // Render all chunks
            for (int c = 0; c < world.Chunks.Count; c++)
            {
                Chunk chunk = world.Chunks[c];
                Vector3 chunkPos = world.ChunkPositions[c];

                for (int x = 0; x < Chunk.Width; x++)
                    for (int y = 0; y < Chunk.Height; y++)
                        for (int z = 0; z < Chunk.Depth; z++)
                        {
                            BlockData b = chunk.GetBlock(x, y, z);
                            if (b.Type != 0)
                            {
                                Vector3 pos = new Vector3(x, y, z) + chunkPos;
                                cubeBlock.Draw(pos, shaderProgram);
                            }
                        }
            }

            SwapBuffers();
        }

        protected override void OnUnload()
        {
            cubeBlock.Delete();
            GL.DeleteProgram(shaderProgram);
            base.OnUnload();
        }

        private static int CompileShader(OpenTK.Graphics.OpenGL4.ShaderType type, string source)
        {
            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);

            GL.GetShader(shader, OpenTK.Graphics.OpenGL4.ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                Console.WriteLine($"ERROR: {type} compilation failed!");
                Console.WriteLine(GL.GetShaderInfoLog(shader));
            }
            return shader;
        }

        private static string LoadShaderSource(string path) => File.ReadAllText("../../../Shaders/" + path);
    }
}
