﻿namespace BinarySerializer.Nintendo.NDS 
{
    public class DS3D_CommandBlock : BinarySerializable 
    {
        public DS3D_Command[] Commands { get; set; }

        public override void SerializeImpl(SerializerObject s) 
        {
            Commands = s.SerializeObjectArray<DS3D_Command>(Commands, 4, name: nameof(Commands));
            
            foreach (DS3D_Command cmd in Commands)
                cmd.SerializeData(s);
        }
    }
}