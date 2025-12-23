using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using minecraft.worldgen;

namespace minecraft.Gameplay
{
    public class PlayerController
    {
        public Camera Camera { get; private set; }
        private float speed = 5f;

        private World world;

        public PlayerController(Vector3 startPos, World world)
        {
            Camera = new Camera(startPos);
            this.world = world;
        }

        public void Update(KeyboardState keyboard, Vector2 mouseDelta, float deltaTime)
        {
            Camera.ProcessKeyboard(keyboard, deltaTime);
            Camera.ProcessMouse(mouseDelta);

            // Ajuste la caméra au dessus du terrain
            Vector3 camPos = Camera.Position;
            int chunkX = (int)MathF.Floor(camPos.X / Chunk.SIZE);
            int chunkZ = (int)MathF.Floor(camPos.Z / Chunk.SIZE);

            // hauteur du terrain
            int terrainHeight = world.GetHeightAt(camPos.X, camPos.Z);
            if (camPos.Y < terrainHeight + 1)
                Camera.SetPosition(new Vector3(camPos.X, terrainHeight + 1, camPos.Z));
        }
    }
}
