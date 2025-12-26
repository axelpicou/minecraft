namespace minecraft.worldgen
{
    public class BiomeData
    {
        public BiomeType Type { get; set; }
        public string Name { get; set; }

        // Paramètres de hauteur
        public int BaseHeight { get; set; }          // Hauteur de base
        public int HeightVariation { get; set; }     // Variation max de hauteur
        public float HeightScale { get; set; }       // Échelle du bruit (petites/grandes collines)

        // Paramètres de température et humidité
        public float Temperature { get; set; }       // 0.0 = froid, 1.0 = chaud
        public float Humidity { get; set; }          // 0.0 = sec, 1.0 = humide

        // Couleurs du biome (RGB 0-255)
        public OpenTK.Mathematics.Vector3 GrassColor { get; set; }  // Couleur de l'herbe
        public OpenTK.Mathematics.Vector3 FoliageColor { get; set; } // Couleur du feuillage (pour futur)

        // Blocs du biome
        public BlockType SurfaceBlock { get; set; }
        public BlockType SubsurfaceBlock { get; set; }
        public BlockType StoneBlock { get; set; }

        // Paramètres de génération
        public int SubsurfaceDepth { get; set; }     // Profondeur de la couche sous la surface
        public float TreeDensity { get; set; }       // 0.0 = pas d'arbres, 1.0 = dense

        public BiomeData(BiomeType type, string name)
        {
            Type = type;
            Name = name;
        }

        // Factory method pour créer des biomes prédéfinis
        public static BiomeData CreatePlains()
        {
            return new BiomeData(BiomeType.Plains, "Plains")
            {
                BaseHeight = 4,
                HeightVariation = 3,
                HeightScale = 0.05f,
                Temperature = 0.6f,
                Humidity = 0.5f,
                GrassColor = new OpenTK.Mathematics.Vector3(91, 181, 51) / 255f,  // Vert vif
                FoliageColor = new OpenTK.Mathematics.Vector3(77, 153, 51) / 255f,
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
                GrassColor = new OpenTK.Mathematics.Vector3(191, 183, 85) / 255f,  // Jaune-vert
                FoliageColor = new OpenTK.Mathematics.Vector3(174, 164, 42) / 255f,
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
                GrassColor = new OpenTK.Mathematics.Vector3(79, 171, 47) / 255f,  // Vert forêt
                FoliageColor = new OpenTK.Mathematics.Vector3(59, 138, 31) / 255f,
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
                GrassColor = new OpenTK.Mathematics.Vector3(136, 170, 136) / 255f,  // Vert grisâtre
                FoliageColor = new OpenTK.Mathematics.Vector3(96, 128, 96) / 255f,
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
                GrassColor = new OpenTK.Mathematics.Vector3(64, 181, 90) / 255f,  // Vert aquatique
                FoliageColor = new OpenTK.Mathematics.Vector3(51, 153, 77) / 255f,
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
                GrassColor = new OpenTK.Mathematics.Vector3(128, 180, 151) / 255f,  // Vert bleuté
                FoliageColor = new OpenTK.Mathematics.Vector3(96, 153, 119) / 255f,
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
                GrassColor = new OpenTK.Mathematics.Vector3(106, 156, 84) / 255f,  // Vert marais
                FoliageColor = new OpenTK.Mathematics.Vector3(74, 128, 53) / 255f,
                SurfaceBlock = BlockType.Grass,
                SubsurfaceBlock = BlockType.Dirt,
                StoneBlock = BlockType.Stone,
                SubsurfaceDepth = 2,
                TreeDensity = 0.15f
            };
        }
    }
}