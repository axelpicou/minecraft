namespace minecraft.worldgen
{
    public class BlockDefinition
    {
        public BlockType Type { get; }
        public int TextureIndex { get; }

        public BlockDefinition(BlockType type, int textureIndex)
        {
            Type = type;
            TextureIndex = textureIndex;
        }
    }
}
