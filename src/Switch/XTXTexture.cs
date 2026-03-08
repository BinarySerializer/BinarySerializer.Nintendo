namespace BinarySerializer.Nintendo.Switch
{
    public class XTXTexture : BinarySerializable
    {
        public uint HeaderSize { get; set; }
        public uint MajorVersion { get; set; }
        public uint MinorVersion { get; set; }
        public XTXBlock[] Blocks { get; set; }

        public override void SerializeImpl(SerializerObject s)
        {
            // Serialize header
            s.SerializeMagicString("DFvN", 4);
            HeaderSize = s.Serialize<uint>(HeaderSize, name: nameof(HeaderSize));
            MajorVersion = s.Serialize<uint>(MajorVersion, name: nameof(MajorVersion));
            MinorVersion = s.Serialize<uint>(MinorVersion, name: nameof(MinorVersion));
            s.Goto(Offset + HeaderSize);

            // Serialize blocks
            Blocks = s.SerializeObjectArrayUntil<XTXBlock>(Blocks, x => s.CurrentFileOffset >= s.CurrentLength, name: nameof(Blocks));
        }
    }
}