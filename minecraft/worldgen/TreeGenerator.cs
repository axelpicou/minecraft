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
            Random rng)
        {
            int height = rng.Next(5, 8);

            for (int i = 0; i < height; i++)
            {
                PlaceBlock(chunk, chunkPos, x, y + i, z, BlockType.Log);
            }

            int top = y + height;
            int radius = 2;

            for (int dx = -radius; dx <= radius; dx++)
                for (int dy = -radius; dy <= radius; dy++)
                    for (int dz = -radius; dz <= radius; dz++)
                    {
                        if (Math.Abs(dx) + Math.Abs(dy) + Math.Abs(dz) > 3)
                            continue;

                        PlaceBlock(chunk, chunkPos,
                            x + dx,
                            top + dy,
                            z + dz,
                            BlockType.Leaves);
                    }
        }

        private static void PlaceBlock(
            Chunk chunk,
            Vector2i chunkPos,
            int lx, int ly, int lz,
            BlockType type)
        {
            int wx = chunkPos.X * Chunk.SIZE + lx;
            int wz = chunkPos.Y * Chunk.SIZE + lz;

            if (chunk.IsInside(lx, ly, lz))
            {
                if (chunk.GetBlock(lx, ly, lz).Type == BlockType.Air)
                    chunk.SetBlock(lx, ly, lz, type);
            }
            else
            {
                chunk.PendingBlocks.Add(
                    new PendingBlock(wx, ly, wz, type)
                );
            }
        }
    }
}
