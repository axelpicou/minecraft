using OpenTK.Mathematics;
using minecraft.worldgen;
using System;

namespace minecraft.Gameplay
{
    public static class VoxelRaycast
    {
        public static bool Raycast(
            World world,
            Vector3 origin,
            Vector3 direction,
            float maxDistance,
            out Vector3 hitBlock,
            out Vector3Int faceNormal)
        {
            hitBlock = Vector3.Zero;
            faceNormal = new Vector3Int(0, 0, 0);

            direction.Normalize();

            // ✅ Ajout d'un petit epsilon pour éviter les problèmes sur les frontières
            const float EPSILON = 0.00001f;
            origin += direction * EPSILON;

            // Voxel de départ
            Vector3Int voxel = new Vector3Int(
                FloorToInt(origin.X),
                FloorToInt(origin.Y),
                FloorToInt(origin.Z)
            );

            // Direction de progression
            Vector3Int step = new Vector3Int(
                direction.X > 0 ? 1 : -1,
                direction.Y > 0 ? 1 : -1,
                direction.Z > 0 ? 1 : -1
            );

            // Distance pour traverser un voxel
            Vector3 tDelta = new Vector3(
                direction.X == 0 ? float.MaxValue : MathF.Abs(1f / direction.X),
                direction.Y == 0 ? float.MaxValue : MathF.Abs(1f / direction.Y),
                direction.Z == 0 ? float.MaxValue : MathF.Abs(1f / direction.Z)
            );

            // ✅ Calcul correct de tMax pour gérer les coordonnées négatives
            Vector3 tMax = new Vector3(
                CalculateTMax(origin.X, direction.X, voxel.X),
                CalculateTMax(origin.Y, direction.Y, voxel.Y),
                CalculateTMax(origin.Z, direction.Z, voxel.Z)
            );

            float distanceTravelled = 0f;

            while (distanceTravelled <= maxDistance)
            {
                // Test du voxel courant
                Chunk chunk = world.GetChunkAtWorldPos(
                    new Vector3(voxel.X, voxel.Y, voxel.Z)
                );

                if (chunk != null)
                {
                    int lx = MathMod(voxel.X, Chunk.SIZE);
                    int lz = MathMod(voxel.Z, Chunk.SIZE);

                    if (chunk.IsInside(lx, voxel.Y, lz) &&
                        chunk.GetBlock(lx, voxel.Y, lz).Type != BlockType.Air)
                    {
                        // ✅ Retourner le bloc exact en Vector3
                        hitBlock = new Vector3(voxel.X, voxel.Y, voxel.Z);
                        return true;
                    }
                }

                // Avancer vers la prochaine face
                if (tMax.X < tMax.Y && tMax.X < tMax.Z)
                {
                    voxel.X += step.X;
                    distanceTravelled = tMax.X;
                    tMax.X += tDelta.X;
                    faceNormal = new Vector3Int(-step.X, 0, 0);
                }
                else if (tMax.Y < tMax.Z)
                {
                    voxel.Y += step.Y;
                    distanceTravelled = tMax.Y;
                    tMax.Y += tDelta.Y;
                    faceNormal = new Vector3Int(0, -step.Y, 0);
                }
                else
                {
                    voxel.Z += step.Z;
                    distanceTravelled = tMax.Z;
                    tMax.Z += tDelta.Z;
                    faceNormal = new Vector3Int(0, 0, -step.Z);
                }
            }

            return false;
        }

        // ✅ Fonction helper pour calculer tMax correctement
        private static float CalculateTMax(float origin, float direction, int voxel)
        {
            if (direction == 0)
                return float.MaxValue;

            if (direction > 0)
            {
                // Se dirige vers le positif : prochaine face = voxel + 1
                return (voxel + 1 - origin) / direction;
            }
            else
            {
                // Se dirige vers le négatif : prochaine face = voxel (pas voxel - 1)
                return (voxel - origin) / direction;
            }
        }

        // ✅ Floor personnalisé pour éviter les problèmes avec les négatifs
        private static int FloorToInt(float value)
        {
            int i = (int)value;
            return value < i ? i - 1 : i;
        }

        private static int MathMod(int a, int b)
        {
            int r = a % b;
            return r < 0 ? r + b : r;
        }
    }

    public struct Vector3Int
    {
        public int X, Y, Z;

        public Vector3Int(int x, int y, int z)
        {
            X = x; Y = y; Z = z;
        }

        public static Vector3Int operator +(Vector3Int a, Vector3Int b)
            => new Vector3Int(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    }
}