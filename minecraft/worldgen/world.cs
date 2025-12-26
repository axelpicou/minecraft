using System.Collections.Generic;
using OpenTK.Mathematics;
using minecraft.Graphics;
using System.Linq;

namespace minecraft.worldgen
{
    public class World
    {
        private Dictionary<Vector2i, Chunk> activeChunks = new();
        private const int VIEW_RADIUS = 5;
        private Block blockTemplate;
        private WorldGenerator generator;

        public World(Block blockTemplate, int seed = 12345)
        {
            this.blockTemplate = blockTemplate;
            this.generator = new WorldGenerator(seed);
        }

        // Permet de modifier les paramètres de génération
        public WorldGenerator GetGenerator() => generator;

        public void Update(Vector3 cameraPosition)
        {
            Vector2i camChunk = new Vector2i(
                (int)MathF.Floor(cameraPosition.X / Chunk.SIZE),
                (int)MathF.Floor(cameraPosition.Z / Chunk.SIZE)
            );

            HashSet<Vector2i> neededChunks = new();

            for (int dx = -VIEW_RADIUS; dx <= VIEW_RADIUS; dx++)
            {
                for (int dz = -VIEW_RADIUS; dz <= VIEW_RADIUS; dz++)
                {
                    Vector2i chunkCoord = new Vector2i(camChunk.X + dx, camChunk.Y + dz);
                    neededChunks.Add(chunkCoord);

                    if (!activeChunks.ContainsKey(chunkCoord))
                    {
                        Chunk chunk = new Chunk();

                        // ✅ Utiliser le nouveau WorldGenerator
                        generator.GenerateChunkTerrain(chunk, chunkCoord);

                        chunk.BuildMesh(blockTemplate, chunkCoord);
                        activeChunks.Add(chunkCoord, chunk);
                    }
                }
            }

            var toRemove = activeChunks.Keys.Where(c => !neededChunks.Contains(c)).ToList();
            foreach (var c in toRemove)
            {
                activeChunks[c].Mesh.Delete();
                activeChunks.Remove(c);
            }
        }

        public IEnumerable<(Chunk chunk, Vector3 position)> GetActiveChunks()
        {
            foreach (var kvp in activeChunks)
            {
                Chunk chunk = kvp.Value;
                Vector3 pos = Vector3.Zero;
                yield return (chunk, pos);
            }
        }

        public int GetHeightAt(float worldX, float worldZ)
        {
            Vector2i chunkCoord = new Vector2i(
                (int)MathF.Floor(worldX / Chunk.SIZE),
                (int)MathF.Floor(worldZ / Chunk.SIZE)
            );

            if (!activeChunks.TryGetValue(chunkCoord, out Chunk chunk))
                return 0;

            int localX = (int)(worldX - chunkCoord.X * Chunk.SIZE);
            int localZ = (int)(worldZ - chunkCoord.Y * Chunk.SIZE);

            for (int y = Chunk.Height - 1; y >= 0; y--)
            {
                if (chunk.GetBlock(localX, y, localZ).Type != BlockType.Air)
                    return y + 1;
            }

            return 0;
        }

        public Chunk GetChunkAtWorldPos(Vector3 worldPos)
        {
            Vector2i chunkCoord = new Vector2i(
                (int)MathF.Floor(worldPos.X / Chunk.SIZE),
                (int)MathF.Floor(worldPos.Z / Chunk.SIZE)
            );

            if (activeChunks.TryGetValue(chunkCoord, out Chunk chunk))
                return chunk;
            return null;
        }

        public void SetBlock(Vector2i chunkCoord, Vector3 localPos, BlockType type)
        {
            if (!activeChunks.TryGetValue(chunkCoord, out Chunk chunk))
                return;

            int x = (int)localPos.X;
            int y = (int)localPos.Y;
            int z = (int)localPos.Z;

            if (x < 0 || x >= Chunk.SIZE ||
                y < 0 || y >= Chunk.Height ||
                z < 0 || z >= Chunk.SIZE)
                return;

            chunk.SetBlock(x, y, z, type);
            chunk.BuildMesh(blockTemplate, chunkCoord);
        }
    }
}