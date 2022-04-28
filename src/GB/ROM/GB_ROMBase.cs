namespace BinarySerializer.Nintendo
{
    /// <summary>
    /// Base data for a GB/GBC ROM. Inherit from this if serializing data from a GB/GBC ROM.
    /// </summary>
    public class GB_ROMBase : BinarySerializable
    {
        /// <summary>
        /// The ROM header
        /// </summary>
        public GB_ROMHeader Header { get; set; }

        public override void SerializeImpl(SerializerObject s)
        {
            Header = s.SerializeObject<GB_ROMHeader>(Header, name: nameof(Header));
        }
    }
}