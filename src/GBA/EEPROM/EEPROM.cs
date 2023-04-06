namespace BinarySerializer.Nintendo.GBA
{
    public class EEPROM<T> : BinarySerializable
        where T : BinarySerializable, new()
    {
        public EEPROMSize Pre_Size { get; set; } = EEPROMSize.Kbit_4;
        public T Obj { get; set; }

        public override void SerializeImpl(SerializerObject s)
        {
            s.DoEncoded(new EEPROMEncoder((long)Pre_Size), () =>
            {
                long fileOffset = s.CurrentFileOffset;
                Obj = s.SerializeObject<T>(Obj, name: nameof(Obj));

                long objSize = s.CurrentFileOffset - fileOffset;

                if (objSize == (long)Pre_Size)
                    return;

                if (objSize > (long)Pre_Size)
                    throw new BinarySerializableException(this, "EEPROM object is too big");

                s.SerializePadding((long)Pre_Size - objSize);
            });
        }

        public enum EEPROMSize
        {
            Kbit_4 = 0x200,
            Kbit_64 = 0x2000,
        }
    }
}