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
            s.SerializeBitValues<ushort>(bitFunc =>
            {
                Priority = (byte)bitFunc(Priority, 2, name: nameof(Priority));
                CharacterBaseBlock = (byte)bitFunc(CharacterBaseBlock, 2, name: nameof(CharacterBaseBlock));
                Bits_04 = (byte)bitFunc(Bits_04, 2, name: nameof(Bits_04));
                Mosaic = bitFunc(Mosaic ? 1 : 0, 1, name: nameof(Mosaic)) == 1;
                Is8Bit = bitFunc(Is8Bit ? 1 : 0, 1, name: nameof(Is8Bit)) == 1;
                ScreenBaseBlock = (byte)bitFunc(ScreenBaseBlock, 5, name: nameof(ScreenBaseBlock));
                AffineWrapping = bitFunc(AffineWrapping ? 1 : 0, 1, name: nameof(AffineWrapping)) == 1;
                BackgroundSize = (byte)bitFunc(BackgroundSize, 2, name: nameof(BackgroundSize));
            });
        }
    }
}