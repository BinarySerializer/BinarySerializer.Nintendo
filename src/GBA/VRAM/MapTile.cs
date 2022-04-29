namespace BinarySerializer.Nintendo.GBA
{
    public class MapTile : BinarySerializable
    {
        public bool Pre_IsAffine { get; set; }

        public int TileIndex { get; set; }
        public bool FlipX { get; set; }
        public bool FlipY { get; set; }
        public int PaletteIndex { get; set; }

        public override void SerializeImpl(SerializerObject s)
        {
            if (Pre_IsAffine)
                TileIndex = s.Serialize<byte>((byte)TileIndex, name: nameof(TileIndex));
            else
                s.DoBits<ushort>(b =>
                {
                    TileIndex = b.SerializeBits<int>(TileIndex, 10, name: nameof(TileIndex));
                    FlipX = b.SerializeBits<bool>(FlipX, 1, name: nameof(FlipX));
                    FlipY = b.SerializeBits<bool>(FlipY, 1, name: nameof(FlipY));
                    PaletteIndex = b.SerializeBits<int>(PaletteIndex, 4, name: nameof(PaletteIndex));
                });
        }

        public override bool UseShortLog => true;
        public override string ToString() => $"Tile(Index: {TileIndex}, FlipX: {FlipX}, FlipY: {FlipY}, Pal: {PaletteIndex})";
    }
}