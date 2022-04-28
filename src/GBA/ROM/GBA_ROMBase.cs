namespace BinarySerializer.Nintendo
{
    /// <summary>
    /// Base data for a GBA ROM. Inherit from this if serializing data from a GBA ROM.
    /// </summary>
    public class GBA_ROMBase : BinarySerializable
    {
        /// <summary>
        /// The ROM header
        /// </summary>
        public GBA_ROMHeader Header { get; set; }

        public override void SerializeImpl(SerializerObject s)
        {
            Header = s.SerializeObject<GBA_ROMHeader>(Header, name: nameof(Header));
        }
    }
}