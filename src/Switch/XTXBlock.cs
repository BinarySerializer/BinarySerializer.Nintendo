namespace BinarySerializer.Nintendo.Switch
{
    public class XTXBlock : BinarySerializable
    {
        public uint BlockSize { get; set; }
        public long DataSize { get; set; }
        public long DataOffset { get; set; }
        public XTXBlockType BlockType { get; set; }
        public uint GlobalBlockIndex { get; set; }
        public uint IncBlockTypeIndex { get; set; }

        // Data
        public XTXTextureInfo TextureInfo { get; set; }
        public byte[] RawData { get; set; }

        public override void SerializeImpl(SerializerObject s)
        {
            // Serialize header
            s.SerializeMagicString("HBvN", 4);
            BlockSize = s.Serialize<uint>(BlockSize, name: nameof(BlockSize));
            DataSize = s.Serialize<long>(DataSize, name: nameof(DataSize));
            DataOffset = s.Serialize<long>(DataOffset, name: nameof(DataOffset));
            BlockType = s.Serialize<XTXBlockType>(BlockType, name: nameof(BlockType));
            GlobalBlockIndex = s.Serialize<uint>(GlobalBlockIndex, name: nameof(GlobalBlockIndex));
            IncBlockTypeIndex = s.Serialize<uint>(IncBlockTypeIndex, name: nameof(IncBlockTypeIndex));
            s.Goto(Offset + DataOffset);

            // Serialize data
            if (BlockType == XTXBlockType.Texture)
            {
                TextureInfo = s.SerializeObject<XTXTextureInfo>(TextureInfo, name: nameof(TextureInfo));
                s.Goto(Offset + DataOffset + DataSize);
            }
            else
            {
                RawData = s.SerializeArray<byte>(RawData, DataSize, name: nameof(RawData));
            }
        }
    }
}