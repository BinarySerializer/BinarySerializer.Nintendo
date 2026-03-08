namespace BinarySerializer.Nintendo.Switch
{
    public class XTXTextureInfo : BinarySerializable
    {
        public long DataSize { get; set; }
        public uint Alignment { get; set; }
        public uint Width { get; set; }
        public uint Height { get; set; }
        public uint Depth { get; set; }
        public uint Target { get; set; }
        public XTXImageFormat Format { get; set; }
        public uint MipCount { get; set; }
        public uint SliceSize { get; set; }
        public uint[] MipOffsets { get; set; }
        public uint TextureLayout1 { get; set; }
        public uint TextureLayout2 { get; set; }
        public uint Boolean { get; set; }

        public override void SerializeImpl(SerializerObject s)
        {
            DataSize = s.Serialize<long>(DataSize, name: nameof(DataSize));
            Alignment = s.Serialize<uint>(Alignment, name: nameof(Alignment));
            Width = s.Serialize<uint>(Width, name: nameof(Width));
            Height = s.Serialize<uint>(Height, name: nameof(Height));
            Depth = s.Serialize<uint>(Depth, name: nameof(Depth));
            Target = s.Serialize<uint>(Target, name: nameof(Target));
            Format = s.Serialize<XTXImageFormat>(Format, name: nameof(Format));
            MipCount = s.Serialize<uint>(MipCount, name: nameof(MipCount));
            SliceSize = s.Serialize<uint>(SliceSize, name: nameof(SliceSize));
            MipOffsets = s.SerializeArray<uint>(MipOffsets, 17, name: nameof(MipOffsets));
            TextureLayout1 = s.Serialize<uint>(TextureLayout1, name: nameof(TextureLayout1));
            TextureLayout2 = s.Serialize<uint>(TextureLayout2, name: nameof(TextureLayout2));
            Boolean = s.Serialize<uint>(Boolean, name: nameof(Boolean));
        }
    }
}