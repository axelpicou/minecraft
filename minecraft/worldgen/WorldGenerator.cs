using OpenTK.Mathematics;
using SimplexNoise;
using System;
using System.Collections.Generic;

namespace minecraft.worldgen
{
    public class WorldGenerator
    {
        private Dictionary<BiomeType, BiomeData> biomes;
        private int seed;

        private float biomeScale = 0.008f;
        private float temperatureScale = 0.012f;
        private float humidityScale = 0.01f;
        private float blendRadius = 32f;

        // AJOUT : Stocker les PendingBlocks par chunk
        private Dictionary<Vector2i, List<PendingBlock>> globalPendingBlocks = new();

        public WorldGenerator(int seed = 0)
        {
            this.seed = seed;
            InitializeBiomes();
        }

        private void InitializeBiomes()
        {
            biomes = new Dictionary<BiomeType, BiomeData>
            {
                { BiomeType.Plains, BiomeData.CreatePlains() },
                { BiomeType.Forest, BiomeData.CreateForest() },
                { BiomeType.Tundra, BiomeData.CreateTundra() },
                { BiomeType.Mountains, BiomeData.CreateMountains() ?? BiomeData.CreatePlains() }
            };
        }

        public void GenerateChunkTerrain(Chunk chunk, Vector2i chunkPos)
        {
            // 1. Générer le terrain
            for (int x = 0; x < Chunk.SIZE; x++)
            {
                for (int z = 0; z < Chunk.SIZE; z++)
                {
                    int worldX = chunkPos.X * Chunk.SIZE + x;
                    int worldZ = chunkPos.Y * Chunk.SIZE + z;

                    var blendedBiome = GetBlendedBiomeAt(worldX, worldZ);
                    int height = GenerateBlendedHeight(worldX, worldZ, blendedBiome);
                    height = Math.Clamp(height, 0, Chunk.Height - 1);

                    Vector3 grassColor = GetBlendedGrassColor(blendedBiome);

                    GenerateColumn(chunk, x, z, height, blendedBiome.dominant, grassColor);
                }
            }

            // 2. Appliquer les PendingBlocks des chunks voisins AVANT de générer les arbres
            int appliedBlocks = ApplyPendingBlocksToChunk(chunk, chunkPos);

            // 3. Générer les arbres de ce chunk
            int treesBefore = globalPendingBlocks.Count;
            GenerateTrees(chunk, chunkPos);
            int treesAfter = globalPendingBlocks.Count;

        }

        private void GenerateTrees(Chunk chunk, Vector2i chunkPos)
        {
            // Générer uniquement les arbres dont le TRONC est dans ce chunk
            int startX = chunkPos.X * Chunk.SIZE;
            int endX = chunkPos.X * Chunk.SIZE + Chunk.SIZE - 1;
            int startZ = chunkPos.Y * Chunk.SIZE;
            int endZ = chunkPos.Y * Chunk.SIZE + Chunk.SIZE - 1;

            for (int wx = startX; wx <= endX; wx++)
                for (int wz = startZ; wz <= endZ; wz++)
                {
                    float randomValue = GetPositionHash(wx, wz);
                    if (randomValue > 0.005f)
                        continue;

                    BiomeData biome = GetBiomeAtPoint(wx, wz);
                    if (biome == null || biome.TreeDensity <= 0f)
                        continue;

                    float treeRoll = GetTreeRandomValue(wx, wz);
                    if (treeRoll > biome.TreeDensity)
                        continue;

                    int y = FindActualSurfaceY(chunk, chunkPos, wx, wz);

                    if (y <= 0)
                        continue;

                    if (!IsValidTreePosition(chunk, chunkPos, wx, y, wz))
                        continue;

                    // Générer l'arbre avec stockage global des pending blocks
                    TreeGenerator.GenerateTreeWithPending(
                        chunk,
                        chunkPos,
                        new Vector3i(wx, y + 1, wz),
                        biome,
                        seed,
                        globalPendingBlocks);
                }
        }

        // Applique les pending blocks destinés à ce chunk
        private int ApplyPendingBlocksToChunk(Chunk chunk, Vector2i chunkPos)
        {
            if (!globalPendingBlocks.ContainsKey(chunkPos))
                return 0;

            List<PendingBlock> blocks = globalPendingBlocks[chunkPos];
            int baseX = chunkPos.X * Chunk.SIZE;
            int baseZ = chunkPos.Y * Chunk.SIZE;
            int applied = 0;


            foreach (PendingBlock pb in blocks)
            {
                int lx = pb.WorldX - baseX;
                int lz = pb.WorldZ - baseZ;

                if (lx >= 0 && lx < Chunk.SIZE &&
                    lz >= 0 && lz < Chunk.SIZE &&
                    pb.WorldY >= 0 && pb.WorldY < Chunk.Height)
                {
                    if (chunk.GetBlock(lx, pb.WorldY, lz).Type == BlockType.Air)
                    {
                        chunk.SetBlock(lx, pb.WorldY, lz, pb.Type, pb.Color);
                        applied++;
                    }
                }
            }

            // Nettoyer les pending blocks appliqués
            globalPendingBlocks.Remove(chunkPos);
            return applied;
        }

