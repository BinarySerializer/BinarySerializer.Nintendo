namespace BinarySerializer.Nintendo.NDS 
{
    public class DS3D_Command_Begin_Vtxs : DS3D_CommandData, ISerializerShortLog
    {
        public DS3D_PrimitiveType Type { get; set; }

        public override void SerializeImpl(SerializerObject s) 
        {
            s.DoBits<uint>(b => 
            {
                Type = b.SerializeBits<DS3D_PrimitiveType>(Type, 2, name: nameof(Type));
            });
        }

        public string ShortLog => ToString();
        public override string ToString() => $"{GetType()}({Type})";
    }
}