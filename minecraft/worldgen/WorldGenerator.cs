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
        private float biomeScale = 0.01f;      // Taille des zones de biome
        private float temperatureScale = 0.02f;
        private float humidityScale = 0.015f;

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

        // Génère le terrain pour un chunk
        public void GenerateChunkTerrain(Chunk chunk, Vector2i chunkPos)
        {
            for (int x = 0; x < Chunk.SIZE; x++)
            {
                for (int z = 0; z < Chunk.SIZE; z++)
                {
                    int worldX = chunkPos.X * Chunk.SIZE + x;
                    int worldZ = chunkPos.Y * Chunk.SIZE + z;

                    // Déterminer le biome pour cette colonne
                    BiomeData biome = GetBiomeAt(worldX, worldZ);

                    // Générer la hauteur basée sur le biome
                    int height = GenerateHeight(worldX, worldZ, biome);
                    height = Math.Clamp(height, 0, Chunk.Height - 1);

                    // Remplir la colonne
                    GenerateColumn(chunk, x, z, height, biome);
                }
            }
        }

        // Détermine le biome en fonction de la température et de l'humidité
        private BiomeData GetBiomeAt(int worldX, int worldZ)
        {
            // Générer température et humidité avec du bruit
            float temperature = GenerateNoise(worldX, worldZ, temperatureScale, seed);
            float humidity = GenerateNoise(worldX, worldZ, humidityScale, seed + 1000);

            // Normaliser entre 0 et 1
            temperature = (temperature + 1f) * 0.5f;
            humidity = (humidity + 1f) * 0.5f;

            // Sélectionner le biome basé sur température/humidité
            return SelectBiome(temperature, humidity);
        }

        // Sélectionne un biome en fonction de température et humidité
        private BiomeData SelectBiome(float temperature, float humidity)
        {
            // Océan (très bas)
            if (temperature < 0.2f && humidity > 0.7f)
                return biomes[BiomeType.Ocean];

            // Toundra (froid)
            if (temperature < 0.3f)
                return biomes[BiomeType.Tundra];

            // Montagnes (altitude)
            if (temperature < 0.4f && humidity < 0.5f)
                return biomes[BiomeType.Mountains];

            // Désert (chaud et sec)
            if (temperature > 0.7f && humidity < 0.3f)
                return biomes[BiomeType.Desert];

            // Marais (humide)
            if (humidity > 0.8f && temperature > 0.5f)
                return biomes[BiomeType.Swamp];

            // Forêt (tempéré et humide)
            if (temperature > 0.4f && temperature < 0.7f && humidity > 0.5f)
                return biomes[BiomeType.Forest];

            // Par défaut : Plaines
            return biomes[BiomeType.Plains];
        }

        // Génère la hauteur du terrain pour une position donnée
        private int GenerateHeight(int worldX, int worldZ, BiomeData biome)
        {
            // Bruit multi-octaves pour plus de détail
            float noise = 0f;
            float amplitude = 1f;
            float frequency = biome.HeightScale;
            float maxValue = 0f;
            int octaves = 4;
            float persistence = 0.5f;
            float lacunarity = 2f;

            for (int o = 0; o < octaves; o++)
            {
                float n = GenerateNoise(worldX, worldZ, frequency, seed + o);
                noise += n * amplitude;
                maxValue += amplitude;

                amplitude *= persistence;
                frequency *= lacunarity;
            }

            // Normaliser [-1, 1] -> [0, 1]
            noise = (noise / maxValue + 1f) * 0.5f;

            // Appliquer les paramètres du biome
            int height = biome.BaseHeight + (int)(noise * biome.HeightVariation);

            return height;
        }

        // Génère une colonne de blocs
        private void GenerateColumn(Chunk chunk, int x, int z, int height, BiomeData biome)
        {
            for (int y = 0; y < Chunk.Height; y++)
            {
                BlockType blockType;

                if (y > height)
                {
                    blockType = BlockType.Air;
                }
                else if (y == height)
                {
                    blockType = biome.SurfaceBlock;
                }
                else if (y >= height - biome.SubsurfaceDepth)
                {
                    blockType = biome.SubsurfaceBlock;
                }
                else
                {
                    blockType = biome.StoneBlock;
                }

                chunk.SetBlock(x, y, z, blockType);
            }
        }

        // Génère du bruit Perlin-like
        private float GenerateNoise(int x, int z, float scale, int seedOffset)
        {
            float sampleX = x * scale;
            float sampleZ = z * scale;

            // Utiliser une combinaison de sin/cos pour simuler du Perlin
            float noise = MathF.Sin(sampleX + seedOffset) * MathF.Cos(sampleZ + seedOffset);
            noise += MathF.Sin(sampleX * 2 + seedOffset) * MathF.Cos(sampleZ * 2 + seedOffset) * 0.5f;
            noise += MathF.Sin(sampleX * 4 + seedOffset) * MathF.Cos(sampleZ * 4 + seedOffset) * 0.25f;

            return noise / 1.75f; // Normaliser approximativement entre -1 et 1
        }

        // Méthodes pour ajuster les paramètres
        public void SetBiomeScale(float scale) => biomeScale = scale;
        public void SetTemperatureScale(float scale) => temperatureScale = scale;
        public void SetHumidityScale(float scale) => humidityScale = scale;
        public void SetSeed(int newSeed) => seed = newSeed;

        // Obtenir un biome spécifique pour modification
        public BiomeData GetBiome(BiomeType type) => biomes[type];
    }
}