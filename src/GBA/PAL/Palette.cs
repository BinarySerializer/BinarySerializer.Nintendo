namespace BinarySerializer.Nintendo.GBA
{
    public class Palette : BinarySerializable
    {
        public bool Pre_Is8Bit { get; set; } // True for 256 colors, otherwise 16

        public RGB555Color[] Colors { get; set; }

        public override void SerializeImpl(SerializerObject s)
        {
            Colors = s.SerializeObjectArray<RGB555Color>(Colors, Pre_Is8Bit ? 256 : 16, name: nameof(Colors));
        }
    }
}