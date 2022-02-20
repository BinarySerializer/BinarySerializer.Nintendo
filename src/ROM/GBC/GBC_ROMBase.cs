namespace BinarySerializer.GBA
{
    /// <summary>
    /// Base data for a GBC ROM. Inherit from this if serializing data from a GBC ROM.
    /// </summary>
    public class GBC_ROMBase : BinarySerializable
    {
        /// <summary>
        /// The ROM header
        /// </summary>
        public GBC_ROMHeader Header { get; set; }

        public override void SerializeImpl(SerializerObject s)
        {
            Header = s.SerializeObject<GBC_ROMHeader>(Header, name: nameof(Header));
        }
    }
}