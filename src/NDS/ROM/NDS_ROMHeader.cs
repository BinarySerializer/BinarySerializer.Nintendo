using System.Text;

namespace BinarySerializer.Nintendo
{
    // https://problemkaputt.de/gbatek.htm#dscartridgeheader
    public class NDS_ROMHeader : BinarySerializable
    {
        public string GameTitle { get; set; }
        public string GameCode { get; set; }
        public string MakerCode { get; set; }
        public byte UnitCode { get; set; } // 0=NDS, 2=NDS+DSi, 3=DSi
        public byte EncryptionSeedSelect { get; set; }
        public byte Devicecapacity { get; set; }
        public byte[] Reserved1 { get; set; }
        public byte DSi_Flags { get; set; }
        public byte NDS_Region { get; set; }
        public byte DSi_PermitJump { get; set; }
        public byte ROMVersion { get; set; }
        public byte Autostart { get; set; } // Bit2: Skip "Press Button" after Health and Safety

        public uint ARM9_RomOffset { get; set; }
        public uint ARM9_EntryAddress { get; set; }
        public uint ARM9_RamAddress { get; set; }
        public uint ARM9_Size { get; set; }

        public uint ARM7_RomOffset { get; set; }
        public uint ARM7_EntryAddress { get; set; }
        public uint ARM7_RamAddress { get; set; }
        public uint ARM7_Size { get; set; }

        // File Name Table
        public uint FNT_Offset { get; set; }
        public uint FNT_Size { get; set; }

        // File Allocation Table
        public uint FAT_Offset { get; set; }
        public uint FAT_Size { get; set; }

        // File ARM9
        public uint ARM9_OverlayOffset { get; set; }
        public uint ARM9_OverlaySize { get; set; }

        // File ARM7
        public uint ARM7_OverlayOffset { get; set; }
        public uint ARM7_OverlaySize { get; set; }

        public override void SerializeImpl(SerializerObject s)
        {
            s.DoWithDefaults(new SerializerDefaults() { StringEncoding = Encoding.ASCII }, () =>
            {
                GameTitle = s.SerializeString(GameTitle, 12, name: nameof(GameTitle));
                GameCode = s.SerializeString(GameCode, 4, name: nameof(GameCode));
                MakerCode = s.SerializeString(MakerCode, 2, name: nameof(MakerCode));
                UnitCode = s.Serialize<byte>(UnitCode, name: nameof(UnitCode));
                EncryptionSeedSelect = s.Serialize<byte>(EncryptionSeedSelect, name: nameof(EncryptionSeedSelect));
                Devicecapacity = s.Serialize<byte>(Devicecapacity, name: nameof(Devicecapacity));
                Reserved1 = s.SerializeArray<byte>(Reserved1, 7, name: nameof(Reserved1));
                DSi_Flags = s.Serialize<byte>(DSi_Flags, name: nameof(DSi_Flags));

                if ((UnitCode & 1) == 0) // Is this correct?
                    NDS_Region = s.Serialize<byte>(NDS_Region, name: nameof(NDS_Region));
                else
                    DSi_PermitJump = s.Serialize<byte>(DSi_PermitJump, name: nameof(DSi_PermitJump));

                ROMVersion = s.Serialize<byte>(ROMVersion, name: nameof(ROMVersion));
                Autostart = s.Serialize<byte>(Autostart, name: nameof(Autostart));

                ARM9_RomOffset = s.Serialize<uint>(ARM9_RomOffset, name: nameof(ARM9_RomOffset));
                ARM9_EntryAddress = s.Serialize<uint>(ARM9_EntryAddress, name: nameof(ARM9_EntryAddress));
                ARM9_RamAddress = s.Serialize<uint>(ARM9_RamAddress, name: nameof(ARM9_RamAddress));
                ARM9_Size = s.Serialize<uint>(ARM9_Size, name: nameof(ARM9_Size));

                ARM7_RomOffset = s.Serialize<uint>(ARM7_RomOffset, name: nameof(ARM7_RomOffset));
                ARM7_EntryAddress = s.Serialize<uint>(ARM7_EntryAddress, name: nameof(ARM7_EntryAddress));
                ARM7_RamAddress = s.Serialize<uint>(ARM7_RamAddress, name: nameof(ARM7_RamAddress));
                ARM7_Size = s.Serialize<uint>(ARM7_Size, name: nameof(ARM7_Size));

                FNT_Offset = s.Serialize<uint>(FNT_Offset, name: nameof(FNT_Offset));
                FNT_Size = s.Serialize<uint>(FNT_Size, name: nameof(FNT_Size));

                FAT_Offset = s.Serialize<uint>(FAT_Offset, name: nameof(FAT_Offset));
                FAT_Size = s.Serialize<uint>(FAT_Size, name: nameof(FAT_Size));

                ARM9_OverlayOffset = s.Serialize<uint>(ARM9_OverlayOffset, name: nameof(ARM9_OverlayOffset));
                ARM9_OverlaySize = s.Serialize<uint>(ARM9_OverlaySize, name: nameof(ARM9_OverlaySize));

                ARM7_OverlayOffset = s.Serialize<uint>(ARM7_OverlayOffset, name: nameof(ARM7_OverlayOffset));
                ARM7_OverlaySize = s.Serialize<uint>(ARM7_OverlaySize, name: nameof(ARM7_OverlaySize));

                // TODO: Serialize remaining data
            });
        }
    }
}
