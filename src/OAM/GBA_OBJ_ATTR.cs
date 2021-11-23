namespace BinarySerializer.GBA
{
    public class GBA_OBJ_ATTR : BinarySerializable
    {
        public byte YPosition { get; set; }
        public GBA_OBJ_ATTR_ObjectMode ObjectMode { get; set; }
        public GBA_OBJ_ATTR_GraphicsMode GraphicsMode { get; set; }
        public bool Mosaic { get; set; }
        public bool Is8Bit { get; set; }
        public byte SpriteShape { get; set; }

        public ushort XPosition { get; set; }
        public byte AffineIndex { get; set; }
        public bool HorizontalFlip { get; set; }
        public bool VerticalFlip { get; set; }
        public byte SpriteSize { get; set; }

        public ushort TileIndex { get; set; }
        public byte Priority { get; set; }
        public byte Palette { get; set; }

        public override void SerializeImpl(SerializerObject s)
        {
            s.DoBits<ushort>(b =>
            {
                YPosition = (byte)b.SerializeBits<int>(YPosition, 8, name: nameof(YPosition));
                ObjectMode = (GBA_OBJ_ATTR_ObjectMode)b.SerializeBits<int>((int)ObjectMode, 2, name: nameof(ObjectMode));
                GraphicsMode = (GBA_OBJ_ATTR_GraphicsMode)b.SerializeBits<int>((int)GraphicsMode, 2, name: nameof(GraphicsMode));
                Mosaic = b.SerializeBits<int>(Mosaic ? 1 : 0, 1, name: nameof(Mosaic)) == 1;
                Is8Bit = b.SerializeBits<int>(Is8Bit ? 1 : 0, 1, name: nameof(Is8Bit)) == 1;
                SpriteShape = (byte)b.SerializeBits<int>(SpriteShape, 2, name: nameof(SpriteShape));
            });
            s.DoBits<ushort>(b =>
            {
                XPosition = (ushort)b.SerializeBits<int>(XPosition, 9, name: nameof(XPosition));

                if (ObjectMode == GBA_OBJ_ATTR_ObjectMode.AFF || ObjectMode == GBA_OBJ_ATTR_ObjectMode.AFF_DBL)
                {
                    AffineIndex = (byte)b.SerializeBits<int>(AffineIndex, 5, name: nameof(AffineIndex));
                }
                else
                {
                    b.SerializeBits<int>(0, 3, name: "Padding");
                    HorizontalFlip = b.SerializeBits<int>(HorizontalFlip ? 1 : 0, 1, name: nameof(HorizontalFlip)) == 1;
                    VerticalFlip = b.SerializeBits<int>(VerticalFlip ? 1 : 0, 1, name: nameof(VerticalFlip)) == 1;
                }

                SpriteSize = (byte)b.SerializeBits<int>(SpriteSize, 2, name: nameof(SpriteSize));
            });
            s.DoBits<ushort>(b =>
            {
                TileIndex = (byte)b.SerializeBits<int>(TileIndex, 10, name: nameof(TileIndex));
                Priority = (byte)b.SerializeBits<int>(Priority, 2, name: nameof(Priority));
                Palette = (byte)b.SerializeBits<int>(Palette, 4, name: nameof(Palette));
            });
        }
    }
}