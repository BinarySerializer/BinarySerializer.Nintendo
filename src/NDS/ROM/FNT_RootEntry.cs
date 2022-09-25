namespace BinarySerializer.Nintendo.NDS
{
    public class FNT_RootEntry : FNT_Entry
    {
        public ushort DirectoriesCount { get; set; }

        public FNT_Entry[] Directories { get; set; }

        public override void SerializeImpl(SerializerObject s)
        {
            SubTableOffset = s.SerializePointer(SubTableOffset, anchor: Offset, name: nameof(SubTableOffset));
            FirstFileID = s.Serialize<ushort>(FirstFileID, name: nameof(FirstFileID));
            DirectoriesCount = s.Serialize<ushort>(DirectoriesCount, name: nameof(DirectoriesCount));

            if (DirectoriesCount > 4096)
                throw new BinarySerializableException(this, $"Invalid directories count {DirectoriesCount}. Max is 4096.");

            SerializeSubTable(s);

            Directories = s.SerializeObjectArray<FNT_Entry>(Directories, DirectoriesCount - 1, 
                onPreSerialize: x => x.Pre_Anchor = Offset, name: nameof(Directories));
        }
    }
}