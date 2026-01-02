using System.Collections.Generic;
using OpenTK.Mathematics;
using System.Linq;
using System;

namespace minecraft.worldgen
{
    public class World
    {
        private Dictionary<Vector2i, Chunk> activeChunks = new();
        private Queue<Vector2i> chunkGenerationQueue = new();
        private Queue<Vector2i> meshRebuildQueue = new();

        private const int VIEW_RADIUS = 5;
        private const int MAX_CHUNKS_PER_FRAME = 1;
        private const int MAX_MESH_REBUILDS_PER_FRAME = 1;

        private Block blockTemplate;
        private WorldGenerator generator;

        public World(Block blockTemplate, int seed = 12345)
        {
            this.blockTemplate = blockTemplate;
            this.generator = new WorldGenerator(seed);
        }
        public WorldGenerator GetGenerator()
        {
            return generator;
        }

        // =============================
        // UPDATE
        // =============================
        public void Update(Vector3 cameraPosition)
        {
            Vector2i camChunk = new(
                (int)MathF.Floor(cameraPosition.X / Chunk.SIZE),
                (int)MathF.Floor(cameraPosition.Z / Chunk.SIZE)
            );

            HashSet<Vector2i> needed = new();

            // === REQUIRED CHUNKS ===
            for (int dx = -VIEW_RADIUS; dx <= VIEW_RADIUS; dx++)
                for (int dz = -VIEW_RADIUS; dz <= VIEW_RADIUS; dz++)
                {
                    Vector2i pos = new(camChunk.X + dx, camChunk.Y + dz);
                    needed.Add(pos);

                    if (!activeChunks.ContainsKey(pos) &&
                        !chunkGenerationQueue.Contains(pos))
                    {
                        chunkGenerationQueue.Enqueue(pos);
                    }
                }

            // === GENERATION ===
            int generated = 0;

            while (chunkGenerationQueue.Count > 0 && generated < MAX_CHUNKS_PER_FRAME)
            {
                Vector2i pos = chunkGenerationQueue.Dequeue();
                if (activeChunks.ContainsKey(pos)) continue;

                Chunk chunk = new Chunk();
                generator.GenerateChunkTerrain(chunk, pos);
                generator.ApplyPendingBlocksToChunk(chunk, pos);

                activeChunks.Add(pos, chunk);

                MarkChunkDirty(pos);
                MarkNeighborsDirty(pos);

                generated++;
            }

            // === MESH REBUILD (LIMITED) ===
            int rebuilt = 0;

            while (meshRebuildQueue.Count > 0 && rebuilt < MAX_MESH_REBUILDS_PER_FRAME)
            {
                Vector2i pos = meshRebuildQueue.Dequeue();

                if (!activeChunks.TryGetValue(pos, out Chunk chunk))
                    continue;

                if (!chunk.NeedsMeshRebuild)
                    continue;

                chunk.BuildMesh(this, blockTemplate, pos);
                rebuilt++;
            }

            // === CLEANUP ===
            var toRemove = activeChunks.Keys
                .Where(c => !needed.Contains(c))
                .ToList();

            foreach (var c in toRemove)
            {
                activeChunks[c].Mesh.Delete();
                generator.RemoveChunkCache(c);
                activeChunks.Remove(c);
            }
        }

        // =============================
        // DIRTY MANAGEMENT
        // =============================
        private void MarkChunkDirty(Vector2i pos)
        {
            if (!meshRebuildQueue.Contains(pos))
                meshRebuildQueue.Enqueue(pos);
        }

        private void MarkNeighborsDirty(Vector2i pos)
        {
            Vector2i[] neighbors =
            {
                pos + new Vector2i( 1, 0),
                pos + new Vector2i(-1, 0),
                pos + new Vector2i( 0, 1),
                pos + new Vector2i( 0,-1),
            };

            foreach (var n in neighbors)
                if (activeChunks.ContainsKey(n))
                    MarkChunkDirty(n);
        }

        // =============================
        // GLOBAL BLOCK ACCESS
        // =============================
        public BlockData GetBlockGlobal(int wx, int y, int wz)
        {
            Vector2i c = new(
                (int)MathF.Floor((float)wx / Chunk.SIZE),
                (int)MathF.Floor((float)wz / Chunk.SIZE)
            );

            if (!activeChunks.TryGetValue(c, out Chunk chunk))
                return new BlockData(BlockType.Air);

            int lx = wx - c.X * Chunk.SIZE;
            int lz = wz - c.Y * Chunk.SIZE;

            if (y < 0 || y >= Chunk.Height)
                return new BlockData(BlockType.Air);

            return chunk.GetBlock(lx, y, lz);
        }

        // =============================
        // RENDER ACCESS
        // =============================
        public IEnumerable<(Chunk chunk, Vector3 pos)> GetActiveChunks()
        {
            foreach (var kv in activeChunks)
                yield return (kv.Value, Vector3.Zero);
        }

        public bool HasChunkAt(int worldX, int worldZ)
        {
            Vector2i chunkPos = new(
                (int)MathF.Floor(worldX / (float)Chunk.SIZE),
                (int)MathF.Floor(worldZ / (float)Chunk.SIZE)
            );

            return activeChunks.ContainsKey(chunkPos);
        }

        public Chunk GetChunkAtWorldPos(Vector3 worldPos)
        {
            Vector2i chunkPos = new(
                (int)MathF.Floor(worldPos.X / Chunk.SIZE),
                (int)MathF.Floor(worldPos.Z / Chunk.SIZE)
            );

            activeChunks.TryGetValue(chunkPos, out Chunk chunk);
            return chunk;
        }


        public void SetBlock(Vector2i chunkCoord, Vector3 localPos, BlockType type)
        {
            if (!activeChunks.TryGetValue(chunkCoord, out Chunk chunk))
                return;

            int x = (int)localPos.X;
            int y = (int)localPos.Y;
            int z = (int)localPos.Z;

            if (!chunk.IsInside(x, y, z))
                return;

            chunk.SetBlock(x, y, z, type);

            // 🔥 marquer ce chunk et ses voisins dirty
            MarkChunkDirty(chunkCoord);
            MarkNeighborsDirty(chunkCoord);
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


    }
}
