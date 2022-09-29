namespace BinarySerializer.Nintendo.NDS 
{
    public class DS3D_Command_Vtx_XZ : DS3D_CommandData, ISerializerShortLog
    {
        public short X { get; set; }
        public short Z { get; set; }

        public override void SerializeImpl(SerializerObject s) 
        {
            X = s.Serialize<short>(X, name: nameof(X));
            Z = s.Serialize<short>(Z, name: nameof(Z));
        }

        public string ShortLog => ToString();
        public override string ToString() => $"{GetType()}({X}, {Z})";
    }
}