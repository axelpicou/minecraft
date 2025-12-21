namespace minecraft.worldgen
{
    public struct BlockData
    {
        private byte data;

        public byte Type
        {
            get => (byte)(data & 0b00001111);
            set => data = (byte)(data & 0b11110000 | value & 0b00001111);
        }

        public byte Meta
        {
            get => (byte)((data & 0b11110000) >> 4);
            set => data = (byte)(data & 0b00001111 | (value & 0b00001111) << 4);
        }

        public BlockData(byte type, byte meta = 0)
        {
            data = 0;
            Type = type;
            Meta = meta;
        }

        public byte GetByte() => data;
    }
}