        // Trouve la vraie surface du terrain
        private int FindActualSurfaceY(Chunk chunk, Vector2i chunkPos, int worldX, int worldZ)
        {
            int localX = worldX - chunkPos.X * Chunk.SIZE;
            int localZ = worldZ - chunkPos.Y * Chunk.SIZE;

            if (localX >= 0 && localX < Chunk.SIZE && localZ >= 0 && localZ < Chunk.SIZE)
            {
                for (int y = Chunk.Height - 1; y >= 0; y--)
                {
                    BlockData block = chunk.GetBlock(localX, y, localZ);
                    if (block.Type != BlockType.Air)
                    {
                        return y;
                    }
                }
                return 0;
            }

            return GetSurfaceYAt(worldX, worldZ);
        }

        // Hash de position pour distribution naturelle
        private float GetPositionHash(int worldX, int worldZ)
        {
            int hash = (worldX * 374761393 + worldZ * 668265263 + seed) ^ (worldX * worldZ);
            hash = (hash ^ (hash >> 13)) * 1274126177;
            hash = hash ^ (hash >> 16);
            return (hash & 0x7FFFFFFF) / (float)0x7FFFFFFF;
        }

        private float GetTreeRandomValue(int worldX, int worldZ)
        {
            int hash = (worldX * 668265263 + worldZ * 374761393 + seed + 12345) ^ (worldZ * worldX);
            hash = (hash ^ (hash >> 13)) * 1274126177;
            hash = hash ^ (hash >> 16);
            return (hash & 0x7FFFFFFF) / (float)0x7FFFFFFF;
        }

        private bool IsValidTreePosition(Chunk chunk, Vector2i chunkPos, int worldX, int y, int worldZ)
        {
            if (y < 5 || y > Chunk.Height - 20)
                return false;

            int localX = worldX - chunkPos.X * Chunk.SIZE;
            int localZ = worldZ - chunkPos.Y * Chunk.SIZE;

            if (localX >= 0 && localX < Chunk.SIZE && localZ >= 0 && localZ < Chunk.SIZE &&
                y >= 0 && y < Chunk.Height)
            {
                BlockData groundBlock = chunk.GetBlock(localX, y, localZ);

                if (groundBlock.Type != BlockType.Grass && groundBlock.Type != BlockType.Dirt)
                    return false;

                if (y + 1 < Chunk.Height)
                {
                    BlockData aboveBlock = chunk.GetBlock(localX, y + 1, localZ);
                    if (aboveBlock.Type != BlockType.Air)
                        return false;
                }

                return true;
            }

            BiomeData biome = GetBiomeAtPoint(worldX, worldZ);
            if (biome.SurfaceBlock != BlockType.Grass && biome.SurfaceBlock != BlockType.Dirt)
                return false;

            return true;
        }

        public int GetSurfaceYAt(int worldX, int worldZ)
        {
            var blendedBiome = GetBlendedBiomeAt(worldX, worldZ);
            int height = GenerateBlendedHeight(worldX, worldZ, blendedBiome);
            return Math.Clamp(height, 1, Chunk.Height - 2);
        }

        private int GenerateBlendedHeight(int worldX, int worldZ, BlendedBiome blendedBiome)
        {
            float totalHeight = 0f;
            foreach (var kvp in blendedBiome.weights)
            {
                BiomeData biome = biomes[kvp.Key];
                totalHeight += GenerateHeightForBiome(worldX, worldZ, biome) * kvp.Value;
            }
            return (int)(totalHeight * 10f);
        }

        private float GenerateHeightForBiome(int worldX, int worldZ, BiomeData biome)
        {
            float noise = 0f, amplitude = 1f, frequency = biome.HeightScale, maxValue = 0f;
            int octaves = 4;
            float persistence = 0.5f, lacunarity = 2f;

            for (int o = 0; o < octaves; o++)
            {
                float n = GenerateNoise(worldX, worldZ, frequency, seed + o + (int)biome.Type * 100);
                noise += n * amplitude;
                maxValue += amplitude;
                amplitude *= persistence;
                frequency *= lacunarity;
            }

            noise = (noise / maxValue + 1f) * 0.5f;
            return biome.BaseHeight + noise * biome.HeightVariation;
        }

        private float GenerateNoise(int x, int z, float scale, int seedOffset)
        {
            float sampleX = x * scale, sampleZ = z * scale;
            float noise = MathF.Sin(sampleX + seedOffset) * MathF.Cos(sampleZ + seedOffset);
            noise += MathF.Sin(sampleX * 2 + seedOffset) * MathF.Cos(sampleZ * 2 + seedOffset) * 0.5f;
            noise += MathF.Sin(sampleX * 4 + seedOffset) * MathF.Cos(sampleZ * 4 + seedOffset) * 0.25f;
            return noise / 1.75f;
        }

