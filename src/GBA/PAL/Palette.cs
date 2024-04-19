namespace BinarySerializer.Nintendo.GBA
{
    public class Palette : BinarySerializable
    {
        public bool Pre_Is8Bit { get; set; } // True for 256 colors, otherwise 16
        public int Pre_CustomLength { get; set; } = -1;

        public SerializableColor[] Colors { get; set; }

        public override void SerializeImpl(SerializerObject s)
        {
            int length = Pre_CustomLength;

            if (length == -1)
                length = Pre_Is8Bit ? 256 : 16;

            Colors = s.SerializeIntoArray<SerializableColor>(Colors, length, BitwiseColor.RGB555, name: nameof(Colors));
        }
    }
}