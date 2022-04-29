namespace BinarySerializer.Nintendo.GBA
{
    /// <summary>
    /// Base data for a GBA ROM. Inherit from this if serializing data from a GBA ROM.
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