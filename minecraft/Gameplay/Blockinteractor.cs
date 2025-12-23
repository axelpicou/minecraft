using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using minecraft.worldgen;

namespace minecraft.Gameplay
{
    public class BlockInteractor
    {
        private World world;
        private Camera camera;
        private BlockSelector selector;

        public BlockInteractor(World world, Camera camera, BlockSelector selector)
        {
            this.world = world;
            this.camera = camera;
            this.selector = selector;
        }

        public void Update(KeyboardState keyboard, MouseState mouse)
        {
            if (!selector.HasHit) return;

            Vector3 targetBlock = selector.HitBlock;
            Vector3Int normal = selector.FaceNormal;

            Vector3 placePos = targetBlock + new Vector3(normal.X, normal.Y, normal.Z);

            Vector2i chunkCoord = new Vector2i(
                (int)MathF.Floor(placePos.X / Chunk.SIZE),
                (int)MathF.Floor(placePos.Z / Chunk.SIZE)
            );

            Vector3 localPos = new Vector3(
                placePos.X - chunkCoord.X * Chunk.SIZE,
                placePos.Y,
                placePos.Z - chunkCoord.Y * Chunk.SIZE
            );

            if (mouse.IsButtonDown(MouseButton.Left))
            {
                // Casser
                world.SetBlock(new Vector2i(
                    (int)MathF.Floor(targetBlock.X / Chunk.SIZE),
                    (int)MathF.Floor(targetBlock.Z / Chunk.SIZE)
                ), targetBlock, BlockType.Air);
            }

            if (mouse.IsButtonDown(MouseButton.Right))
            {
                // Placer
                world.SetBlock(chunkCoord, localPos, BlockType.Stone);
            }
        }
    }
}
