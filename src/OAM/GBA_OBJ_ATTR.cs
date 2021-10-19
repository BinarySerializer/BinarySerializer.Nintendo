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
            s.SerializeBitValues<ushort>(bitFunc =>
            {
                YPosition = (byte)bitFunc(YPosition, 8, name: nameof(YPosition));
                ObjectMode = (GBA_OBJ_ATTR_ObjectMode)bitFunc((int)ObjectMode, 2, name: nameof(ObjectMode));
                GraphicsMode = (GBA_OBJ_ATTR_GraphicsMode)bitFunc((int)GraphicsMode, 2, name: nameof(GraphicsMode));
                Mosaic = bitFunc(Mosaic ? 1 : 0, 1, name: nameof(Mosaic)) == 1;
                Is8Bit = bitFunc(Is8Bit ? 1 : 0, 1, name: nameof(Is8Bit)) == 1;
                SpriteShape = (byte)bitFunc(SpriteShape, 2, name: nameof(SpriteShape));
            });
            s.SerializeBitValues<ushort>(bitFunc =>
            {
                XPosition = (ushort)bitFunc(XPosition, 9, name: nameof(XPosition));

                if (ObjectMode == GBA_OBJ_ATTR_ObjectMode.AFF || ObjectMode == GBA_OBJ_ATTR_ObjectMode.AFF_DBL)
                {
                    AffineIndex = (byte)bitFunc(AffineIndex, 5, name: nameof(AffineIndex));
                }
                else
                {
                    bitFunc(0, 3, name: "Padding");
                    HorizontalFlip = bitFunc(HorizontalFlip ? 1 : 0, 1, name: nameof(HorizontalFlip)) == 1;
                    VerticalFlip = bitFunc(VerticalFlip ? 1 : 0, 1, name: nameof(VerticalFlip)) == 1;
                }

                SpriteSize = (byte)bitFunc(SpriteSize, 2, name: nameof(SpriteSize));
            });
            s.SerializeBitValues<ushort>(bitFunc =>
            {
                TileIndex = (byte)bitFunc(TileIndex, 10, name: nameof(TileIndex));
                Priority = (byte)bitFunc(Priority, 2, name: nameof(Priority));
                Palette = (byte)bitFunc(Palette, 4, name: nameof(Palette));
            });
        }
    }
}