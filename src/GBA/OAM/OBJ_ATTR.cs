namespace BinarySerializer.Nintendo.GBA
{
    public class OBJ_ATTR : BinarySerializable
    {
        public byte YPosition { get; set; }
        public OBJ_ATTR_ObjectMode ObjectMode { get; set; }
        public OBJ_ATTR_GraphicsMode GraphicsMode { get; set; }
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
                YPosition = b.SerializeBits<byte>(YPosition, 8, name: nameof(YPosition));
                ObjectMode = b.SerializeBits<OBJ_ATTR_ObjectMode>(ObjectMode, 2, name: nameof(ObjectMode));
                GraphicsMode = b.SerializeBits<OBJ_ATTR_GraphicsMode>(GraphicsMode, 2, name: nameof(GraphicsMode));
                Mosaic = b.SerializeBits<bool>(Mosaic, 1, name: nameof(Mosaic));
                Is8Bit = b.SerializeBits<bool>(Is8Bit, 1, name: nameof(Is8Bit));
                SpriteShape = b.SerializeBits<byte>(SpriteShape, 2, name: nameof(SpriteShape));
            });
            s.DoBits<ushort>(b =>
            {
                XPosition = b.SerializeBits<ushort>(XPosition, 9, name: nameof(XPosition));

                if (ObjectMode == OBJ_ATTR_ObjectMode.AFF || ObjectMode == OBJ_ATTR_ObjectMode.AFF_DBL)
                {
                    AffineIndex = b.SerializeBits<byte>(AffineIndex, 5, name: nameof(AffineIndex));
                }
                else
                {
                    b.SerializePadding(3);
                    HorizontalFlip = b.SerializeBits<bool>(HorizontalFlip, 1, name: nameof(HorizontalFlip));
                    VerticalFlip = b.SerializeBits<bool>(VerticalFlip, 1, name: nameof(VerticalFlip));
                }

                SpriteSize = b.SerializeBits<byte>(SpriteSize, 2, name: nameof(SpriteSize));
            });
            s.DoBits<ushort>(b =>
            {
                TileIndex = b.SerializeBits<ushort>(TileIndex, 10, name: nameof(TileIndex));
                Priority = b.SerializeBits<byte>(Priority, 2, name: nameof(Priority));
                Palette = b.SerializeBits<byte>(Palette, 4, name: nameof(Palette));
            });
        }
    }
}