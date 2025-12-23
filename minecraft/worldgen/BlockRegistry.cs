using System.Collections.Generic;

namespace minecraft.worldgen
{
    public static class BlockRegistry
    {
        private static Dictionary<BlockType, BlockDefinition> blocks;

        public static void Init()
        {
            blocks = new Dictionary<BlockType, BlockDefinition>
            {
                // index = position dans l'atlas
                { BlockType.Dirt,  new BlockDefinition(2) },
                { BlockType.Stone, new BlockDefinition(1) },
                { BlockType.Sand,  new BlockDefinition(19) },

                // Grass : top ≠ sides ≠ bottom
                {
                    BlockType.Grass,
                    new BlockDefinition(
                        front: 3,
                        back: 3,
                        left: 3,
                        right: 3,
                        top: 0,
                        bottom: 3
                    )
                }
            };
        }

        public static BlockDefinition Get(BlockType type)
        {
            return blocks[type];
        }
    }
}
