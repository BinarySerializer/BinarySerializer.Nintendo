namespace BinarySerializer.GBA
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
                s.SerializeBitValues<ushort>(bitFunc =>
                {
                    TileIndex = bitFunc(TileIndex, 10, name: nameof(TileIndex));
                    FlipX = bitFunc(FlipX ? 1 : 0, 1, name: nameof(FlipX)) == 1;
                    FlipY = bitFunc(FlipY ? 1 : 0, 1, name: nameof(FlipY)) == 1;
                    PaletteIndex = bitFunc(PaletteIndex, 4, name: nameof(PaletteIndex));
                });
        }

        public override bool UseShortLog => true;
        public override string ToString() => $"Tile(Index: {TileIndex}, FlipX: {FlipX}, FlipY: {FlipY}, Pal: {PaletteIndex})";
    }
}