        private void GenerateColumn(Chunk chunk, int x, int z, int height, BiomeData biome, Vector3 grassColor)
        {
            for (int y = 0; y < Chunk.Height; y++)
            {
                BlockType blockType;
                if (y > height) blockType = BlockType.Air;
                else if (y == height) blockType = biome.SurfaceBlock;
                else if (y >= height - biome.SubsurfaceDepth) blockType = biome.SubsurfaceBlock;
                else blockType = biome.StoneBlock;

                chunk.SetBlock(x, y, z, blockType, blockType == BlockType.Grass ? grassColor : Vector3.One);
            }
        }

        private Vector3 GetBlendedGrassColor(BlendedBiome blendedBiome)
        {
            Vector3 color = Vector3.Zero;
            foreach (var kvp in blendedBiome.weights)
                color += biomes[kvp.Key].GrassColor * kvp.Value;
            return color;
        }

        private struct BlendedBiome
        {
            public BiomeData dominant;
            public Dictionary<BiomeType, float> weights;
        }

        private BlendedBiome GetBlendedBiomeAt(int worldX, int worldZ)
        {
            Dictionary<BiomeType, float> weights = new();
            int radius = (int)(blendRadius / 16f);
            float totalWeight = 0f;

            for (int dx = -radius; dx <= radius; dx++)
                for (int dz = -radius; dz <= radius; dz++)
                {
                    int sampleX = worldX + dx * 8;
                    int sampleZ = worldZ + dz * 8;

                    float distance = MathF.Sqrt(dx * dx * 64 + dz * dz * 64);
                    float weight = Math.Max(0f, 1f - (distance / blendRadius));
                    weight *= weight;

                    if (weight > 0.01f)
                    {
                        BiomeData biome = GetBiomeAtPoint(sampleX, sampleZ);
                        if (!weights.ContainsKey(biome.Type))
                            weights[biome.Type] = 0f;
                        weights[biome.Type] += weight;
                        totalWeight += weight;
                    }
                }

            if (totalWeight > 0f)
                foreach (var key in weights.Keys.ToList())
                    weights[key] /= totalWeight;

            BiomeType dominantType = BiomeType.Plains;
            float maxWeight = 0f;
            foreach (var kvp in weights)
                if (kvp.Value > maxWeight)
                {
                    maxWeight = kvp.Value;
                    dominantType = kvp.Key;
                }

            return new BlendedBiome
            {
                dominant = biomes[dominantType],
                weights = weights
            };
        }

        private BiomeData GetBiomeAtPoint(int worldX, int worldZ)
        {
            float temp = (GenerateNoise(worldX, worldZ, temperatureScale, seed) + 1f) * 0.5f;
            float hum = (GenerateNoise(worldX, worldZ, humidityScale, seed + 1000) + 1f) * 0.5f;
            return SelectBiome(temp, hum);
        }

        private BiomeData SelectBiome(float temperature, float humidity)
        {
            if (temperature < 0.3f) return biomes[BiomeType.Tundra];
            if (temperature > 0.4f && temperature < 0.7f && humidity > 0.5f) return biomes[BiomeType.Forest];
            return biomes[BiomeType.Plains];
        }

        public BiomeData GetBiome(BiomeType type) => biomes.TryGetValue(type, out var b) ? b : null;

        // NOUVELLE MÉTHODE : Applique tous les PendingBlocks restants aux chunks déjà générés
        public int ApplyAllPendingBlocks(Dictionary<Vector2i, Chunk> loadedChunks, Block blockTemplate)
        {
            int totalApplied = 0;
            List<Vector2i> chunksToRebuild = new();

            foreach (var kvp in globalPendingBlocks.ToList())
            {
                Vector2i chunkPos = kvp.Key;

                // Si ce chunk existe déjà
                if (loadedChunks.ContainsKey(chunkPos))
                {
                    Chunk chunk = loadedChunks[chunkPos];
                    int baseX = chunkPos.X * Chunk.SIZE;
                    int baseZ = chunkPos.Y * Chunk.SIZE;
                    int applied = 0;

                    foreach (PendingBlock pb in kvp.Value)
                    {
                        int lx = pb.WorldX - baseX;
                        int lz = pb.WorldZ - baseZ;

                        if (lx >= 0 && lx < Chunk.SIZE &&
                            lz >= 0 && lz < Chunk.SIZE &&
                            pb.WorldY >= 0 && pb.WorldY < Chunk.Height)
                        {
                            if (chunk.GetBlock(lx, pb.WorldY, lz).Type == BlockType.Air)
                            {
                                chunk.SetBlock(lx, pb.WorldY, lz, pb.Type, pb.Color);
                                applied++;
                            }
                        }
                    }

                    if (applied > 0)
                    {
                        chunksToRebuild.Add(chunkPos);
                        totalApplied += applied;
                    }

                    // Nettoyer les pending blocks appliqués
                    globalPendingBlocks.Remove(chunkPos);
                }
            }

            // Regénérer les mesh des chunks modifiés
            foreach (var chunkPos in chunksToRebuild)
            {
                loadedChunks[chunkPos].BuildMesh(blockTemplate, chunkPos);
            }

            return totalApplied;
        }
    }
}