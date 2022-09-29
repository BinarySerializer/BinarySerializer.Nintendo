namespace BinarySerializer.Nintendo.NDS 
{
    public class DS3D_Command_Color : DS3D_CommandData, ISerializerShortLog
    {
        public RGB555Color Color { get; set; }

        public override void SerializeImpl(SerializerObject s) 
        {
            Color = s.SerializeObject<RGB555Color>(Color, name: nameof(Color));
            s.SerializePadding(2, logIfNotNull: true);
        }

        public string ShortLog => ToString();
        public override string ToString() => $"{GetType()}({Color})";
    }
}