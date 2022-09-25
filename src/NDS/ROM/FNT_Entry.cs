using System.Linq;

namespace BinarySerializer.Nintendo.NDS
{
    public class FNT_Entry : BinarySerializable
    {
        public Pointer Pre_Anchor { get; set; }

        public Pointer SubTableOffset { get; set; }
        public ushort FirstFileID { get; set; }
        public ushort ParentDirectoryID { get; set; }

        public FNT_SubTableEntry[] SubTable { get; set; }

        protected void SerializeSubTable(SerializerObject s)
        {
            s.DoAt(SubTableOffset, () =>
            {
                return SubTable = s.SerializeObjectArrayUntil<FNT_SubTableEntry>(
                    obj: SubTable, 
                    conditionCheckFunc: x => x.NameLength == 0,
                    getLastObjFunc: () => new FNT_SubTableEntry() { NameLength = 0 }, 
                    name: nameof(SubTable));
            });

            ushort id = FirstFileID;

            foreach (FNT_SubTableEntry entry in SubTable.Where(x => x.IsFile))
            {
                entry.ID = id;
                id++;
            }
        }

        public override void SerializeImpl(SerializerObject s)
        {
            SubTableOffset = s.SerializePointer(SubTableOffset, anchor: Pre_Anchor ?? Offset, name: nameof(SubTableOffset));
            FirstFileID = s.Serialize<ushort>(FirstFileID, name: nameof(FirstFileID));
            ParentDirectoryID = s.Serialize<ushort>(ParentDirectoryID, name: nameof(ParentDirectoryID));

            SerializeSubTable(s);
        }
    }
}