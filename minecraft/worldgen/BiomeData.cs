namespace minecraft.worldgen
{
    public class BiomeData
    {
        public BiomeType Type { get; set; }
        public string Name { get; set; }

        // Paramètres de hauteur
        public int BaseHeight { get; set; }
        public int HeightVariation { get; set; }
        public float HeightScale { get; set; }

        // Paramètres de température et humidité
        public float Temperature { get; set; }
        public float Humidity { get; set; }

        // Blocs du biome
        public BlockType SurfaceBlock { get; set; }
        public BlockType SubsurfaceBlock { get; set; }
        public BlockType StoneBlock { get; set; }

        // Paramètres de génération
        public int SubsurfaceDepth { get; set; }
        public float TreeDensity { get; set; }

        public BiomeData(BiomeType type, string name)
        {
            Type = type;
            Name = name;
        }

        public static BiomeData CreatePlains()
        {
            return new BiomeData(BiomeType.Plains, "Plains")
            {
                BaseHeight = 4,
                HeightVariation = 3,
                HeightScale = 0.05f,
                Temperature = 0.6f,
                Humidity = 0.5f,
                SurfaceBlock = BlockType.Grass,
                SubsurfaceBlock = BlockType.Dirt,
                StoneBlock = BlockType.Stone,
                SubsurfaceDepth = 3,
                TreeDensity = 0.05f
            };
        }

        public static BiomeData CreateDesert()
        {
            return new BiomeData(BiomeType.Desert, "Desert")
            {
                BaseHeight = 3,
                HeightVariation = 4,
                HeightScale = 0.08f,
                Temperature = 0.95f,
                Humidity = 0.1f,
                SurfaceBlock = BlockType.Dirt,
                SubsurfaceBlock = BlockType.Dirt,
                StoneBlock = BlockType.Stone,
                SubsurfaceDepth = 4,
                TreeDensity = 0.0f
            };
        }

        public static BiomeData CreateForest()
        {
            return new BiomeData(BiomeType.Forest, "Forest")
            {
                BaseHeight = 5,
                HeightVariation = 4,
                HeightScale = 0.06f,
                Temperature = 0.5f,
                Humidity = 0.7f,
                SurfaceBlock = BlockType.Grass,
                SubsurfaceBlock = BlockType.Dirt,
                StoneBlock = BlockType.Stone,
                SubsurfaceDepth = 3,
                TreeDensity = 0.3f
            };
        }

        public static BiomeData CreateMountains()
        {
            return new BiomeData(BiomeType.Mountains, "Mountains")
            {
                BaseHeight = 8,
                HeightVariation = 10,
                HeightScale = 0.03f,
                Temperature = 0.2f,
                Humidity = 0.4f,
                SurfaceBlock = BlockType.Stone,
                SubsurfaceBlock = BlockType.Stone,
                StoneBlock = BlockType.Stone,
                SubsurfaceDepth = 1,
                TreeDensity = 0.01f
            };
        }

        public static BiomeData CreateOcean()
        {
            return new BiomeData(BiomeType.Ocean, "Ocean")
            {
                BaseHeight = 0,
                HeightVariation = 2,
                HeightScale = 0.1f,
                Temperature = 0.5f,
                Humidity = 1.0f,
                SurfaceBlock = BlockType.Dirt,
                SubsurfaceBlock = BlockType.Dirt,
                StoneBlock = BlockType.Stone,
                SubsurfaceDepth = 3,
                TreeDensity = 0.0f
            };
        }

        public static BiomeData CreateTundra()
        {
            return new BiomeData(BiomeType.Tundra, "Tundra")
            {
                BaseHeight = 4,
                HeightVariation = 2,
                HeightScale = 0.07f,
                Temperature = 0.1f,
                Humidity = 0.3f,
                SurfaceBlock = BlockType.Grass,
                SubsurfaceBlock = BlockType.Dirt,
                StoneBlock = BlockType.Stone,
                SubsurfaceDepth = 2,
                TreeDensity = 0.02f
            };
        }

        public static BiomeData CreateSwamp()
        {
            return new BiomeData(BiomeType.Swamp, "Swamp")
            {
                BaseHeight = 2,
                HeightVariation = 2,
                HeightScale = 0.1f,
                Temperature = 0.7f,
                Humidity = 0.9f,
                SurfaceBlock = BlockType.Grass,
                SubsurfaceBlock = BlockType.Dirt,
                StoneBlock = BlockType.Stone,
                SubsurfaceDepth = 2,
                TreeDensity = 0.15f
            };
        }
    }
}