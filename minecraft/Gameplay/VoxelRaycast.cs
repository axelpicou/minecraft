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
            direction.Normalize();

            Vector3 pos = origin;
            Vector3Int voxel = new Vector3Int(
                (int)MathF.Floor(pos.X),
                (int)MathF.Floor(pos.Y),
                (int)MathF.Floor(pos.Z)
            );

            Vector3 deltaDist = new Vector3(
                direction.X == 0 ? float.MaxValue : MathF.Abs(1 / direction.X),
                direction.Y == 0 ? float.MaxValue : MathF.Abs(1 / direction.Y),
                direction.Z == 0 ? float.MaxValue : MathF.Abs(1 / direction.Z)
            );

            Vector3Int step = new Vector3Int(
                direction.X < 0 ? -1 : 1,
                direction.Y < 0 ? -1 : 1,
                direction.Z < 0 ? -1 : 1
            );

            Vector3 sideDist = new Vector3(
                (step.X > 0 ? voxel.X + 1 - pos.X : pos.X - voxel.X) * deltaDist.X,
                (step.Y > 0 ? voxel.Y + 1 - pos.Y : pos.Y - voxel.Y) * deltaDist.Y,
                (step.Z > 0 ? voxel.Z + 1 - pos.Z : pos.Z - voxel.Z) * deltaDist.Z
            );

            float dist = 0f;
            faceNormal = new Vector3Int(0, 0, 0);

            while (dist < maxDistance)
            {
                Chunk chunk = world.GetChunkAtWorldPos(new Vector3(voxel.X, voxel.Y, voxel.Z));
                if (chunk != null)
                {
                    int lx = MathMod(voxel.X, Chunk.SIZE);
                    int lz = MathMod(voxel.Z, Chunk.SIZE);

                    if (chunk.IsInside(lx, voxel.Y, lz) && chunk.GetBlock(lx, voxel.Y, lz).Type != BlockType.Air)
                    {
                        hitBlock = new Vector3(voxel.X, voxel.Y, voxel.Z);
                        return true;
                    }
                }

                // Avance vers la face la plus proche
                if (sideDist.X < sideDist.Y && sideDist.X < sideDist.Z)
                {
                    sideDist.X += deltaDist.X;
                    voxel.X += step.X;
                    faceNormal = new Vector3Int(-step.X, 0, 0);
                }
                else if (sideDist.Y < sideDist.Z)
                {
                    sideDist.Y += deltaDist.Y;
                    voxel.Y += step.Y;
                    faceNormal = new Vector3Int(0, -step.Y, 0);
                }
                else
                {
                    sideDist.Z += deltaDist.Z;
                    voxel.Z += step.Z;
                    faceNormal = new Vector3Int(0, 0, -step.Z);
                }

                dist += 0.01f; // petit incrément
            }

            hitBlock = Vector3.Zero;
            return false;
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
    }
}
