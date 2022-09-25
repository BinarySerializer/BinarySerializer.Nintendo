namespace BinarySerializer.Nintendo.NDS
{
    public class FAT_Entry : BinarySerializable
    {
        public Pointer StartPointer { get; set; }
        public Pointer EndPointer { get; set; }

        public long Length => EndPointer - StartPointer;

        public override void SerializeImpl(SerializerObject s)
        {
            StartPointer = s.SerializePointer(StartPointer, name: nameof(StartPointer));
            EndPointer = s.SerializePointer(EndPointer, name: nameof(EndPointer));
            s.Log($"File size: 0x{Length:X8}");
        }
    }
}