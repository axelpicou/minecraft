using OpenTK.Mathematics;

namespace minecraft.worldgen
{
    public struct BlockData
    {
        private byte data;
        public Vector3 BiomeColor; // Couleur du biome (RGB 0-1)

        // 4 bits bas
        public BlockType Type
        {
            get => (BlockType)(data & 0b00001111);
            set => data = (byte)((data & 0b11110000) | ((byte)value & 0b00001111));
        }

        // 4 bits hauts
        public byte Meta
        {
            get => (byte)((data & 0b11110000) >> 4);
            set => data = (byte)((data & 0b00001111) | ((value & 0b00001111) << 4));
        }

        public BlockData(BlockType type, byte meta = 0, Vector3 biomeColor = default)
        {
            data = 0;
            BiomeColor = biomeColor == default ? Vector3.One : biomeColor;
            Type = type;
            Meta = meta;
        }

        public byte GetByte() => data;
    }
}