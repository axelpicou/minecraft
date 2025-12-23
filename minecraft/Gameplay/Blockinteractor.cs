using OpenTK.Mathematics;
using minecraft.worldgen;

namespace minecraft.Gameplay
{
    public class BlockInteractor
    {
        private World world;
        private Camera camera;
        private const float RayStep = 0.1f;

        public BlockInteractor(World world, Camera camera)
        {
            this.world = world;
            this.camera = camera;
        }

        public void Update(bool leftClick, bool rightClick)
        {
            if (!leftClick && !rightClick) return;

            var result = Raycast(5f); // distance max 5 unités
            if (result.hit && leftClick)
            {
                world.SetBlock(result.chunkCoord, result.localPos, BlockType.Air);
            }
            else if (result.hit && rightClick)
            {
                // place le bloc juste devant la face touchée
                Vector3 placePos = result.worldPos + result.normal;
                Vector2i chunkCoord = new Vector2i(
                    (int)MathF.Floor(placePos.X / Chunk.SIZE),
                    (int)MathF.Floor(placePos.Z / Chunk.SIZE)
                );
                Vector3 localPos = new Vector3(
                    placePos.X - chunkCoord.X * Chunk.SIZE,
                    placePos.Y,
                    placePos.Z - chunkCoord.Y * Chunk.SIZE
                );
                world.SetBlock(chunkCoord, localPos, BlockType.Stone);
            }
        }

        private (bool hit, Vector2i chunkCoord, Vector3 localPos, Vector3 worldPos, Vector3 normal) Raycast(float maxDistance)
        {
            Vector3 origin = camera.Position;
            Vector3 dir = camera.Front.Normalized();

            float step = RayStep;
            Vector3 lastPos = origin;

            for (float t = 0; t < maxDistance; t += step)
            {
                Vector3 pos = origin + dir * t;

                Chunk chunk = world.GetChunkAtWorldPos(pos);
                if (chunk == null) continue;

                int localX = (int)MathF.Floor(pos.X - (int)MathF.Floor(pos.X / Chunk.SIZE) * Chunk.SIZE);
                int localY = (int)MathF.Floor(pos.Y);
                int localZ = (int)MathF.Floor(pos.Z - (int)MathF.Floor(pos.Z / Chunk.SIZE) * Chunk.SIZE);

                if (!chunk.IsInside(localX, localY, localZ)) continue;

                var block = chunk.GetBlock(localX, localY, localZ);
                if (block.Type != BlockType.Air)
                {
                    // normal approximative pour placer le bloc
                    Vector3 normal = (pos - lastPos).Normalized();
                    Vector2i chunkCoord = new Vector2i(
                        (int)MathF.Floor(pos.X / Chunk.SIZE),
                        (int)MathF.Floor(pos.Z / Chunk.SIZE)
                    );
                    Vector3 localPos = new Vector3(localX, localY, localZ);

                    return (true, chunkCoord, localPos, pos, normal);
                }

                lastPos = pos;
            }

            return (false, new Vector2i(), new Vector3(), Vector3.Zero, Vector3.Zero);
        }
    }
}
