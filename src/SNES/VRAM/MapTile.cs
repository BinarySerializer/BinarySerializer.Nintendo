namespace BinarySerializer.Nintendo.SNES
{
    public readonly struct MapTile : ISerializerShortLog
    {
        public MapTile(int tileIndex, byte paletteIndex, bool priority, bool flipX, bool flipY)
        {
            TileIndex = tileIndex;
            PaletteIndex = paletteIndex;
            Priority = priority;
            FlipX = flipX;
            FlipY = flipY;
        }

        public int TileIndex { get; }
        public byte PaletteIndex { get; }
        public bool Priority { get; }
        public bool FlipX { get; }
        public bool FlipY { get; }

        public static SerializeInto<MapTile> SerializeInto = (s, x) =>
        {
            s.DoBits<ushort>(b =>
            {
                int tileIndex = b.SerializeBits<int>(x.TileIndex, 10, name: nameof(TileIndex));
                byte paletteIndex = b.SerializeBits<byte>(x.PaletteIndex, 3, name: nameof(PaletteIndex));
                bool priority = b.SerializeBits<bool>(x.Priority, 1, name: nameof(Priority));
                bool flipX = b.SerializeBits<bool>(x.FlipX, 1, name: nameof(FlipX));
                bool flipY = b.SerializeBits<bool>(x.FlipY, 1, name: nameof(FlipY));

                x = new MapTile(tileIndex, paletteIndex, priority, flipX, flipY);
            });

            return x;
        };

        public string ShortLog => ToString();
        public override string ToString() => $"Tile(Index: {TileIndex}, Pal: {PaletteIndex}, Prio: {Priority}, FlipX: {FlipX}, FlipY: {FlipY})";
    }
}