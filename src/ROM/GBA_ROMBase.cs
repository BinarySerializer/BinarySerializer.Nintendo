namespace BinarySerializer.GBA
{
    /// <summary>
    /// Base data for a GBA ROM
    /// </summary>
    public class GBA_ROMBase : BinarySerializable
    {
        public GBA_ROMHeader Header { get; set; }

        public override void SerializeImpl(SerializerObject s)
        {
            Header = s.SerializeObject<GBA_ROMHeader>(Header, name: nameof(Header));
        }
    }
}