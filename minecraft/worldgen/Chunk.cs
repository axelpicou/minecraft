using OpenTK.Mathematics;
using System.Collections.Generic;

namespace minecraft.worldgen
{
    public class Chunk
    {
        public const int SIZE = 16;
        public const int Height = 256;
        public const int ATLAS_TILES = 16;

        private BlockData[,,] blocks = new BlockData[SIZE, Height, SIZE];
        public ChunkMesh Mesh { get; private set; } = new ChunkMesh();

        public bool NeedsMeshRebuild = true;

        public BlockData GetBlock(int x, int y, int z)
            => blocks[x, y, z];

        // === VERSION COMPLÈTE (utilisée par la génération lumière) ===
        public void SetBlock(
            int x, int y, int z,
            BlockType type,
            Vector3 biomeColor,
            byte light
        )
        {
            if (!IsInside(x, y, z)) return;
            blocks[x, y, z] = new BlockData(type, light, biomeColor);
            NeedsMeshRebuild = true;
        }

        // === SURCHARGE SIMPLE (AIR, caves, suppression) ===
        public void SetBlock(
            int x, int y, int z,
            BlockType type
        )
        {
            SetBlock(x, y, z, type, Vector3.One, 0);
        }

        // === SURCHARGE BIOME (terrain, arbres) ===
        public void SetBlock(
            int x, int y, int z,
            BlockType type,
            Vector3 biomeColor
        )
        {
            SetBlock(x, y, z, type, biomeColor, 15); // ☀️ lumière ciel par défaut
        }


        // ============================================================
        // BUILD MESH (ULTRA OPTIMISÉ)
        // ============================================================
        public void BuildMesh(World world, Block blockTemplate, Vector2i chunkPos)
        {
            List<float> vertices = new();
            List<uint> indices = new();
            uint offset = 0;

            float GetFaceLight(BlockFace face) => face switch
            {
                BlockFace.Top => 1.00f,
                BlockFace.Front => 0.80f,
                BlockFace.Back => 0.80f,
                BlockFace.Left => 0.75f,
                BlockFace.Right => 0.75f,
                BlockFace.Bottom => 0.50f,
                _ => 1.0f
            };

            for (int x = 0; x < SIZE; x++)
                for (int y = 0; y < Height; y++)
                    for (int z = 0; z < SIZE; z++)
                    {
                        BlockData b = blocks[x, y, z];
                        if (b.Type == BlockType.Air) continue;

                        BlockDefinition def = BlockRegistry.Get(b.Type);

                        int wx = chunkPos.X * SIZE + x;
                        int wz = chunkPos.Y * SIZE + z;
                        Vector3 pos = new(wx, y, wz);

                        bool IsAir(int gx, int gy, int gz)
                            => world.GetBlockGlobal(gx, gy, gz).Type == BlockType.Air;

                        float skyLight = Math.Clamp(b.Light / 15f, 0.25f, 1.0f);

                        // ======================
                        // FACES SIMPLES
                        // ======================
                        void SimpleFace(BlockFace face, bool visible, int tex)
                        {
                            if (!visible) return;

                            Vector3 baseColor = Vector3.One;

                            if (b.Type == BlockType.Leaves)
                                baseColor = b.BiomeColor;

                            float ao = ComputeFaceAO(world, wx, y, wz, face);

                            Vector3 finalColor =
                                baseColor *
                                GetFaceLight(face) *
                                skyLight *
                                ao;

                            AddFace(vertices, indices, ref offset,
                                blockTemplate, pos,
                                face, tex, finalColor);
                        }

                        // ======================
                        // TOP FACE → AO PAR SOMMET
                        // ======================
                        void TopFace(bool visible)
                        {
                            if (!visible) return;

                            Vector3 baseColor =
                                b.Type == BlockType.Grass ? b.BiomeColor :
                                b.Type == BlockType.Leaves ? b.BiomeColor :
                                Vector3.One;

                            float[] ao = ComputeTopFaceAO(world, wx, y, wz);
                            float faceLight = GetFaceLight(BlockFace.Top);

                            Vector3[] colors = new Vector3[4];
                            for (int i = 0; i < 4; i++)
                                colors[i] = baseColor * ao[i] * faceLight * skyLight;

                            AddFaceVertexAO(vertices, indices, ref offset,
                                blockTemplate, pos,
                                BlockFace.Top,
                                def.GetTexture(BlockFace.Top),
                                colors);
                        }

                        // === DISPATCH ===
                        SimpleFace(BlockFace.Front, IsAir(wx, y, wz + 1), def.GetTexture(BlockFace.Front));
                        SimpleFace(BlockFace.Back, IsAir(wx, y, wz - 1), def.GetTexture(BlockFace.Back));
                        SimpleFace(BlockFace.Left, IsAir(wx - 1, y, wz), def.GetTexture(BlockFace.Left));
                        SimpleFace(BlockFace.Right, IsAir(wx + 1, y, wz), def.GetTexture(BlockFace.Right));
                        TopFace(IsAir(wx, y + 1, wz));
                        SimpleFace(BlockFace.Bottom, IsAir(wx, y - 1, wz), def.GetTexture(BlockFace.Bottom));
                    }

            Mesh.Build(vertices.ToArray(), indices.ToArray());
            NeedsMeshRebuild = false;
        }

