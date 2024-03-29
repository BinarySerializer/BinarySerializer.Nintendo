﻿namespace BinarySerializer.Nintendo.NDS 
{
    public class DS3D_Command_Vtx_16 : DS3D_CommandData, ISerializerShortLog
    {
        public short X { get; set; }
        public short Y { get; set; }
        public short Z { get; set; }

        public override void SerializeImpl(SerializerObject s) 
        {
            X = s.Serialize<short>(X, name: nameof(X));
            Y = s.Serialize<short>(Y, name: nameof(Y));
            Z = s.Serialize<short>(Z, name: nameof(Z));
            s.SerializePadding(2, logIfNotNull: true);
        }

        public string ShortLog => ToString();
        public override string ToString() => $"{GetType()}({X}, {Y}, {Z})";
    }
}