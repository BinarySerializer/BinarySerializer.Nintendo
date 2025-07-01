namespace BinarySerializer.Nintendo.GCN
{
    // GameCube ISO
    public class GCM : BinarySerializable
    {
        public Pointer FileSystemOffset { get; set; }
        public uint FileSystemSize { get; set; }

        public uint FileEntriesCount { get; set; }
        public GCMFileEntry[] FileEntries { get; set; }

        public override void SerializeImpl(SerializerObject s)
        {
            s.Goto(s.CurrentPointer + 0x424);
            FileSystemOffset = s.SerializePointer(FileSystemOffset, name: nameof(FileSystemOffset));
            FileSystemSize = s.Serialize<uint>(FileSystemSize, name: nameof(FileSystemSize));

            s.Goto(FileSystemOffset + 0x8);
            FileEntriesCount = s.Serialize<uint>(FileEntriesCount, name: nameof(FileEntriesCount));
            s.Goto(FileSystemOffset);
            Pointer fileNameTableOffset = FileSystemOffset + FileEntriesCount * 12;
            FileEntries = s.SerializeObjectArray<GCMFileEntry>(FileEntries, FileEntriesCount, (x, i) =>
            {
                x.Pre_FileIndex = i;
                x.Pre_FileNameTableOffset = fileNameTableOffset;
            }, name: nameof(FileEntries));
        }
    }
}