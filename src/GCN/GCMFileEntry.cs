using System;

namespace BinarySerializer.Nintendo.GCN
{
    public class GCMFileEntry : BinarySerializable
    {
        public int Pre_FileIndex { get; set; }
        public Pointer Pre_FileNameTableOffset { get; set; }

        public uint NameOffset { get; set; }
        public bool IsDirectory { get; set; }
    
        // Directory
        public uint ParentIndex { get; set; }
        public uint NextIndex { get; set; }

        // File
        public Pointer FileOffset { get; set; }
        public uint FileSize { get; set; }

        public string Name { get; set; }

        public byte[] ReadFile()
        {
            BinaryDeserializer s = Context.Deserializer;
            return s.DoAt(FileOffset, () => s.SerializeArray<byte>(null, FileSize, name: Name));
        }

        public override void SerializeImpl(SerializerObject s)
        {
            s.DoBits<uint>(b =>
            {
                NameOffset = b.SerializeBits<uint>(NameOffset, 24, name: nameof(NameOffset));
                IsDirectory = b.SerializeBits<bool>(IsDirectory, 8, name: nameof(IsDirectory));
            });

            if (IsDirectory)
            {
                ParentIndex = s.Serialize<uint>(ParentIndex, name: nameof(ParentIndex));
                NextIndex = s.Serialize<uint>(NextIndex, name: nameof(NextIndex));
            }
            else
            {
                FileOffset = s.SerializePointer(FileOffset, name: nameof(FileOffset));
                FileSize = s.Serialize<uint>(FileSize, name: nameof(FileSize));
            }

            if (Pre_FileIndex == 0)
                Name = String.Empty;
            else
                s.DoAt(Pre_FileNameTableOffset + NameOffset, () => Name = s.SerializeString(Name, name: nameof(Name)));
        }
    }
}