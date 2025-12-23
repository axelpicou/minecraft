using System.Collections.Generic;
using OpenTK.Mathematics;

namespace minecraft.worldgen
{
    public class World
    {
        public List<Chunk> Chunks { get; private set; } = new();
        public List<Vector3> ChunkPositions { get; private set; } = new();

        private int gridWidth;
        private int gridDepth;

        private Block blockTemplate;

        public World(Block blockTemplate, int width = 10, int depth = 10)
        {
            this.blockTemplate = blockTemplate;
            gridWidth = width;
            gridDepth = depth;

            GenerateWorld();
        }

        private void GenerateWorld()
        {
            for (int cx = 0; cx < gridWidth; cx++)
            {
                for (int cz = 0; cz < gridDepth; cz++)
                {
                    Chunk chunk = new Chunk();

                    // Position monde (rendu)
                    Vector3 chunkWorldPos = new Vector3(
                        cx * Chunk.SIZE,
                        0,
                        cz * Chunk.SIZE
                    );

                    // Coordonnées chunk (logique)
                    Vector2i chunkCoord = new Vector2i(cx, cz);

                    chunk.GenerateTerrain(chunkCoord);
                    chunk.BuildMesh(blockTemplate);

                    Chunks.Add(chunk);
                    ChunkPositions.Add(chunkWorldPos);
                }
            }
        }
    }
}
