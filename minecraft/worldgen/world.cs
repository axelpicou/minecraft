using System.Collections.Generic;
using OpenTK.Mathematics;
using minecraft.Graphics;
using System.Linq;
using System;

namespace minecraft.worldgen
{
    public class World
    {
        private Dictionary<Vector2i, Chunk> activeChunks = new();
        private const int VIEW_RADIUS = 5;
        private Block blockTemplate;
        private WorldGenerator generator;
        // Génération progressive
        private Queue<Vector2i> chunkGenerationQueue = new();
        private const int MAX_CHUNKS_PER_FRAME = 1;


        public World(Block blockTemplate, int seed = 12345)
        {
            this.blockTemplate = blockTemplate;
            this.generator = new WorldGenerator(seed);
        }

        public WorldGenerator GetGenerator() => generator;

        public void Update(Vector3 cameraPosition)
        {
            Vector2i camChunk = new Vector2i(
                (int)MathF.Floor(cameraPosition.X / Chunk.SIZE),
                (int)MathF.Floor(cameraPosition.Z / Chunk.SIZE)
            );

            HashSet<Vector2i> neededChunks = new();
            List<Vector2i> newChunks = new(); // Garder trace des nouveaux chunks

            // ÉTAPE 1 : Générer le terrain et les arbres pour tous les chunks
            for (int dx = -VIEW_RADIUS; dx <= VIEW_RADIUS; dx++)
            {
                for (int dz = -VIEW_RADIUS; dz <= VIEW_RADIUS; dz++)
                {
                    Vector2i chunkCoord = new Vector2i(camChunk.X + dx, camChunk.Y + dz);
                    neededChunks.Add(chunkCoord);

                    if (!activeChunks.ContainsKey(chunkCoord))
                    {
                        if (!chunkGenerationQueue.Contains(chunkCoord))
                            chunkGenerationQueue.Enqueue(chunkCoord);
                    }

                }
            }

            // ÉTAPE 2 : Appliquer les PendingBlocks restants aux chunks déjà générés
            if (newChunks.Count > 0)
            {
                // Appliquer uniquement aux nouveaux chunks
                foreach (var chunkPos in newChunks)
                {
                    int applied = generator.ApplyPendingBlocksToChunk(activeChunks[chunkPos], chunkPos);
                    if (applied > 0)
                        activeChunks[chunkPos].BuildMesh(blockTemplate, chunkPos);
                }
            }

            // ÉTAPE 3 : Construire les mesh pour les nouveaux chunks
            foreach (var chunkCoord in newChunks)
            {
                activeChunks[chunkCoord].BuildMesh(blockTemplate, chunkCoord);
            }

            // Nettoyer les chunks trop éloignés
            var toRemove = activeChunks.Keys.Where(c => !neededChunks.Contains(c)).ToList();
            foreach (var c in toRemove)
            {
                activeChunks[c].Mesh.Delete();
                generator.RemoveChunkCache(c);
                activeChunks.Remove(c);
            }

            int generated = 0;

            while (chunkGenerationQueue.Count > 0 && generated < MAX_CHUNKS_PER_FRAME)
            {
                Vector2i pos = chunkGenerationQueue.Dequeue();

                if (activeChunks.ContainsKey(pos))
                    continue;

                Chunk chunk = new Chunk();

                // Génération terrain + arbres (data only)
                generator.GenerateChunkTerrain(chunk, pos);

                activeChunks.Add(pos, chunk);

                // Appliquer pending blocks ciblés
                generator.ApplyPendingBlocksToChunk(chunk, pos);

                // Build mesh (gros coût → limité à 1 chunk / frame)
                chunk.BuildMesh(blockTemplate, pos);

                generated++;
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

        public bool HasChunkAt(int worldX, int worldZ)
        {
            Vector2i chunkPos = new Vector2i(
                (int)MathF.Floor(worldX / (float)Chunk.SIZE),
                (int)MathF.Floor(worldZ / (float)Chunk.SIZE)
            );

            return activeChunks.ContainsKey(chunkPos);
        }

    }
}