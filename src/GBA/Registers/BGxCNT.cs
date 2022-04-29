namespace BinarySerializer.Nintendo.GBA
{
    public class BGxCNT : BinarySerializable
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
                Priority = b.SerializeBits<byte>(Priority, 2, name: nameof(Priority));
                CharacterBaseBlock = b.SerializeBits<byte>(CharacterBaseBlock, 2, name: nameof(CharacterBaseBlock));
                Bits_04 = b.SerializeBits<byte>(Bits_04, 2, name: nameof(Bits_04));
                Mosaic = b.SerializeBits<bool>(Mosaic, 1, name: nameof(Mosaic));
                Is8Bit = b.SerializeBits<bool>(Is8Bit, 1, name: nameof(Is8Bit));
                ScreenBaseBlock = b.SerializeBits<byte>(ScreenBaseBlock, 5, name: nameof(ScreenBaseBlock));
                AffineWrapping = b.SerializeBits<bool>(AffineWrapping, 1, name: nameof(AffineWrapping));
                BackgroundSize = b.SerializeBits<byte>(BackgroundSize, 2, name: nameof(BackgroundSize));
            });
        }
    }
}