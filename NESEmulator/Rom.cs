using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.IO;

namespace NESEmulator;

public class Rom
{
    private class RomHeader
    {
        public uint Signature { get; init; }
        public byte PrgRomSize { get; init; }
        public byte ChrRomSize { get; init; }
        public byte Flags6 { get; init; }
        public byte Flags7 { get; init; }
        public byte Flags8 { get; init; }
        public byte Flags9 { get; init; }
        public byte Flags10 { get; init; }
        public byte[] Unused { get; init; }
    }

    private const int HeaderSize = 16;
    private const int TrainerSize = 512;
    private const int PrgRomBaseUnitSize = 16384;
    private const int ChrRomBaseUnitSize = 8192;
    private const int PrgRamSize = 8192;

    private readonly RomHeader header;
    private readonly int mapper;
    public Func<int, byte> ReadCartridge { get; }
    public Action<int, byte> WriteCartridge { get; }
    private readonly bool hasTrainer;
    private readonly byte[] trainerData;
    public bool VerticalMirroring { get; }
    private readonly byte[] prgRom;
    private readonly byte[] chrRom;
    public byte[] PrgRam { get; }

    public Rom(string romFile)
    {
        using var reader = new BinaryReader(File.OpenRead(romFile));
        var romHeader = new byte[HeaderSize].AsSpan();
        reader.Read(romHeader);

        header = new RomHeader
        {
            Signature = BinaryPrimitives.ReadUInt32BigEndian(romHeader[0..4]),
            PrgRomSize = romHeader[4],
            ChrRomSize = romHeader[5],
            Flags6 = romHeader[6],
            Flags7 = romHeader[7],
            Flags8 = romHeader[8],
            Flags9 = romHeader[9],
            Flags10 = romHeader[10],
            Unused = romHeader[11..].ToArray()
        };

        if (header.Signature != 0x4E45531A)
        {
            Debug.WriteLine("Invalid ROM Header Signature");
        }
        else
        {
            Debug.WriteLine("Valid ROM Header Signature");
        }
        // Untangle Mapper - one nibble in flags6 and one nibble in flags7
        mapper = (header.Flags7 & 0xF0) | ((header.Flags6 & 0xF0) >> 4);
        Debug.WriteLine($"Mapper {mapper}");
        if (mapper != 0)
        {
            Debug.WriteLine($"Invalid Mapper: Only Mapper 0 is Implemented");
        }

        ReadCartridge = ReadMapper0;
        WriteCartridge = WriteMapper0;
        // Check if there's a trainer (4th bit flags6) and read it
        hasTrainer = ((header.Flags6 & 4) == 1);
        if (hasTrainer)
        {
            trainerData = new byte[TrainerSize];
            reader.Read(trainerData, 0, TrainerSize);
        }
        // Check mirroring from flags6 bit 0
        VerticalMirroring = ((header.Flags6 & 1) == 1);
        Debug.WriteLine($"Has vertical mirroring {VerticalMirroring}");
        // Read PRG_ROM & CHR_ROM, in multiples of 16K and 8K, respectively
        prgRom = new byte[PrgRomBaseUnitSize * header.PrgRomSize];
        chrRom = new byte[ChrRomBaseUnitSize * header.ChrRomSize];
        PrgRam = new byte[PrgRamSize];
        reader.Read(prgRom, 0, PrgRomBaseUnitSize * header.PrgRomSize);
        reader.Read(chrRom, 0, ChrRomBaseUnitSize * header.ChrRomSize);
    }

    private byte ReadMapper0(int address)
    {
        switch (address)
        {
            case < 0x2000:
                return chrRom[address];
            case >= 0x6000 and < 0x8000:
                return PrgRam[address % PrgRamSize];
            case >= 0x8000:
                if (header.PrgRomSize > 1)
                {
                    return prgRom[address - 0x8000];
                }
                else
                {
                    return prgRom[(address - 0x8000) % PrgRomBaseUnitSize];
                }

            default:
                throw new IndexOutOfRangeException($"Tried to read at invalid address {address:X}");
        }
    }

    private void WriteMapper0(int address, byte value)
    {
        if (address >= 0x6000)
        {
            PrgRam[address % PrgRamSize] = value;
        }
    }
}
