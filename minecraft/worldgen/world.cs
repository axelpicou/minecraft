using System.Collections.Generic;
using OpenTK.Mathematics;
using minecraft.Graphics;
using System.Linq;

namespace minecraft.worldgen
{
    public class World
    {
        // Dictionnaire de chunks actifs
        private Dictionary<Vector2i, Chunk> activeChunks = new();

        // Taille de la zone de génération autour de la caméra
        private const int VIEW_RADIUS = 20; // 5 chunks autour → 10x10

        private Block blockTemplate;

        public World(Block blockTemplate)
        {
            this.blockTemplate = blockTemplate;
        }

        // Appelée à chaque frame pour maintenir les chunks autour de la caméra
        public void Update(Vector3 cameraPosition)
        {
            Vector2i camChunk = new Vector2i(
                (int)MathF.Floor(cameraPosition.X / Chunk.SIZE),
                (int)MathF.Floor(cameraPosition.Z / Chunk.SIZE)
            );

            HashSet<Vector2i> neededChunks = new();

            for (int dx = -VIEW_RADIUS; dx <= VIEW_RADIUS; dx++)
            {
                for (int dz = -VIEW_RADIUS; dz <= VIEW_RADIUS; dz++)
                {
                    Vector2i chunkCoord = new Vector2i(camChunk.X + dx, camChunk.Y + dz);
                    neededChunks.Add(chunkCoord);

                    if (!activeChunks.ContainsKey(chunkCoord))
                    {
                        // Génère et build le chunk
                        Chunk chunk = new Chunk();
                        chunk.GenerateTerrain(chunkCoord);
                        chunk.BuildMesh(blockTemplate, chunkCoord); // ✅ Passer chunkCoord
                        activeChunks.Add(chunkCoord, chunk);
                    }
                }
            }

            // Supprime les chunks trop éloignés
            var toRemove = activeChunks.Keys.Where(c => !neededChunks.Contains(c)).ToList();
            foreach (var c in toRemove)
            {
                activeChunks[c].Mesh.Delete();
                activeChunks.Remove(c);
            }
        }

        // Permet d'itérer sur tous les chunks actifs pour le rendu
        public IEnumerable<(Chunk chunk, Vector3 position)> GetActiveChunks()
        {
            foreach (var kvp in activeChunks)
            {
                Chunk chunk = kvp.Value;

                // ✅ Plus besoin de position : les vertices sont en coordonnées monde
                Vector3 pos = Vector3.Zero;
                yield return (chunk, pos);
            }
        }

        public int GetHeightAt(float worldX, float worldZ)
        {
            Vector2i chunkCoord = new Vector2i(
                (int)MathF.Floor(worldX / Chunk.SIZE),
                (int)MathF.Floor(worldZ / Chunk.SIZE)
            );

            if (!activeChunks.TryGetValue(chunkCoord, out Chunk chunk))
                return 0;

            int localX = (int)(worldX - chunkCoord.X * Chunk.SIZE);
            int localZ = (int)(worldZ - chunkCoord.Y * Chunk.SIZE);

            // Cherche la plus haute bloc non-air dans le chunk
            for (int y = Chunk.Height - 1; y >= 0; y--)
            {
                if (chunk.GetBlock(localX, y, localZ).Type != BlockType.Air)
                    return y + 1; // +1 pour que la caméra soit au-dessus
            }

            return 0;
        }

        public Chunk GetChunkAtWorldPos(Vector3 worldPos)
        {
            Vector2i chunkCoord = new Vector2i(
                (int)MathF.Floor(worldPos.X / Chunk.SIZE),
                (int)MathF.Floor(worldPos.Z / Chunk.SIZE)
            );

            if (activeChunks.TryGetValue(chunkCoord, out Chunk chunk))
                return chunk;
            return null;
        }

        public void SetBlock(Vector2i chunkCoord, Vector3 localPos, BlockType type)
        {
            if (!activeChunks.TryGetValue(chunkCoord, out Chunk chunk))
                return;

            int x = (int)localPos.X;
            int y = (int)localPos.Y;
            int z = (int)localPos.Z;

            if (x < 0 || x >= Chunk.SIZE ||
                y < 0 || y >= Chunk.Height ||
                z < 0 || z >= Chunk.SIZE)
                return;

            chunk.SetBlock(x, y, z, type);

            // IMPORTANT : rebuild du mesh avec les coordonnées du chunk
            chunk.BuildMesh(blockTemplate, chunkCoord); // ✅ Passer chunkCoord
        }
    }
}