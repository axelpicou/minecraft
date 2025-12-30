using System;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace minecraft.worldgen
{
    public class CaveGenerator
    {
        private int seed;
        private const int CAVE_REACH = 3; // Portée moyenne pour les performances

        public CaveGenerator(int seed)
        {
            this.seed = seed;
        }

        public void GenerateCaves(Chunk chunk, Vector2i chunkPos)
        {
            int worldBaseX = chunkPos.X * Chunk.SIZE;
            int worldBaseZ = chunkPos.Y * Chunk.SIZE;

            // Vérifier les grottes qui pourraient affecter ce chunk
            for (int offsetX = -CAVE_REACH; offsetX <= CAVE_REACH; offsetX++)
            {
                for (int offsetZ = -CAVE_REACH; offsetZ <= CAVE_REACH; offsetZ++)
                {
                    Vector2i originChunk = new Vector2i(
                        chunkPos.X + offsetX,
                        chunkPos.Y + offsetZ
                    );

                    int originWorldX = originChunk.X * Chunk.SIZE;
                    int originWorldZ = originChunk.Y * Chunk.SIZE;

                    Random caveRandom = new Random(HashPosition(originWorldX, originWorldZ, seed));

                    // Générer plusieurs tentatives de grottes par chunk (style MC 1.7)
                    int caveAttempts = caveRandom.Next(0, 5); // Réduit de 8 à 5

                    for (int attempt = 0; attempt < caveAttempts; attempt++)
                    {
                        float caveChance = (float)caveRandom.NextDouble();

                        // 14% de chance pour chaque tentative
                        if (caveChance < 0.14f)
                        {
                            int startX = originWorldX + caveRandom.Next(Chunk.SIZE);
                            int startY = caveRandom.Next(8, Chunk.Height - 8);
                            int startZ = originWorldZ + caveRandom.Next(Chunk.SIZE);

                            // Différents types de grottes
                            if (caveRandom.NextDouble() < 0.25) // 25% = grandes grottes
                            {
                                GenerateLargeCave(chunk, chunkPos, startX, startY, startZ, caveRandom);
                            }
                            else // 75% = tunnels normaux
                            {
                                float horizontalAngle = (float)(caveRandom.NextDouble() * MathF.PI * 2);
                                float verticalAngle = (float)((caveRandom.NextDouble() - 0.5) * MathF.PI * 0.5);

                                GenerateTunnel(chunk, chunkPos, startX, startY, startZ,
                                              horizontalAngle, verticalAngle, caveRandom);
                            }
                        }
                    }
                }
            }
        }

        private void GenerateLargeCave(Chunk chunk, Vector2i chunkPos, float x, float y, float z, Random rng)
        {
            int roomCount = rng.Next(2, 5);

            for (int i = 0; i < roomCount; i++)
            {
                float roomRadius = (float)(3.0 + rng.NextDouble() * 4.0); // 3-7 blocs
                CarveRoom(chunk, chunkPos, x, y, z, roomRadius);

                // Connecter avec des tunnels
                if (i < roomCount - 1)
                {
                    float angle = (float)(rng.NextDouble() * MathF.PI * 2);
                    float distance = 10f + (float)rng.NextDouble() * 15f;

                    x += MathF.Cos(angle) * distance;
                    z += MathF.Sin(angle) * distance;
                    y += (float)(rng.NextDouble() - 0.5) * 5f;
                }
            }
        }

        private void CarveRoom(Chunk chunk, Vector2i chunkPos, float cx, float cy, float cz, float radius)
        {
            int baseX = chunkPos.X * Chunk.SIZE;
            int baseZ = chunkPos.Y * Chunk.SIZE;

            int localMinX = Math.Max(0, (int)(cx - radius - baseX));
            int localMaxX = Math.Min(Chunk.SIZE - 1, (int)(cx + radius - baseX));
            int localMinZ = Math.Max(0, (int)(cz - radius - baseZ));
            int localMaxZ = Math.Min(Chunk.SIZE - 1, (int)(cz + radius - baseZ));
            int minY = Math.Max(1, (int)(cy - radius));
            int maxY = Math.Min(Chunk.Height - 1, (int)(cy + radius));

            if (localMinX > localMaxX || localMinZ > localMaxZ)
                return;

            float radiusSq = radius * radius;

            for (int lx = localMinX; lx <= localMaxX; lx++)
                for (int y = minY; y <= maxY; y++)
                    for (int lz = localMinZ; lz <= localMaxZ; lz++)
                    {
                        int worldX = baseX + lx;
                        int worldZ = baseZ + lz;

                        float dx = worldX - cx;
                        float dy = y - cy;
                        float dz = worldZ - cz;

                        if (dx * dx + dy * dy + dz * dz <= radiusSq)
                        {
                            chunk.SetBlock(lx, y, lz, BlockType.Air, Vector3.One);
                        }
                    }
        }

        private void GenerateTunnel(Chunk chunk, Vector2i chunkPos, float x, float y, float z,
                                   float horizontalAngle, float verticalAngle, Random rng)
        {
            int maxNodes = rng.Next(40, 80); // Réduit de 100 à 80

            // Liste des branches à générer (pour éviter la récursion)
            List<TunnelBranch> branches = new List<TunnelBranch>();

            for (int currentNode = 0; currentNode < maxNodes; currentNode++)
            {
                // Rayon variable du tunnel
                float baseRadius = 1.5f + (MathF.Sin(currentNode * MathF.PI / maxNodes) * 1.5f);
                float radius = baseRadius * (0.75f + (float)rng.NextDouble() * 0.5f);

                // Direction du mouvement
                float horizontalSpeed = MathF.Cos(verticalAngle);
                float dx = MathF.Cos(horizontalAngle) * horizontalSpeed;
                float dy = MathF.Sin(verticalAngle);
                float dz = MathF.Sin(horizontalAngle) * horizontalSpeed;

                x += dx;
                y += dy;
                z += dz;

                // Limites de hauteur
                if (y < 1) y = 1;
                if (y > Chunk.Height - 5) y = Chunk.Height - 5;

                CarveSphere(chunk, chunkPos, x, y, z, radius);

                // Variation de direction (style minecraft)
                horizontalAngle += (float)(rng.NextDouble() - 0.5) * 0.3f;
                verticalAngle *= 0.92f; // Atténuation progressive
                verticalAngle += (float)(rng.NextDouble() - 0.5) * 0.2f;
                verticalAngle = Math.Clamp(verticalAngle, -1.2f, 1.2f);

                // Chance de créer une branche (5% au lieu de 10%)
                if (rng.NextDouble() < 0.05 && branches.Count < 3) // Max 3 branches
                {
                    float branchAngle = horizontalAngle + (float)((rng.NextDouble() - 0.5) * MathF.PI);
                    float branchVertical = (float)((rng.NextDouble() - 0.5) * 0.5);
                    int branchLength = maxNodes / 4;

                    branches.Add(new TunnelBranch
                    {
                        X = x,
                        Y = y,
                        Z = z,
                        HorizontalAngle = branchAngle,
                        VerticalAngle = branchVertical,
                        MaxNodes = branchLength
                    });
                }
            }

            // Générer les branches de manière itérative
            foreach (var branch in branches)
            {
                GenerateTunnelIterative(chunk, chunkPos, branch.X, branch.Y, branch.Z,
                                       branch.HorizontalAngle, branch.VerticalAngle,
                                       branch.MaxNodes, rng);
            }
        }

        private struct TunnelBranch
        {
            public float X, Y, Z;
            public float HorizontalAngle, VerticalAngle;
            public int MaxNodes;
        }

        private void GenerateTunnelIterative(Chunk chunk, Vector2i chunkPos, float x, float y, float z,
                                            float horizontalAngle, float verticalAngle,
                                            int maxNodes, Random rng)
        {
            for (int currentNode = 0; currentNode < maxNodes; currentNode++)
            {
                // Rayon variable du tunnel
                float baseRadius = 1.5f + (MathF.Sin(currentNode * MathF.PI / maxNodes) * 1.0f);
                float radius = baseRadius * (0.75f + (float)rng.NextDouble() * 0.5f);

                // Direction du mouvement
                float horizontalSpeed = MathF.Cos(verticalAngle);
                float dx = MathF.Cos(horizontalAngle) * horizontalSpeed;
                float dy = MathF.Sin(verticalAngle);
                float dz = MathF.Sin(horizontalAngle) * horizontalSpeed;

                x += dx;
                y += dy;
                z += dz;

                // Limites de hauteur
                if (y < 1) y = 1;
                if (y > Chunk.Height - 5) y = Chunk.Height - 5;

                CarveSphere(chunk, chunkPos, x, y, z, radius);

                // Variation de direction
                horizontalAngle += (float)(rng.NextDouble() - 0.5) * 0.3f;
                verticalAngle *= 0.92f;
                verticalAngle += (float)(rng.NextDouble() - 0.5) * 0.2f;
                verticalAngle = Math.Clamp(verticalAngle, -1.2f, 1.2f);
            }
        }

        private void CarveSphere(Chunk chunk, Vector2i chunkPos, float cx, float cy, float cz, float radius)
        {
            int baseX = chunkPos.X * Chunk.SIZE;
            int baseZ = chunkPos.Y * Chunk.SIZE;

            int localMinX = Math.Max(0, (int)(cx - radius - baseX));
            int localMaxX = Math.Min(Chunk.SIZE - 1, (int)(cx + radius - baseX));
            int localMinZ = Math.Max(0, (int)(cz - radius - baseZ));
            int localMaxZ = Math.Min(Chunk.SIZE - 1, (int)(cz + radius - baseZ));
            int minY = Math.Max(1, (int)(cy - radius));
            int maxY = Math.Min(Chunk.Height - 1, (int)(cy + radius));

            if (localMinX > localMaxX || localMinZ > localMaxZ)
                return;

            float radiusSq = radius * radius;

            for (int lx = localMinX; lx <= localMaxX; lx++)
                for (int y = minY; y <= maxY; y++)
                    for (int lz = localMinZ; lz <= localMaxZ; lz++)
                    {
                        int worldX = baseX + lx;
                        int worldZ = baseZ + lz;

                        float dx = worldX - cx;
                        float dy = y - cy;
                        float dz = worldZ - cz;

                        if (dx * dx + dy * dy + dz * dz <= radiusSq)
                        {
                            chunk.SetBlock(lx, y, lz, BlockType.Air, Vector3.One);
                        }
                    }
        }

        private float GetRandomFloat(int x, int z)
        {
            int hash = (x * 374761393 + z * 668265263 + seed) ^ (x * z);
            hash = (hash ^ (hash >> 13)) * 1274126177;
            hash = hash ^ (hash >> 16);
            return (hash & 0x7FFFFFFF) / (float)0x7FFFFFFF;
        }

        private int HashPosition(int x, int z, int seed)
        {
            return x * 374761393 + z * 668265263 + seed;
        }
    }
}