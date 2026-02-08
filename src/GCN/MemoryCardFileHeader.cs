using System.Text;

namespace BinarySerializer.Nintendo.GCN
{
    public class MemoryCardFileHeader : BinarySerializable
    {
        public string GameCode { get; set; }
        public string MakerCode { get; set; }
        public byte Reserved1 { get; set; }
        public byte BannerAndIconFlags { get; set; }
        public string FileName { get; set; }
        public uint LastModifiedTime { get; set; } // Seconds since 12am, January 1st, 2000
        public uint ImageDataOffset { get; set; }
        public ushort IconGfxFormats { get; set; } // 2 bits per icon
        public ushort IconAnimationSpeeds { get; set; } // 2 bits per icon
        public byte FilePermissions { get; set; }
        public byte CopyCounter { get; set; }
        public ushort FirstBlockNumber { get; set; }
        public ushort BlocksCount { get; set; }
        public ushort Reserved2 { get; set; }
        public uint FileCommentsOffset { get; set; }

        public override void SerializeImpl(SerializerObject s)
        {
            GameCode = s.SerializeString(GameCode, 4, Encoding.ASCII, name: nameof(GameCode));
            MakerCode = s.SerializeString(MakerCode, 2, Encoding.ASCII, name: nameof(MakerCode));
            Reserved1 = s.Serialize<byte>(Reserved1, name: nameof(Reserved1));
            BannerAndIconFlags = s.Serialize<byte>(BannerAndIconFlags, name: nameof(BannerAndIconFlags));
            FileName = s.SerializeString(FileName, 32, Encoding.ASCII, name: nameof(FileName));
            LastModifiedTime = s.Serialize<uint>(LastModifiedTime, name: nameof(LastModifiedTime));
            ImageDataOffset = s.Serialize<uint>(ImageDataOffset, name: nameof(ImageDataOffset));
            IconGfxFormats = s.Serialize<ushort>(IconGfxFormats, name: nameof(IconGfxFormats));
            IconAnimationSpeeds = s.Serialize<ushort>(IconAnimationSpeeds, name: nameof(IconAnimationSpeeds));
            FilePermissions = s.Serialize<byte>(FilePermissions, name: nameof(FilePermissions));
            CopyCounter = s.Serialize<byte>(CopyCounter, name: nameof(CopyCounter));
            FirstBlockNumber = s.Serialize<ushort>(FirstBlockNumber, name: nameof(FirstBlockNumber));
            BlocksCount = s.Serialize<ushort>(BlocksCount, name: nameof(BlocksCount));
            Reserved2 = s.Serialize<ushort>(Reserved2, name: nameof(Reserved2));
            FileCommentsOffset = s.Serialize<uint>(FileCommentsOffset, name: nameof(FileCommentsOffset));
        }
    }
}