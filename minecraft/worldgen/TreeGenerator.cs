using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace minecraft.worldgen
{
    public static class TreeGenerator
    {
        public static void GenerateTreeWithPending(
            Chunk chunk,
            Vector2i chunkPos,
            Vector3i basePos,
            BiomeData biome,
            int seed,
            Dictionary<Vector2i, List<PendingBlock>> globalPendingBlocks)
        {
            int hash = basePos.X * 734287 ^ basePos.Z * 912931 ^ seed;
            Random rng = new Random(hash);

            switch (biome.TreeType)
            {
                case TreeType.Oak:
                    GenerateOak(chunk, chunkPos, basePos, biome, rng, globalPendingBlocks);
                    break;

                case TreeType.Pine:
                    GeneratePine(chunk, chunkPos, basePos, biome, rng, globalPendingBlocks);
                    break;

                case TreeType.Birch:
                    GenerateBirch(chunk, chunkPos, basePos, biome, rng, globalPendingBlocks);
                    break;
            }
        }

        // =============================
        // TREE TYPES
        // =============================

        private static void GenerateOak(
            Chunk chunk, Vector2i chunkPos, Vector3i pos, BiomeData biome, Random rng,
            Dictionary<Vector2i, List<PendingBlock>> globalPending)
        {
            bool big = rng.NextDouble() < 0.35;

            if (big)
            {
                int height = rng.Next(7, 10);

                // Tronc 2x2
                for (int dx = 0; dx < 2; dx++)
                    for (int dz = 0; dz < 2; dz++)
                        for (int y = 0; y < height; y++)
                            PlaceBlock(chunk, chunkPos,
                                pos.X + dx,
                                pos.Y + y,
                                pos.Z + dz,
                                BlockType.Log,
                                Vector3.One,
                                globalPending);

                GenerateLeaves(chunk, chunkPos,
                    pos.X + 1,
                    pos.Y + height,
                    pos.Z + 1,
                    3,
                    biome.LeavesColor,
                    globalPending);
            }
            else
            {
                int height = rng.Next(4, 6);

                for (int y = 0; y < height; y++)
                    PlaceBlock(chunk, chunkPos, pos.X, pos.Y + y, pos.Z, BlockType.Log, Vector3.One, globalPending);

                GenerateLeaves(chunk, chunkPos,
                    pos.X,
                    pos.Y + height,
                    pos.Z,
                    2,
                    biome.LeavesColor,
                    globalPending);
            }
        }

        private static void GeneratePine(
            Chunk chunk, Vector2i chunkPos, Vector3i pos, BiomeData biome, Random rng,
            Dictionary<Vector2i, List<PendingBlock>> globalPending)
        {
            int height = rng.Next(8, 12);

            for (int y = 0; y < height; y++)
            {
                PlaceBlock(chunk, chunkPos, pos.X, pos.Y + y, pos.Z, BlockType.Log, Vector3.One, globalPending);

                int r = Math.Max(0, (height - y) / 3);
                if (r > 0)
                    GenerateLeaves(chunk, chunkPos, pos.X, pos.Y + y, pos.Z, r, biome.LeavesColor, globalPending);
            }
        }

        private static void GenerateBirch(
            Chunk chunk, Vector2i chunkPos, Vector3i pos, BiomeData biome, Random rng,
            Dictionary<Vector2i, List<PendingBlock>> globalPending)
        {
            int height = rng.Next(5, 7);

            for (int y = 0; y < height; y++)
                PlaceBlock(chunk, chunkPos, pos.X, pos.Y + y, pos.Z, BlockType.Log, Vector3.One, globalPending);

            GenerateLeaves(chunk, chunkPos,
                pos.X,
                pos.Y + height,
                pos.Z,
                2,
                biome.LeavesColor,
                globalPending);
        }

        // =============================
        // LEAVES
        // =============================

        private static void GenerateLeaves(
            Chunk chunk, Vector2i chunkPos, int cx, int cy, int cz, int radius, Vector3 color,
            Dictionary<Vector2i, List<PendingBlock>> globalPending)
        {
            for (int dx = -radius; dx <= radius; dx++)
                for (int dy = -radius; dy <= radius; dy++)
                    for (int dz = -radius; dz <= radius; dz++)
                    {
                        int dist = Math.Abs(dx) + Math.Abs(dy) + Math.Abs(dz);
                        if (dist > radius + 1) continue;

                        PlaceBlock(chunk, chunkPos,
                            cx + dx,
                            cy + dy,
                            cz + dz,
                            BlockType.Leaves,
                            color,
                            globalPending);
                    }
        }

        // =============================
        // BLOCK PLACEMENT
        // =============================

        private static void PlaceBlock(
            Chunk chunk,
            Vector2i chunkPos,
            int wx, int wy, int wz,
            BlockType type,
            Vector3 color,
            Dictionary<Vector2i, List<PendingBlock>> globalPending)
        {
            // Calculer la position locale dans le chunk actuel
            int lx = wx - chunkPos.X * Chunk.SIZE;
            int lz = wz - chunkPos.Y * Chunk.SIZE;

            // Si dans le chunk actuel, placer directement
            if (lx >= 0 && lx < Chunk.SIZE && lz >= 0 && lz < Chunk.SIZE &&
                wy >= 0 && wy < Chunk.Height)
            {
                if (chunk.GetBlock(lx, wy, lz).Type == BlockType.Air)
                    chunk.SetBlock(lx, wy, lz, type, color);
            }
            else
            {
                // Sinon, calculer quel chunk devrait contenir ce bloc
                Vector2i targetChunk = new Vector2i(
                    (int)Math.Floor((float)wx / Chunk.SIZE),
                    (int)Math.Floor((float)wz / Chunk.SIZE)
                );

                // Debug : afficher quand on ajoute un pending block
                if (type == BlockType.Leaves)
                {
                    Console.WriteLine($"    PendingBlock: World({wx},{wy},{wz}) -> Chunk{targetChunk} (from {chunkPos})");
                }

                // Ajouter aux pending blocks de ce chunk cible
                if (!globalPending.ContainsKey(targetChunk))
                    globalPending[targetChunk] = new List<PendingBlock>();

                globalPending[targetChunk].Add(new PendingBlock(wx, wy, wz, type, color));
            }
        }
    }
}