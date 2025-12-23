using OpenTK.Mathematics;
using System.Collections.Generic;

namespace minecraft.worldgen
{
    public class Chunk
    {
        public const int SIZE = 16;
        public const int Height = 16;

        private BlockData[,,] blocks = new BlockData[SIZE, Height, SIZE];

        public ChunkMesh Mesh { get; private set; } = new ChunkMesh();
        public Vector3 WorldPosition;

        public BlockData GetBlock(int x, int y, int z) => blocks[x, y, z];
        public void SetBlock(int x, int y, int z, BlockData b) => blocks[x, y, z] = b;

        public void GenerateTerrain(Vector3 worldPos)
        {
            WorldPosition = worldPos;
            for (int x = 0; x < SIZE; x++)
                for (int z = 0; z < SIZE; z++)
                {
                    float height = PerlinNoise.Noise((x + worldPos.X) * 0.1f, (z + worldPos.Z) * 0.1f) * Height;
                    int h = Math.Clamp((int)height, 0, Height - 1);

                    for (int y = 0; y <= h; y++)
                        blocks[x, y, z] = new BlockData(BlockType.Dirt);
                }
        }

        private bool IsAir(int x, int y, int z)
        {
            if (x < 0 || x >= SIZE || y < 0 || y >= Height || z < 0 || z >= SIZE)
                return true;
            return blocks[x, y, z].Type == BlockType.Air;
        }

        public void BuildMesh(Block blockTemplate)
        {
            List<float> vertices = new List<float>();
            List<uint> indices = new List<uint>();
            uint offset = 0; // offset des sommets, doit être passé par référence

            for (int x = 0; x < SIZE; x++)
                for (int y = 0; y < Height; y++)
                    for (int z = 0; z < SIZE; z++)
                    {
                        BlockData b = blocks[x, y, z];
                        if (b.Type == BlockType.Air) continue;

                        int texIndex = BlockRegistry.Get(b.Type).TextureIndex;
                        Vector3 pos = new Vector3(x, y, z);

                        // Génération des faces visibles, offset passé par référence
                        AddFace(vertices, indices, ref offset, blockTemplate, pos, BlockFace.Front, IsAir(x, y, z + 1), texIndex);
                        AddFace(vertices, indices, ref offset, blockTemplate, pos, BlockFace.Back, IsAir(x, y, z - 1), texIndex);
                        AddFace(vertices, indices, ref offset, blockTemplate, pos, BlockFace.Left, IsAir(x - 1, y, z), texIndex);
                        AddFace(vertices, indices, ref offset, blockTemplate, pos, BlockFace.Right, IsAir(x + 1, y, z), texIndex);
                        AddFace(vertices, indices, ref offset, blockTemplate, pos, BlockFace.Top, IsAir(x, y + 1, z), texIndex);
                        AddFace(vertices, indices, ref offset, blockTemplate, pos, BlockFace.Bottom, IsAir(x, y - 1, z), texIndex);
                    }

            // Construire le mesh final
            Mesh.Build(vertices.ToArray(), indices.ToArray());
        }


        private void AddFace(List<float> verts, List<uint> inds, ref uint offset, Block blockTemplate, Vector3 pos, BlockFace face, bool visible, int texIndex)
        {
            if (!visible) return;

            float tileSize = 1f / 16f; // atlas size
            int xTile = texIndex % 16;
            int yTile = texIndex / 16;

            float uMin = xTile * tileSize;
            float vMin = yTile * tileSize;
            float uMax = uMin + tileSize;
            float vMax = vMin + tileSize;

            float[] faceVerts = blockTemplate.GetFaceVertices(face, pos, uMin, vMin, uMax, vMax);
            verts.AddRange(faceVerts);

            inds.Add(offset + 0); inds.Add(offset + 1); inds.Add(offset + 2);
            inds.Add(offset + 2); inds.Add(offset + 3); inds.Add(offset + 0);

            offset += 4; // incrémente correctement pour la prochaine face
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
    }
}
