using OpenTK.Mathematics;

namespace minecraft.worldgen
{
    public struct PendingBlock
    {
        public int WorldX;
        public int WorldY;
        public int WorldZ;
        public BlockType Type;
        public Vector3 Color;

        public PendingBlock(int x, int y, int z, BlockType type, Vector3? color = null)
        {
            WorldX = x;
            WorldY = y;
            WorldZ = z;
            Type = type;
            Color = color ?? Vector3.One;
        }
    }
}