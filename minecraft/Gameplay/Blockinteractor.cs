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
            if (!selector.HasHit)
                return;

            Vector3Int hitVoxel = selector.HitVoxel;
            Vector3Int normal = selector.FaceNormal;

            // ----------------------
            // CASSER LE BLOC
            // ----------------------
            if (mouse.IsButtonPressed(MouseButton.Left))
            {
                Vector2i chunkCoord = new Vector2i(
                    WorldToChunk(hitVoxel.X),
                    WorldToChunk(hitVoxel.Z)
                );

                Vector3 localPos = new Vector3(
                    MathMod(hitVoxel.X, Chunk.SIZE),
                    hitVoxel.Y,
                    MathMod(hitVoxel.Z, Chunk.SIZE)
                );

                world.SetBlock(chunkCoord, localPos, BlockType.Air);
            }

            // ----------------------
            // POSER SUR LA FACE VISÉE
            // ----------------------
            if (mouse.IsButtonPressed(MouseButton.Right))
            {
                Vector3Int placeVoxel = hitVoxel + normal;

                Vector2i chunkCoord = new Vector2i(
                    WorldToChunk(placeVoxel.X),
                    WorldToChunk(placeVoxel.Z)
                );

                Vector3 localPos = new Vector3(
                    MathMod(placeVoxel.X, Chunk.SIZE),
                    placeVoxel.Y,
                    MathMod(placeVoxel.Z, Chunk.SIZE)
                );

                world.SetBlock(chunkCoord, localPos, BlockType.Stone);
            }
        }

        private int WorldToChunk(int world)
        {
            return (int)MathF.Floor((float)world / Chunk.SIZE);
        }

        private int MathMod(int a, int b)
        {
            int r = a % b;
            return r < 0 ? r + b : r;
        }
    }
}
