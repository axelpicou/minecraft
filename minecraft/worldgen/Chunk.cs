using OpenTK.Mathematics;
using System.Collections.Generic;

namespace minecraft.worldgen
{
    public class Chunk
    {
        public const int SIZE = 16;
        public const int Height = 256;
        public const int ATLAS_TILES = 16;

        // AJOUT : Liste des blocs en attente pour les arbres qui débordent
        public List<PendingBlock> PendingBlocks = new();

        private BlockData[,,] blocks = new BlockData[SIZE, Height, SIZE];
        public ChunkMesh Mesh { get; private set; } = new ChunkMesh();

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
        public void BuildMesh(Block blockTemplate, Vector2i chunkPos)
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

                        Vector3 pos = new Vector3(
                            chunkPos.X * SIZE + x,
                            y,
                            chunkPos.Y * SIZE + z
                        );

                        // =============================
                        // 🎨 COULEUR PAR FACE (BONUS)
                        // =============================

                        // Couleur par défaut (neutre)
                        Vector3 white = Vector3.One;

                        // Couleur du biome uniquement pour l'herbe (face TOP)
                        Vector3 grassTopColor =
                            (b.Type == BlockType.Grass) ? b.BiomeColor : Vector3.One;

                        // Couleur pour les feuilles
                        Vector3 leavesColor =
                            (b.Type == BlockType.Leaves) ? b.BiomeColor : Vector3.One;

                        // FRONT
                        AddFace(vertices, indices, ref offset, blockTemplate, pos,
                            BlockFace.Front,
                            IsAir(x, y, z + 1),
                            def.GetTexture(BlockFace.Front),
                            b.Type == BlockType.Leaves ? leavesColor : white
                        );

                        // BACK
                        AddFace(vertices, indices, ref offset, blockTemplate, pos,
                            BlockFace.Back,
                            IsAir(x, y, z - 1),
                            def.GetTexture(BlockFace.Back),
                            b.Type == BlockType.Leaves ? leavesColor : white
                        );

                        // LEFT
                        AddFace(vertices, indices, ref offset, blockTemplate, pos,
                            BlockFace.Left,
                            IsAir(x - 1, y, z),
                            def.GetTexture(BlockFace.Left),
                            b.Type == BlockType.Leaves ? leavesColor : white
                        );

                        // RIGHT
                        AddFace(vertices, indices, ref offset, blockTemplate, pos,
                            BlockFace.Right,
                            IsAir(x + 1, y, z),
                            def.GetTexture(BlockFace.Right),
                            b.Type == BlockType.Leaves ? leavesColor : white
                        );

                        // TOP 🌱 (HERBE TEINTÉE)
                        AddFace(vertices, indices, ref offset, blockTemplate, pos,
                            BlockFace.Top,
                            IsAir(x, y + 1, z),
                            def.GetTexture(BlockFace.Top),
                            b.Type == BlockType.Grass ? grassTopColor : (b.Type == BlockType.Leaves ? leavesColor : white)
                        );

                        // BOTTOM
                        AddFace(vertices, indices, ref offset, blockTemplate, pos,
                            BlockFace.Bottom,
                            IsAir(x, y - 1, z),
                            def.GetTexture(BlockFace.Bottom),
                            b.Type == BlockType.Leaves ? leavesColor : white
                        );
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
            int texIndex,
            Vector3 color)
        {
            if (!visible) return;

            float tileSize = 1f / ATLAS_TILES;

            int xTile = texIndex % ATLAS_TILES;
            int yTile = ATLAS_TILES - 1 - (texIndex / ATLAS_TILES);

            float uMin = xTile * tileSize;
            float vMin = yTile * tileSize;
            float uMax = uMin + tileSize;
            float vMax = vMin + tileSize;

            verts.AddRange(
                blockTemplate.GetFaceVerticesWithColor(
                    face, pos,
                    uMin, vMin, uMax, vMax,
                    color
                )
            );

            inds.Add(offset + 0);
            inds.Add(offset + 1);
            inds.Add(offset + 2);
            inds.Add(offset + 2);
            inds.Add(offset + 3);
            inds.Add(offset + 0);

            offset += 4;
        }

        // =============================
        // SET BLOCK
        // =============================
        public void SetBlock(int x, int y, int z, BlockType type, Vector3 biomeColor = default)
        {
            if (x < 0 || x >= SIZE || y < 0 || y >= Height || z < 0 || z >= SIZE)
                return;

            if (biomeColor == default)
                biomeColor = Vector3.One;

            blocks[x, y, z] = new BlockData(type, 0, biomeColor);
        }

        public bool IsInside(int x, int y, int z)
        {
            return x >= 0 && x < SIZE &&
                   y >= 0 && y < Height &&
                   z >= 0 && z < SIZE;
        }

        // =============================
        // PENDING BLOCKS
        // =============================
        public void ApplyPendingBlocks(Vector2i chunkPos)
        {
            int baseX = chunkPos.X * SIZE;
            int baseZ = chunkPos.Y * SIZE;

            for (int i = PendingBlocks.Count - 1; i >= 0; i--)
            {
                PendingBlock pb = PendingBlocks[i];

                int lx = pb.WorldX - baseX;
                int lz = pb.WorldZ - baseZ;

                if (lx >= 0 && lx < SIZE &&
                    lz >= 0 && lz < SIZE &&
                    pb.WorldY >= 0 && pb.WorldY < Height)
                {
                    // Appliquer avec la couleur stockée
                    SetBlock(lx, pb.WorldY, lz, pb.Type, pb.Color);
                    PendingBlocks.RemoveAt(i);
                }
            }
        }
    }
}