using System.Collections.Generic;
using OpenTK.Mathematics;

namespace minecraft.worldgen
{
    public class World
    {
        public List<Chunk> Chunks { get; private set; } = new List<Chunk>();
        public List<Vector3> ChunkPositions { get; private set; } = new List<Vector3>(); // OK

        private int gridwidth;
        private int griddepth;

        public World(int width = 10, int depth = 10)
        {
            gridwidth = width;
            griddepth = depth;
            GenerateWorld();
        }

        private void GenerateWorld()
        {
            for (int cx = 0; cx < gridwidth; cx++)
            {
                for (int cz = 0; cz < griddepth; cz++)
                {
                    Chunk chunk = new Chunk();
                    Vector3 chunkPos = new Vector3(cx * Chunk.SIZE, 0, cz * Chunk.SIZE);
                    chunk.GenerateTerrain(chunkPos); // ça utilise OpenTK.Mathematics.Vector3
                    Chunks.Add(chunk);
                    ChunkPositions.Add(chunkPos);
                }
            }
        }
    }
}
