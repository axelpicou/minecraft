using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace minecraft.worldgen
{
    public static class TreeGenerator
    {
        public static void GenerateTree(
            Chunk chunk,
            Vector2i chunkPos,
            int x, int y, int z,
            Random rng,
            BiomeData biome)
        {
            int minHeight = 5;
            int maxHeight = 8;

            // Ajustement par biome
            if (biome.Type == BiomeType.Forest) { minHeight = 5; maxHeight = 9; }
            if (biome.Type == BiomeType.Swamp) { minHeight = 4; maxHeight = 6; }
            if (biome.Type == BiomeType.Tundra) { minHeight = 3; maxHeight = 5; }

            int height = rng.Next(minHeight, maxHeight + 1);

            // 🔹 Tronc
            for (int i = 0; i < height; i++)
            {
                PlaceBlock(chunk, chunkPos, x, y + i, z, BlockType.Log);
            }

            int top = y + height;
            int radius = 2;

            // 🔹 Feuilles
            Vector3 leafColor = biome.GrassColor; // coloré selon biome

            for (int dx = -radius; dx <= radius; dx++)
                for (int dy = -radius; dy <= radius; dy++)
                    for (int dz = -radius; dz <= radius; dz++)
                    {
                        int dist = Math.Abs(dx) + Math.Abs(dy) + Math.Abs(dz);
                        if (dist > 3) continue;

                        PlaceBlock(chunk, chunkPos,
                            x + dx,
                            top + dy,
                            z + dz,
                            BlockType.Leaves,
                            leafColor);
                    }
        }

        private static void PlaceBlock(
            Chunk chunk,
            Vector2i chunkPos,
            int lx, int ly, int lz,
            BlockType type,
            Vector3? color = null)
        {
            int wx = chunkPos.X * Chunk.SIZE + lx;
            int wz = chunkPos.Y * Chunk.SIZE + lz;

            if (chunk.IsInside(lx, ly, lz))
            {
                if (chunk.GetBlock(lx, ly, lz).Type == BlockType.Air)
                    chunk.SetBlock(lx, ly, lz, type, color ?? Vector3.One);
            }
            else
            {
                // Stocke pending block si hors chunk
                chunk.PendingBlocks.Add(new PendingBlock(wx, ly, wz, type));
            }
        }
    }
}
