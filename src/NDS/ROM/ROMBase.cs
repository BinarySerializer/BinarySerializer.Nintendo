namespace BinarySerializer.Nintendo.NDS
{
    /// <summary>
    /// Base data for a NDS ROM. Inherit from this if serializing data from a NDS ROM.
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