        // ============================================================
        // AO (LOCAL, PAS CHER)
        // ============================================================
        private float ComputeFaceAO(World world, int x, int y, int z, BlockFace face)
        {
            int solid = 0;

            Vector3i[] checks = face switch
            {
                BlockFace.Front => new[] { new Vector3i(0, 0, 1), new Vector3i(-1, 0, 1), new Vector3i(1, 0, 1) },
                BlockFace.Back => new[] { new Vector3i(0, 0, -1), new Vector3i(-1, 0, -1), new Vector3i(1, 0, -1) },
                BlockFace.Left => new[] { new Vector3i(-1, 0, 0), new Vector3i(-1, 0, -1), new Vector3i(-1, 0, 1) },
                BlockFace.Right => new[] { new Vector3i(1, 0, 0), new Vector3i(1, 0, -1), new Vector3i(1, 0, 1) },
                BlockFace.Bottom => new[] { new Vector3i(0, -1, 0) },
                _ => null
            };

            if (checks != null)
            {
                foreach (var d in checks)
                    if (world.GetBlockGlobal(x + d.X, y + d.Y, z + d.Z).Type != BlockType.Air)
                        solid++;
            }

            return solid switch
            {
                0 => 1.0f,
                1 => 0.85f,
                2 => 0.7f,
                _ => 0.55f
            };
        }

        private float ComputeVertexAO(bool side1, bool side2, bool corner)
        {
            if (side1 && side2) return 0.55f;
            int occ = (side1 ? 1 : 0) + (side2 ? 1 : 0) + (corner ? 1 : 0);
            return occ switch
            {
                0 => 1.0f,
                1 => 0.85f,
                2 => 0.7f,
                _ => 0.55f
            };
        }

        private float[] ComputeTopFaceAO(World world, int x, int y, int z)
        {
            return new[]
            {
                ComputeVertexAO(
                    world.GetBlockGlobal(x - 1, y + 1, z).Type != BlockType.Air,
                    world.GetBlockGlobal(x, y + 1, z + 1).Type != BlockType.Air,
                    world.GetBlockGlobal(x - 1, y + 1, z + 1).Type != BlockType.Air),

                ComputeVertexAO(
                    world.GetBlockGlobal(x + 1, y + 1, z).Type != BlockType.Air,
                    world.GetBlockGlobal(x, y + 1, z + 1).Type != BlockType.Air,
                    world.GetBlockGlobal(x + 1, y + 1, z + 1).Type != BlockType.Air),

                ComputeVertexAO(
                    world.GetBlockGlobal(x + 1, y + 1, z).Type != BlockType.Air,
                    world.GetBlockGlobal(x, y + 1, z - 1).Type != BlockType.Air,
                    world.GetBlockGlobal(x + 1, y + 1, z - 1).Type != BlockType.Air),

                ComputeVertexAO(
                    world.GetBlockGlobal(x - 1, y + 1, z).Type != BlockType.Air,
                    world.GetBlockGlobal(x, y + 1, z - 1).Type != BlockType.Air,
                    world.GetBlockGlobal(x - 1, y + 1, z - 1).Type != BlockType.Air)
            };
        }

        // ============================================================
        // FACE BUILDERS
        // ============================================================
        private void AddFace(
            List<float> verts,
            List<uint> inds,
            ref uint offset,
            Block blockTemplate,
            Vector3 pos,
            BlockFace face,
            int texIndex,
            Vector3 color)
        {
            float tile = 1f / ATLAS_TILES;
            int tx = texIndex % ATLAS_TILES;
            int ty = ATLAS_TILES - 1 - texIndex / ATLAS_TILES;

            verts.AddRange(blockTemplate.GetFaceVerticesWithColor(
                face, pos,
                tx * tile, ty * tile,
                (tx + 1) * tile, (ty + 1) * tile,
                color));

            inds.Add(offset); inds.Add(offset + 1); inds.Add(offset + 2);
            inds.Add(offset + 2); inds.Add(offset + 3); inds.Add(offset);
            offset += 4;
        }

        private void AddFaceVertexAO(
            List<float> verts,
            List<uint> inds,
            ref uint offset,
            Block blockTemplate,
            Vector3 pos,
            BlockFace face,
            int texIndex,
            Vector3[] colors)
        {
            float tile = 1f / ATLAS_TILES;
            int tx = texIndex % ATLAS_TILES;
            int ty = ATLAS_TILES - 1 - texIndex / ATLAS_TILES;

            verts.AddRange(blockTemplate.GetFaceVerticesWithColor(
                face, pos,
                tx * tile, ty * tile,
                (tx + 1) * tile, (ty + 1) * tile,
                colors));

            inds.Add(offset); inds.Add(offset + 1); inds.Add(offset + 2);
            inds.Add(offset + 2); inds.Add(offset + 3); inds.Add(offset);
            offset += 4;
        }

        public bool IsInside(int x, int y, int z)
            => x >= 0 && x < SIZE && y >= 0 && y < Height && z >= 0 && z < SIZE;
    }
}
