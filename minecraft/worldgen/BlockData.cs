namespace minecraft.worldgen
{
    public struct BlockData
    {
        private byte data;

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

        public BlockData(BlockType type, byte meta = 0)
        {
            data = 0;
            Type = type;
            Meta = meta;
        }

        public byte GetByte() => data;
    }
}
