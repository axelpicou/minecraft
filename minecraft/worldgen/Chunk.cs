using OpenTK.Mathematics;
using System.Collections.Generic;
using SimplexNoise;

namespace minecraft.worldgen
{
    public class Chunk
    {
        public const int SIZE = 16;
        public const int Height = 16;
        public const int ATLAS_TILES = 16;

        private BlockData[,,] blocks = new BlockData[SIZE, Height, SIZE];
        public ChunkMesh Mesh { get; private set; } = new ChunkMesh();

        // =============================
        // TERRAIN GENERATION
        // =============================
        public void GenerateTerrain(Vector2i chunkPos)
        {
            const int minHeight = 2;
            const int maxHeight = Height - 1;
            const float scale = 0.1f;   // taille des collines
            const int octaves = 4;
            const float persistence = 0.5f;
            const float lacunarity = 2f;

            for (int x = 0; x < SIZE; x++)
            {
                for (int z = 0; z < SIZE; z++)
                {
                    int worldX = chunkPos.X * SIZE + x;
                    int worldZ = chunkPos.Y * SIZE + z;

                    float noise = 0f;
                    float amplitude = 1f;
                    float frequency = scale;
                    float maxValue = 0f;

                    // Multi-octaves
                    for (int o = 0; o < octaves; o++)
                    {
                        // Fonction mathématique pseudo-perlin : combinaison de sin et cos
                        float n = (float)(Math.Sin(worldX * frequency) + Math.Cos(worldZ * frequency));
                        n /= 2f; // normalisation [-1,1]

                        noise += n * amplitude;
                        maxValue += amplitude;

                        amplitude *= persistence;
                        frequency *= lacunarity;
                    }

                    // Normalisation [0,1]
                    noise /= maxValue;
                    noise = (noise + 1f) * 0.5f;

                    int height = minHeight + (int)(noise * (maxHeight - minHeight));
                    height = Math.Clamp(height, 0, Height);

                    // Remplissage du chunk
                    for (int y = 0; y < Height; y++)
                    {
                        if (y < height - 1)
                            blocks[x, y, z] = new BlockData(BlockType.Dirt);
                        else if (y == height - 1)
                            blocks[x, y, z] = new BlockData(BlockType.Grass);
                        else
                            blocks[x, y, z] = new BlockData(BlockType.Air);
                    }
                }
            }
        }









        // =============================
        // BLOCK ACCESS
        // =============================
        public BlockData GetBlock(int x, int y, int z)
            => blocks[x, y, z];

        private bool IsAir(int x, int y, int z)
        {
            if (x < 0 || x >= SIZE ||
                y < 0 || y >= Height ||
                z < 0 || z >= SIZE)
                return true;

            return blocks[x, y, z].Type == BlockType.Air;
        }

        // =============================
        // MESH BUILDING
        // =============================
        public void BuildMesh(Block blockTemplate)
        {
            List<float> vertices = new();
            List<uint> indices = new();
            uint offset = 0;

            for (int x = 0; x < SIZE; x++)
                for (int y = 0; y < Height; y++)
                    for (int z = 0; z < SIZE; z++)
                    {
                        BlockData b = blocks[x, y, z];
                        if (b.Type == BlockType.Air)
                            continue;

                        BlockDefinition def = BlockRegistry.Get(b.Type);
                        Vector3 pos = new Vector3(x, y, z);

                        AddFace(vertices, indices, ref offset, blockTemplate, pos, BlockFace.Front, IsAir(x, y, z + 1), def.GetTexture(BlockFace.Front));
                        AddFace(vertices, indices, ref offset, blockTemplate, pos, BlockFace.Back, IsAir(x, y, z - 1), def.GetTexture(BlockFace.Back));
                        AddFace(vertices, indices, ref offset, blockTemplate, pos, BlockFace.Left, IsAir(x - 1, y, z), def.GetTexture(BlockFace.Left));
                        AddFace(vertices, indices, ref offset, blockTemplate, pos, BlockFace.Right, IsAir(x + 1, y, z), def.GetTexture(BlockFace.Right));
                        AddFace(vertices, indices, ref offset, blockTemplate, pos, BlockFace.Top, IsAir(x, y + 1, z), def.GetTexture(BlockFace.Top));
                        AddFace(vertices, indices, ref offset, blockTemplate, pos, BlockFace.Bottom, IsAir(x, y - 1, z), def.GetTexture(BlockFace.Bottom));
                    }

            Mesh.Build(vertices.ToArray(), indices.ToArray());
        }

        private void AddFace(
            List<float> verts,
            List<uint> inds,
            ref uint offset,
            Block blockTemplate,
            Vector3 pos,
            BlockFace face,
            bool visible,
            int texIndex)
        {
            if (!visible) return;

            float tileSize = 1f / ATLAS_TILES;

            int xTile = texIndex % ATLAS_TILES;
            int yTile = ATLAS_TILES - 1 - (texIndex / ATLAS_TILES); // Y inversé

            float uMin = xTile * tileSize;
            float vMin = yTile * tileSize;
            float uMax = uMin + tileSize;
            float vMax = vMin + tileSize;

            verts.AddRange(
                blockTemplate.GetFaceVertices(face, pos, uMin, vMin, uMax, vMax)
            );

            inds.Add(offset + 0);
            inds.Add(offset + 1);
            inds.Add(offset + 2);
            inds.Add(offset + 2);
            inds.Add(offset + 3);
            inds.Add(offset + 0);

            offset += 4;
        }
    }
}
