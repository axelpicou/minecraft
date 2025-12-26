using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace minecraft.worldgen
{
    public class WorldGenerator
    {
        private Dictionary<BiomeType, BiomeData> biomes;
        private int seed;

        // Paramètres de bruit pour la sélection de biome
        private float biomeScale = 0.008f;
        private float temperatureScale = 0.012f;
        private float humidityScale = 0.01f;

        private float blendRadius = 32f; // Distance pour transition

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
                { BiomeType.Desert, BiomeData.CreateDesert() },
                { BiomeType.Forest, BiomeData.CreateForest() },
                { BiomeType.Mountains, BiomeData.CreateMountains() },
                { BiomeType.Ocean, BiomeData.CreateOcean() },
                { BiomeType.Tundra, BiomeData.CreateTundra() },
                { BiomeType.Swamp, BiomeData.CreateSwamp() }
            };
        }

        // =========================================
        // GENERATION CHUNK
        // =========================================
        public void GenerateChunkTerrain(Chunk chunk, Vector2i chunkPos)
        {
            for (int x = 0; x < Chunk.SIZE; x++)
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

            // ✅ Génération des arbres
            GenerateTrees(chunk, chunkPos);

            // ✅ Appliquer les pending blocks (feuilles hors chunk)
            chunk.ApplyPendingBlocks(chunkPos);
        }

        // =========================================
        // TREE GENERATION
        // =========================================
        private void GenerateTrees(Chunk chunk, Vector2i chunkPos)
        {
            Random rng = new Random(seed ^ (chunkPos.X * 73856093) ^ (chunkPos.Y * 19349663));

            for (int x = 1; x < Chunk.SIZE - 1; x++)
                for (int z = 1; z < Chunk.SIZE - 1; z++)
                {
                    if (rng.NextDouble() > 0.02)
                        continue;

                    int y = FindSurface(chunk, x, z);
                    if (y < 0)
                        continue;

                    if (chunk.GetBlock(x, y, z).Type != BlockType.Grass)
                        continue;

                    BiomeData biome = GetBiomeAtPoint(
                        chunkPos.X * Chunk.SIZE + x,
                        chunkPos.Y * Chunk.SIZE + z
                    );

                    if (biome.TreeDensity <= 0f || rng.NextDouble() > biome.TreeDensity)
                        continue;

                    TreeGenerator.GenerateTree(chunk, chunkPos, x, y + 1, z, rng, biome);

                }
        }

        private int FindSurface(Chunk chunk, int x, int z)
        {
            for (int y = Chunk.Height - 2; y >= 1; y--)
            {
                if (chunk.GetBlock(x, y, z).Type != BlockType.Air &&
                    chunk.GetBlock(x, y + 1, z).Type == BlockType.Air)
                {
                    return y;
                }
            }
            return -1;
        }

        // =========================================
        // BIOME INTERPOLATION
        // =========================================
        private Vector3 GetBlendedGrassColor(BlendedBiome blendedBiome)
        {
            Vector3 color = Vector3.Zero;
            foreach (var kvp in blendedBiome.weights)
            {
                BiomeData biome = biomes[kvp.Key];
                color += biome.GrassColor * kvp.Value;
            }
            return color;
        }

        private struct BlendedBiome
        {
            public BiomeData dominant;
            public Dictionary<BiomeType, float> weights;
        }

        private BlendedBiome GetBlendedBiomeAt(int worldX, int worldZ)
        {
            Dictionary<BiomeType, float> biomeWeights = new();
            int sampleRadius = (int)(blendRadius / 16f);
            float totalWeight = 0f;

            for (int dx = -sampleRadius; dx <= sampleRadius; dx++)
                for (int dz = -sampleRadius; dz <= sampleRadius; dz++)
                {
                    int sampleX = worldX + dx * 8;
                    int sampleZ = worldZ + dz * 8;

                    float distance = MathF.Sqrt(dx * dx * 64 + dz * dz * 64);
                    float weight = Math.Max(0f, 1f - (distance / blendRadius));
                    weight *= weight;

                    if (weight > 0.01f)
                    {
                        BiomeData biome = GetBiomeAtPoint(sampleX, sampleZ);

                        if (!biomeWeights.ContainsKey(biome.Type))
                            biomeWeights[biome.Type] = 0f;

                        biomeWeights[biome.Type] += weight;
                        totalWeight += weight;
                    }
                }

            if (totalWeight > 0f)
            {
                var keys = new List<BiomeType>(biomeWeights.Keys);
                foreach (var key in keys)
                    biomeWeights[key] /= totalWeight;
            }

            BiomeType dominantType = BiomeType.Plains;
            float maxWeight = 0f;
            foreach (var kvp in biomeWeights)
            {
                if (kvp.Value > maxWeight)
                {
                    maxWeight = kvp.Value;
                    dominantType = kvp.Key;
                }
            }

            return new BlendedBiome
            {
                dominant = biomes[dominantType],
                weights = biomeWeights
            };
        }

        private BiomeData GetBiomeAtPoint(int worldX, int worldZ)
        {
            float temperature = GenerateNoise(worldX, worldZ, temperatureScale, seed);
            float humidity = GenerateNoise(worldX, worldZ, humidityScale, seed + 1000);

            temperature = (temperature + 1f) * 0.5f;
            humidity = (humidity + 1f) * 0.5f;

            return SelectBiome(temperature, humidity);
        }

        private BiomeData SelectBiome(float temperature, float humidity)
        {
            if (temperature < 0.25f && humidity > 0.65f) return biomes[BiomeType.Ocean];
            if (temperature < 0.3f) return biomes[BiomeType.Tundra];
            if (temperature < 0.45f && humidity < 0.5f) return biomes[BiomeType.Mountains];
            if (temperature > 0.7f && humidity < 0.35f) return biomes[BiomeType.Desert];
            if (humidity > 0.75f && temperature > 0.5f && temperature < 0.75f) return biomes[BiomeType.Swamp];
            if (temperature > 0.4f && temperature < 0.7f && humidity > 0.5f) return biomes[BiomeType.Forest];

            return biomes[BiomeType.Plains];
        }

        private int GenerateBlendedHeight(int worldX, int worldZ, BlendedBiome blendedBiome)
        {
            float totalHeight = 0f;
            foreach (var kvp in blendedBiome.weights)
            {
                BiomeData biome = biomes[kvp.Key];
                float biomeHeight = GenerateHeightForBiome(worldX, worldZ, biome);
                totalHeight += biomeHeight * kvp.Value;
            }
            return (int)totalHeight;
        }

        private float GenerateHeightForBiome(int worldX, int worldZ, BiomeData biome)
        {
            float noise = 0f;
            float amplitude = 1f;
            float frequency = biome.HeightScale;
            float maxValue = 0f;
            int octaves = 4;
            float persistence = 0.5f;
            float lacunarity = 2f;

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

        private float GenerateNoise(int x, int z, float scale, int seedOffset)
        {
            float sampleX = x * scale;
            float sampleZ = z * scale;

            float noise = MathF.Sin(sampleX + seedOffset) * MathF.Cos(sampleZ + seedOffset);
            noise += MathF.Sin(sampleX * 2 + seedOffset) * MathF.Cos(sampleZ * 2 + seedOffset) * 0.5f;
            noise += MathF.Sin(sampleX * 4 + seedOffset) * MathF.Cos(sampleZ * 4 + seedOffset) * 0.25f;

            return noise / 1.75f;
        }

        public void SetBiomeScale(float scale) => biomeScale = scale;
        public void SetTemperatureScale(float scale) => temperatureScale = scale;
        public void SetHumidityScale(float scale) => humidityScale = scale;
        public void SetBlendRadius(float radius) => blendRadius = radius;
        public void SetSeed(int newSeed) => seed = newSeed;

        public BiomeData GetBiome(BiomeType type) => biomes[type];
    }
}
