using OpenTK.Mathematics;

namespace minecraft.worldgen
{
    public class BiomeData
    {
        public BiomeType Type;
        // Terrain
        public int BaseHeight;
        public int HeightVariation;
        public float HeightScale;
        public BlockType SurfaceBlock;
        public BlockType SubsurfaceBlock;
        public BlockType StoneBlock;
        public int SubsurfaceDepth;
        // Visual
        public Vector3 GrassColor;
        public Vector3 LeavesColor;
        // Trees
        public float TreeDensity;
        public TreeType TreeType;

        // =========================
        // BIOMES
        // =========================

        public static BiomeData CreatePlains()
        {
            return new BiomeData
            {
                Type = BiomeType.Plains,
                BaseHeight = 10,
                HeightVariation = 3,
                HeightScale = 0.01f,
                SurfaceBlock = BlockType.Grass,
                SubsurfaceBlock = BlockType.Dirt,
                StoneBlock = BlockType.Stone,
                SubsurfaceDepth = 4,
                GrassColor = new Vector3(0.5f, 0.8f, 0.3f),
                LeavesColor = new Vector3(0.4f, 0.7f, 0.3f),
                TreeDensity = 0.1f,  // Réduit pour encore moins d'arbres
                TreeType = TreeType.Oak
            };
        }

        public static BiomeData CreateForest()
        {
            return new BiomeData
            {
                Type = BiomeType.Forest,
                BaseHeight = 11,
                HeightVariation = 4,
                HeightScale = 0.012f,
                SurfaceBlock = BlockType.Grass,
                SubsurfaceBlock = BlockType.Dirt,
                StoneBlock = BlockType.Stone,
                SubsurfaceDepth = 4,
                GrassColor = new Vector3(0.3f, 0.7f, 0.2f),
                LeavesColor = new Vector3(0.25f, 0.6f, 0.2f),
                TreeDensity = 1f,  // Réduit pour une forêt moins dense
                TreeType = TreeType.Oak
            };
        }

        public static BiomeData CreateTundra()
        {
            return new BiomeData
            {
                Type = BiomeType.Tundra,
                BaseHeight = 12,
                HeightVariation = 2,
                HeightScale = 0.01f,
                SurfaceBlock = BlockType.Grass,
                SubsurfaceBlock = BlockType.Dirt,
                StoneBlock = BlockType.Stone,
                SubsurfaceDepth = 3,
                GrassColor = new Vector3(0.9f, 0.9f, 0.9f),
                LeavesColor = new Vector3(0.8f, 0.9f, 0.8f),
                TreeDensity = 0.205f,  // Très peu de pins
                TreeType = TreeType.Pine
            };
        }

        public static BiomeData CreateMountains()
        {
            return new BiomeData
            {
                Type = BiomeType.Mountains,
                BaseHeight = 18,
                HeightVariation = 10,
                HeightScale = 0.02f,
                SurfaceBlock = BlockType.Stone,
                SubsurfaceBlock = BlockType.Stone,
                StoneBlock = BlockType.Stone,
                SubsurfaceDepth = 1,
                GrassColor = Vector3.One,
                LeavesColor = Vector3.One,
                TreeDensity = 0f,  // Pas d'arbres sur la roche
                TreeType = TreeType.Pine
            };
        }
    }
}