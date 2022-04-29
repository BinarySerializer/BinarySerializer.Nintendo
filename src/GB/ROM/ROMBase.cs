namespace BinarySerializer.Nintendo.GB
{
    /// <summary>
    /// Base data for a GB/GBC ROM. Inherit from this if serializing data from a GB/GBC ROM.
    /// </summary>
    public class ROMBase : BinarySerializable
    {
        /// <summary>
        /// The ROM header
        /// </summary>
        public ROMHeader Header { get; set; }

        public override void SerializeImpl(SerializerObject s)
        {
            Header = s.SerializeObject<ROMHeader>(Header, name: nameof(Header));
        }
    }
}