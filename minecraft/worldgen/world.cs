using System.Collections.Generic;
using OpenTK.Mathematics;

namespace minecraft.worldgen
{
    public class World
    {
        public List<Chunk> Chunks { get; private set; } = new List<Chunk>();
        public List<Vector3> ChunkPositions { get; private set; } = new List<Vector3>(); // OK

        private int gridWidth;
        private int gridDepth;

        public World(int width = 10, int depth = 10)
        {
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
                    Vector3 chunkPos = new Vector3(cx * Chunk.Width, 0, cz * Chunk.Depth);
                    chunk.GenerateTerrain(chunkPos); // ça utilise OpenTK.Mathematics.Vector3
                    Chunks.Add(chunk);
                    ChunkPositions.Add(chunkPos);
                }
            }
        }
    }
}
