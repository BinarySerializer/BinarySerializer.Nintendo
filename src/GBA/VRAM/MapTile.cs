namespace BinarySerializer.Nintendo.GBA
{
    public readonly struct MapTile : ISerializerShortLog
    {
        public MapTile(int tileIndex, bool flipX, bool flipY, byte paletteIndex)
        {
            TileIndex = tileIndex;
            FlipX = flipX;
            FlipY = flipY;
            PaletteIndex = paletteIndex;
        }

        public int TileIndex { get; }
        public bool FlipX { get; }
        public bool FlipY { get; }
        public byte PaletteIndex { get; }

        public static SerializeInto<MapTile> SerializeInto_Regular = (s, x) =>
        {
            s.DoBits<ushort>(b =>
            {
                int tileIndex = b.SerializeBits<int>(x.TileIndex, 10, name: nameof(TileIndex));
                bool flipX = b.SerializeBits<bool>(x.FlipX, 1, name: nameof(FlipX));
                bool flipY = b.SerializeBits<bool>(x.FlipY, 1, name: nameof(FlipY));
                byte paletteIndex = b.SerializeBits<byte>(x.PaletteIndex, 4, name: nameof(PaletteIndex));

                x = new MapTile(tileIndex, flipX, flipY, paletteIndex);
            });

            return x;
        };

        public static SerializeInto<MapTile> SerializeInto_Affine = (s, x) =>
        {
            int tileIndex = s.Serialize<byte>((byte)x.TileIndex, name: nameof(TileIndex));

            return new MapTile(tileIndex, false, false, 0);
        };

        public string ShortLog => ToString();
        public override string ToString() => $"Tile(Index: {TileIndex}, FlipX: {FlipX}, FlipY: {FlipY}, Pal: {PaletteIndex})";
    }
}