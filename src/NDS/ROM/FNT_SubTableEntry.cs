namespace BinarySerializer.Nintendo.NDS
{
    public class FNT_SubTableEntry : BinarySerializable
    {
        public byte NameLength { get; set; }
        public string Name { get; set; }
        public ushort ID { get; set; }

        public bool IsFile => NameLength < 0x80;
        public bool IsDirectory => NameLength >= 0x80;

        public override void SerializeImpl(SerializerObject s)
        {
            NameLength = s.Serialize<byte>(NameLength, name: nameof(NameLength));
            Name = s.SerializeString(Name, length: IsFile ? NameLength : NameLength - 0x80, name: nameof(Name));

            if (IsDirectory)
                ID = s.Serialize<ushort>(ID, name: nameof(ID));
        }
    }
}