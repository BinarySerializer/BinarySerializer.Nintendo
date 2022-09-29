using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BinarySerializer.Nintendo.NDS
{
    // https://problemkaputt.de/gbatek.htm#dscartridgeheader
    public class ROMHeader : BinarySerializable
    {
        // TODO: Calculate 16-bit checksums

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

        public Pointer ARM9_RomOffset { get; set; }
        public uint ARM9_EntryAddress { get; set; }
        public uint ARM9_RamAddress { get; set; }
        public uint ARM9_Size { get; set; }

        public Pointer ARM7_RomOffset { get; set; }
        public uint ARM7_EntryAddress { get; set; }
        public uint ARM7_RamAddress { get; set; }
        public uint ARM7_Size { get; set; }

        // File Name Table
        public Pointer FNT_Offset { get; set; }
        public uint FNT_Size { get; set; }

        // File Allocation Table
        public Pointer FAT_Offset { get; set; }
        public uint FAT_Size { get; set; }

        // File ARM9
        public Pointer ARM9_OverlayOffset { get; set; }
        public uint ARM9_OverlaySize { get; set; }

        // File ARM7
        public Pointer ARM7_OverlayOffset { get; set; }
        public uint ARM7_OverlaySize { get; set; }

        public uint Uint_60 { get; set; } // Port 40001A4h setting for normal commands (usually 00586000h)
        public uint Uint_64 { get; set; } // Port 40001A4h setting for KEY1 commands   (usually 001808F8h)
        public Pointer IconOffset { get; set; }
        public ushort SecureAreaChecksum { get; set; }
        public ushort SecureAreaDelay { get; set; } // In 131kHz units, 051Eh=10ms or 0D7Eh=26ms

        public uint ARM9_AutoLoadRamAddress { get; set; }
        public uint ARM7_AutoLoadRamAddress { get; set; }

        public ulong SecureDisable { get; set; }
        public uint ROMSize { get; set; }
        public uint ROMHeaderSize { get; set; }
        public byte[] Reserved2 { get; set; } // TODO: Not all of this data is reserved - there's some NAND data here

        public byte[] NintendoLogo { get; set; }
        public ushort NintendoLogoChecksum { get; set; }
        public ushort HeaderChecksum { get; set; }

        public Pointer Debug_Offset { get; set; }
        public uint Debug_Size { get; set; }
        public uint Debug_RamAddress { get; set; }

        public uint Reserved3 { get; set; }
        public byte[] Reserved4 { get; set; }
        public byte[] Reserved5 { get; set; }

        // Serialized from offsets
        public FAT_Entry[] FAT { get; set; } // File Allocation Table
        public FNT_RootEntry FNT { get; set; } // File Name Table

        public FAT_Entry FindFile(string filePath)
        {
            string[] parts = filePath.Split('/', '\\');

            FNT_Entry currentDir = FNT;

            foreach (string part in parts)
            {
                int fileID = currentDir.FirstFileID;
                
                FNT_SubTableEntry subEntry = currentDir.SubTable.FirstOrDefault(x =>
                {
                    if (x.Name == part)
                        return true;

                    if (x.IsFile)
                        fileID++;

                    return false;
                });

                if (subEntry == null)
                    return null;

                if (subEntry.IsFile)
                    return FAT[fileID];

                currentDir = FNT.Directories[(subEntry.ID & 0xFFF) - 1]; // -1 since the root isn't included
            }

            return null;
        }

        public void SerializeFile(SerializerObject s, string filePath, Action<SerializerObject, long> serializeFunc)
        {
            FAT_Entry file = FindFile(filePath);

            if (file == null)
                throw new Exception($"File '{filePath}' not found");

            long fileLength = file.Length;

            Offset.File.AddRegion(file.StartPointer.FileOffset, fileLength, filePath);

            s.DoAt(file.StartPointer, () =>
            {
                serializeFunc(s, fileLength);

                if (s.CurrentPointer != file.EndPointer)
                    s.SystemLogger?.LogWarning("The full file {0} was not serialized. Missing {1} bytes.", filePath, file.EndPointer - s.CurrentPointer);
            });
        }

        public Dictionary<string, FAT_Entry> CreateFileTable()
        {
            Dictionary<string, FAT_Entry> table = new();

            const char separator = '/';
            processDir(FNT, String.Empty);

            return table;

            void processDir(FNT_Entry fntEntry, string path)
            {
                int fileID = fntEntry.FirstFileID;

                foreach (FNT_SubTableEntry subEntry in fntEntry.SubTable)
                {
                    if (subEntry.IsFile)
                    {
                        table.Add(path + subEntry.Name, FAT[fileID]);
                        fileID++;
                    }
                    else
                    {
                        processDir(FNT.Directories[(subEntry.ID & 0xFFF) - 1], path + subEntry.Name + separator);
                    }
                }
            }
        }

        public override void SerializeImpl(SerializerObject s)
        {
            s.DoWithDefaults(new SerializerDefaults()
            {
                StringEncoding = Encoding.ASCII, 
                PointerAnchor = Offset, 
                PointerNullValue = 0
            }, () =>
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

                ARM9_RomOffset = s.SerializePointer(ARM9_RomOffset, name: nameof(ARM9_RomOffset));
                ARM9_EntryAddress = s.Serialize<uint>(ARM9_EntryAddress, name: nameof(ARM9_EntryAddress));
                ARM9_RamAddress = s.Serialize<uint>(ARM9_RamAddress, name: nameof(ARM9_RamAddress));
                ARM9_Size = s.Serialize<uint>(ARM9_Size, name: nameof(ARM9_Size));

                ARM7_RomOffset = s.SerializePointer(ARM7_RomOffset, name: nameof(ARM7_RomOffset));
                ARM7_EntryAddress = s.Serialize<uint>(ARM7_EntryAddress, name: nameof(ARM7_EntryAddress));
                ARM7_RamAddress = s.Serialize<uint>(ARM7_RamAddress, name: nameof(ARM7_RamAddress));
                ARM7_Size = s.Serialize<uint>(ARM7_Size, name: nameof(ARM7_Size));

                FNT_Offset = s.SerializePointer(FNT_Offset, name: nameof(FNT_Offset));
                FNT_Size = s.Serialize<uint>(FNT_Size, name: nameof(FNT_Size));

                FAT_Offset = s.SerializePointer(FAT_Offset, name: nameof(FAT_Offset));
                FAT_Size = s.Serialize<uint>(FAT_Size, name: nameof(FAT_Size));

                ARM9_OverlayOffset = s.SerializePointer(ARM9_OverlayOffset, name: nameof(ARM9_OverlayOffset));
                ARM9_OverlaySize = s.Serialize<uint>(ARM9_OverlaySize, name: nameof(ARM9_OverlaySize));

                ARM7_OverlayOffset = s.SerializePointer(ARM7_OverlayOffset, name: nameof(ARM7_OverlayOffset));
                ARM7_OverlaySize = s.Serialize<uint>(ARM7_OverlaySize, name: nameof(ARM7_OverlaySize));

                Uint_60 = s.Serialize<uint>(Uint_60, name: nameof(Uint_60));
                Uint_64 = s.Serialize<uint>(Uint_64, name: nameof(Uint_64));
                IconOffset = s.SerializePointer(IconOffset, name: nameof(IconOffset));
                SecureAreaChecksum = s.Serialize<ushort>(SecureAreaChecksum, name: nameof(SecureAreaChecksum));
                SecureAreaDelay = s.Serialize<ushort>(SecureAreaDelay, name: nameof(SecureAreaDelay));
                
                ARM9_AutoLoadRamAddress = s.Serialize<uint>(ARM9_AutoLoadRamAddress, name: nameof(ARM9_AutoLoadRamAddress));
                ARM7_AutoLoadRamAddress = s.Serialize<uint>(ARM7_AutoLoadRamAddress, name: nameof(ARM7_AutoLoadRamAddress));

                SecureDisable = s.Serialize<ulong>(SecureDisable, name: nameof(SecureDisable));
                ROMSize = s.Serialize<uint>(ROMSize, name: nameof(ROMSize));
                ROMHeaderSize = s.Serialize<uint>(ROMHeaderSize, name: nameof(ROMHeaderSize));
                Reserved2 = s.SerializeArray<byte>(Reserved2, 56, name: nameof(Reserved2));

                NintendoLogo = s.SerializeArray<byte>(NintendoLogo, 156, name: nameof(NintendoLogo));
                NintendoLogoChecksum = s.Serialize<ushort>(NintendoLogoChecksum, name: nameof(NintendoLogoChecksum));
                HeaderChecksum = s.Serialize<ushort>(HeaderChecksum, name: nameof(HeaderChecksum));

                Debug_Offset = s.SerializePointer(Debug_Offset, name: nameof(Debug_Offset));
                Debug_Size = s.Serialize<uint>(Debug_Size, name: nameof(Debug_Size));
                Debug_RamAddress = s.Serialize<uint>(Debug_RamAddress, name: nameof(Debug_RamAddress));

                Reserved3 = s.Serialize<uint>(Reserved3, name: nameof(Reserved3));
                Reserved4 = s.SerializeArray<byte>(Reserved4, 0x90, name: nameof(Reserved4));
                Reserved5 = s.SerializeArray<byte>(Reserved5, 0xE00, name: nameof(Reserved5));

                // Serialize data from offsets
                s.DoAt(FAT_Offset, () => FAT = s.SerializeObjectArray<FAT_Entry>(FAT, FAT_Size / 8, name: nameof(FAT)));
                s.DoAt(FNT_Offset, () => FNT = s.SerializeObject<FNT_RootEntry>(FNT, name: nameof(FNT)));
            });
        }
    }
}