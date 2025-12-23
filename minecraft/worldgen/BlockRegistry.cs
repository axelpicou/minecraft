using System.Collections.Generic;

namespace minecraft.worldgen
{
    public static class BlockRegistry
    {
        private static readonly Dictionary<BlockType, BlockDefinition> defs = new();

        public static void Register(BlockDefinition def)
        {
            defs[def.Type] = def;
        }

        public static BlockDefinition Get(BlockType type)
        {
            return defs[type];
        }

        public static void Init()
        {
            Register(new BlockDefinition(BlockType.Dirt, 0));
            Register(new BlockDefinition(BlockType.Grass, 1));
            Register(new BlockDefinition(BlockType.Stone, 2));
            Register(new BlockDefinition(BlockType.Sand, 3));
        }
    }
}
