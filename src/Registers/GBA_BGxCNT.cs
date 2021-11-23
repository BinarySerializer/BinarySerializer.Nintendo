namespace BinarySerializer.GBA
{
    public class GBA_BGxCNT : BinarySerializable
    {
        public byte Priority { get; set; }
        public byte CharacterBaseBlock { get; set; }
        public byte Bits_04 { get; set; }
        public bool Mosaic { get; set; }
        public bool Is8Bit { get; set; }
        public byte ScreenBaseBlock { get; set; }
        public bool AffineWrapping { get; set; }
        public byte BackgroundSize { get; set; }

        public override void SerializeImpl(SerializerObject s)
        {
            s.DoBits<ushort>(b =>
            {
                Priority = (byte)b.SerializeBits<int>(Priority, 2, name: nameof(Priority));
                CharacterBaseBlock = (byte)b.SerializeBits<int>(CharacterBaseBlock, 2, name: nameof(CharacterBaseBlock));
                Bits_04 = (byte)b.SerializeBits<int>(Bits_04, 2, name: nameof(Bits_04));
                Mosaic = b.SerializeBits<int>(Mosaic ? 1 : 0, 1, name: nameof(Mosaic)) == 1;
                Is8Bit = b.SerializeBits<int>(Is8Bit ? 1 : 0, 1, name: nameof(Is8Bit)) == 1;
                ScreenBaseBlock = (byte)b.SerializeBits<int>(ScreenBaseBlock, 5, name: nameof(ScreenBaseBlock));
                AffineWrapping = b.SerializeBits<int>(AffineWrapping ? 1 : 0, 1, name: nameof(AffineWrapping)) == 1;
                BackgroundSize = (byte)b.SerializeBits<int>(BackgroundSize, 2, name: nameof(BackgroundSize));
            });
        }
    }
}