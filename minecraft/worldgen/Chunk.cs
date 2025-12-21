using OpenTK.Mathematics;



namespace minecraft.worldgen
{
    public class Chunk
    {
        public const int Width = 16;
        public const int Height = 16;
        public const int Depth = 16;

        private BlockData[,,] blocks = new BlockData[Width, Height, Depth];

        public BlockData GetBlock(int x, int y, int z) => blocks[x, y, z];

        public void SetBlock(int x, int y, int z, BlockData block) => blocks[x, y, z] = block;

        public void GenerateTerrain(Vector3 worldPos)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int z = 0; z < Depth; z++)
                {
                    // Coordonnées globales pour le Perlin Noise
                    float globalX = x + worldPos.X;
                    float globalZ = z + worldPos.Z;

                    // Perlin Noise
                    float height = PerlinNoise.Noise(globalX * 0.1f, globalZ * 0.1f) * Height;
                    int h = Math.Clamp((int)height, 0, Height - 1);

                    for (int y = 0; y <= h; y++)
                    {
                        blocks[x, y, z] = new BlockData() { Type = 1 }; // type 1 = terre
                    }

                    // les autres restent vides (type 0)
                }
            }
        }

        public static class PerlinNoise
        {
            public static float Noise(float x, float y)
            {
                int n = (int)x + (int)y * 57;
                n = n << 13 ^ n;
                return 1.0f - (n * (n * n * 15731 + 789221) + 1376312589 & 0x7fffffff) / 1073741824.0f;
            }
        }

        public bool IsFaceVisible(int x, int y, int z, Vector3 direction)
        {
            int nx = x + (int)direction.X;
            int ny = y + (int)direction.Y;
            int nz = z + (int)direction.Z;

            // Hors du chunk = visible
            if (nx < 0 || nx >= Width || ny < 0 || ny >= Height || nz < 0 || nz >= Depth)
                return true;

            return blocks[nx, ny, nz].Type == 0; // si vide = visible
        }

    }
}
