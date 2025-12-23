using System;
using System.IO;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using minecraft.worldgen;
using minecraft.Gameplay;
using minecraft.Graphics;

namespace minecraft
{
    internal class Game : GameWindow
    {
        private World world;
        private Camera camera;
        private Block cubeBlock;
        private int shaderProgram;

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
            0,1,2,2,3,0,      // Front
            4,5,6,6,7,4,      // Back
            8,9,10,10,11,8,   // Left
            12,13,14,14,15,12,// Right
            16,17,18,18,19,16,// Top
            20,21,22,22,23,20 // Bottom
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
            camera = new Camera(new Vector3(0f, 5f, 10f));
            CursorState = CursorState.Grabbed;

            // Shader
            shaderProgram = GL.CreateProgram();
            int vertexShader = CompileShader(ShaderType.VertexShader, LoadShaderSource("Default3D.vert"));
            int fragmentShader = CompileShader(ShaderType.FragmentShader, LoadShaderSource("Default3D.frag"));
            GL.AttachShader(shaderProgram, vertexShader);
            GL.AttachShader(shaderProgram, fragmentShader);
            GL.LinkProgram(shaderProgram);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            // Texture atlas + BlockRegistry
            TextureAtlas atlas = new TextureAtlas("../../../Texture/atlas.png", 16);
            BlockRegistry.Init();

            cubeBlock = new Block(vertices, indices, atlas.TextureID);

            // World
            world = new World();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            camera.ProcessKeyboard(KeyboardState, (float)args.Time);
            camera.ProcessMouse(MousePosition);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.UseProgram(shaderProgram);

            Matrix4 view = camera.GetViewMatrix();
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(45f),
                Size.X / (float)Size.Y,
                0.1f,
                500f
            );

            GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "view"), false, ref view);
            GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "projection"), false, ref projection);

            for (int c = 0; c < world.Chunks.Count; c++)
            {
                Chunk chunk = world.Chunks[c];
                Vector3 chunkPos = world.ChunkPositions[c];

                chunk.BuildMesh(cubeBlock);

                GL.UseProgram(shaderProgram);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, cubeBlock.Texture);
                GL.Uniform1(GL.GetUniformLocation(shaderProgram, "ourTexture"), 0);

                Matrix4 model = Matrix4.CreateTranslation(chunkPos);
                GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "model"), false, ref model);

                chunk.Mesh.Draw();
            }


            SwapBuffers();
        }

        protected override void OnUnload()
        {
            cubeBlock.Delete();
            GL.DeleteProgram(shaderProgram);
            base.OnUnload();
        }

        private static int CompileShader(ShaderType type, string source)
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

        private static string LoadShaderSource(string path)
            => File.ReadAllText("../../../Shaders/" + path);
    }
}
