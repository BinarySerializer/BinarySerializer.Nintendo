﻿namespace BinarySerializer.Nintendo.NDS 
{
    public class DS3D_Command_Vtx_YZ : DS3D_CommandData, ISerializerShortLog
    {
        public short Y { get; set; }
        public short Z { get; set; }

        public override void SerializeImpl(SerializerObject s) 
        {
            Y = s.Serialize<short>(Y, name: nameof(Y));
            Z = s.Serialize<short>(Z, name: nameof(Z));
        }

        public string ShortLog => ToString();
        public override string ToString() => $"{GetType()}({Y}, {Z})";
    }
}