namespace minecraft.worldgen
{
    public class BlockDefinition
    {
        private readonly int[] faceTextures = new int[6];

        // même texture partout
        public BlockDefinition(int allFaces)
        {
            for (int i = 0; i < 6; i++)
                faceTextures[i] = allFaces;
        }

        // texture différente par face
        public BlockDefinition(
            int front, int back, int left,
            int right, int top, int bottom)
        {
            faceTextures[(int)BlockFace.Front] = front;
            faceTextures[(int)BlockFace.Back] = back;
            faceTextures[(int)BlockFace.Left] = left;
            faceTextures[(int)BlockFace.Right] = right;
            faceTextures[(int)BlockFace.Top] = top;
            faceTextures[(int)BlockFace.Bottom] = bottom;
        }

        public int GetTexture(BlockFace face)
        {
            return faceTextures[(int)face];
        }
    }
}
