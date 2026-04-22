namespace BinarySerializer.Nintendo.GBA
{
    public readonly struct MapTile : ISerializerShortLog
    {
        public MapTile(int tileIndex)
        {
            TileIndex = tileIndex;
            PaletteIndex = 0;
            FlipX = false;
            FlipY = false;
        }

        public MapTile(int tileIndex, byte paletteIndex)
        {
            TileIndex = tileIndex;
            PaletteIndex = paletteIndex;
            FlipX = false;
            FlipY = false;
        }

        public MapTile(int tileIndex, byte paletteIndex, bool flipX)
        {
            TileIndex = tileIndex;
            PaletteIndex = paletteIndex;
            FlipX = flipX;
            FlipY = false;
        }

        public MapTile(int tileIndex, byte paletteIndex, bool flipX, bool flipY)
        {
            TileIndex = tileIndex;
            PaletteIndex = paletteIndex;
            FlipX = flipX;
            FlipY = flipY;
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

                x = new MapTile(tileIndex, paletteIndex, flipX, flipY);
            });

            return x;
        };

        public static SerializeInto<MapTile> SerializeInto_Affine = (s, x) =>
        {
            int tileIndex = s.Serialize<byte>((byte)x.TileIndex, name: nameof(TileIndex));

            return new MapTile(tileIndex, 0, false, false);
        };

        public string ShortLog => ToString();
        public override string ToString() => $"Tile(Index: {TileIndex}, FlipX: {FlipX}, FlipY: {FlipY}, Pal: {PaletteIndex})";
    }
}