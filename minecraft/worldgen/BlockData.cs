using OpenTK.Mathematics;

namespace minecraft.worldgen
{
    public struct BlockData
    {
        private byte data;
        public Vector3 BiomeColor;

        // 4 bits bas : TYPE
        public BlockType Type
        {
            get => (BlockType)(data & 0b00001111);
            set => data = (byte)((data & 0b11110000) | ((byte)value & 0b00001111));
        }

        // 4 bits hauts : LUMIÈRE (0..15)
        public byte Light
        {
            get => (byte)((data & 0b11110000) >> 4);
            set => data = (byte)((data & 0b00001111) | ((value & 0b00001111) << 4));
        }

        public BlockData(BlockType type, byte light = 0, Vector3 biomeColor = default)
        {
            data = 0;
            BiomeColor = biomeColor == default ? Vector3.One : biomeColor;
            Type = type;
            Light = light;
        }

        public byte GetByte() => data;
    }
}
