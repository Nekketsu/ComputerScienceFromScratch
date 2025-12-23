using System;
using System.Diagnostics;

namespace NESEmulator;

public class Ppu(Rom rom)
{
    public const int SprRamSize = 256;
    public const int NametableSize = 2048;
    public const int PaletteSize = 32;
    public const int NesWidth = 256;
    public const int NesHeight = 240;

    private readonly uint[] nesPalette =
    [
        0x7C7C7C, 0x0000FC, 0x0000BC, 0x4428BC, 0x940084, 0xA80020,
        0xA81000, 0x881400, 0x503000, 0x007800, 0x006800, 0x005800,
        0x004058, 0x000000, 0x000000, 0x000000, 0xBCBCBC, 0x0078F8,
        0x0058F8, 0x6844FC, 0xD800CC, 0xE40058, 0xF83800, 0xE45C10,
        0xAC7C00, 0x00B800, 0x00A800, 0x00A844, 0x008888, 0x000000,
        0x000000, 0x000000, 0xF8F8F8, 0x3CBCFC, 0x6888FC, 0x9878F8,
        0xF878F8, 0xF85898, 0xF87858, 0xFCA044, 0xF8B800, 0xB8F818,
        0x58D854, 0x58F898, 0x00E8D8, 0x787878, 0x000000, 0x000000,
        0xFCFCFC, 0xA4E4FC, 0xB8B8F8, 0xD8B8F8, 0xF8B8F8, 0xF8A4C0,
        0xF0D0B0, 0xFCE0A8, 0xF8D878, 0xD8F878, 0xB8F8B8, 0xB8F8D8,
        0x00FCFC, 0xF8D8F8, 0x000000, 0x000000
    ];


    private readonly Rom rom = rom;
    // PPU memory
    public byte[] Spr { get; private set; } = new byte[SprRamSize]; // Sprite RAM
    private readonly byte[] nametables = new byte[NametableSize]; // Nametable RAM
    private readonly byte[] palette = new byte[PaletteSize]; // Palette RAM

    // Registers
    private ushort addr = 0; // Main PPU address register
    private bool addrWriteLatch = false;
    private byte status = 0;
    private int sprAddress = 0;
    // Variables controlled by PPU control registers
    private ushort nametableAddress = 0;
    private ushort addressIncrement = 1;
    private ushort sprPatternTableAddress = 0;
    private ushort backgroundPatternTableAddress = 0;
    public bool GenerateNmi { get; private set; } = false;
    private bool showBackground = false;
    private bool showSprites = false;
    private bool left8SpriteShow = false;
    private bool left8BackgroundShow = false;

    // Internal helper variables
    private byte buffer2007 = 0;
    public int Scanline { get; private set; } = 0;
    public int Cycle { get; private set; } = 0;
    // Pixels for screen
    public uint[,] DisplayBuffer { get; } = new uint[NesWidth, NesHeight];

    public void Step()
    {
        // Our simplified PPU draws just once per frame
        if ((Scanline == 240) && (Cycle == 256))
        {
            if (showBackground)
            {
                DrawBackground();
            }
            if (showSprites)
            {
                DrawSprites(false);
            }
        }
        if ((Scanline == 241) && (Cycle == 1))
        {
            status |= 0b10000000; // Set vblank
        }
        if ((Scanline == 261) && (Cycle == 1))
        {
            // Vblank off, clear sprite zero, clear sprite overflow
            status |= 0b00011111;
        }

        Cycle++;
        if (Cycle > 340)
        {
            Cycle = 0;
            Scanline++;
            if (Scanline > 261)
            {
                Scanline = 0;
            }
        }
    }

    private void DrawBackground()
    {
        var attributeTableAddress = nametableAddress + 960;

        for (var y = 0; y < 30; y++)
        {
            for (var x = 0; x < 32; x++)
            {
                var tileAddress = (ushort)(nametableAddress + y * 32 + x);
                var nametableEntry = ReadMemory(tileAddress);

                var attrX = x / 4;
                var attrY = y / 4;
                var attributeAddress = (ushort)(attributeTableAddress + attrY * 8 + attrX);
                var attributeEntry = ReadMemory(attributeAddress);
                // https://forums.nesdev.com/viewtopic.php?f=10&t=13315
                var block = (y & 0x2) | ((x & 0x2) >> 1);
                var attributeBits = 0;
                switch (block)
                {
                    case 0:
                        attributeBits = (attributeEntry & 0b00000011) << 2;
                        break;
                    case 1:
                        attributeBits = (attributeEntry & 0b00001100);
                        break;
                    case 2:
                        attributeBits = (attributeEntry & 0b00110000) >> 2;
                        break;
                    case 3:
                        attributeBits = (attributeEntry & 0b11000000) >> 4;
                        break;
                    default:
                        Debug.WriteLine($"Invalid block");
                        break;
                }

                for (var fineY = 0; fineY < 8; fineY++)
                {
                    var lowOrder = ReadMemory((ushort)(backgroundPatternTableAddress + nametableEntry * 16 + fineY));
                    var highOrder = ReadMemory((ushort)(backgroundPatternTableAddress + nametableEntry * 16 + 8 + fineY));
                    for (var fineX = 0; fineX < 8; fineX++)
                    {
                        var pixel = ((lowOrder >> (7 - fineX)) & 1) |
                            (((highOrder >> (7 - fineX)) & 1) << 1) |
                            attributeBits;

                        var xScreenLoc = x * 8 + fineX;
                        var yScreenLoc = y * 8 + fineY;
                        var transparent = (pixel & 3) == 0;
                        // If the background is transparent, use the first color in the palette
                        var color = transparent
                            ? palette[0]
                            : palette[pixel];

                        DisplayBuffer[xScreenLoc, yScreenLoc] = nesPalette[color];
                    }
                }
            }
        }
    }

    private void DrawSprites(bool backgroundTransparent)
    {
        for (var i = SprRamSize - 4; i >= 0; i -= 4)
        {
            var yPosition = Spr[i];
            if (yPosition == 0xFF) // 0xFF is a marker for no spirte data
            {
                continue;
            }
            var backgroundSprite = ((Spr[i + 2] >> 5) & 1) == 1;
            var xPosition = Spr[i + 3];

            for (int x = xPosition; x < xPosition + 8; x++)
            {
                if (x >= NesWidth)
                {
                    break;
                }
                for (int y = yPosition; y < yPosition + 8; y++)
                {
                    if (y >= NesHeight)
                    {
                        break;
                    }

                    var flipY = ((Spr[i + 2] >> 7) & 1) == 1;
                    var spriteLine = y - yPosition;
                    if (flipY)
                    {
                        spriteLine = 7 - spriteLine;
                    }

                    var index = Spr[i + 1];
                    var bit0sAddress = (ushort)(sprPatternTableAddress + (index * 16) + spriteLine);
                    var bit1sAddress = (ushort)(sprPatternTableAddress + (index * 16) + spriteLine + 8);
                    var bit0s = ReadMemory(bit0sAddress);
                    var bit1s = ReadMemory(bit1sAddress);
                    var bit3and2 = (Spr[i + 2] & 3) << 2;

                    var flipX = ((Spr[i + 2] >> 6) & 1) == 1;
                    var xLoc = x - xPosition; // Position within sprite
                    if (!flipX)
                    {
                        xLoc = 7 - xLoc;
                    }

                    var bit1and0 = (((bit1s >> xLoc) & 1) << 1) |
                        (((bit0s >> xLoc) & 1) << 0);
                    if (bit1and0 == 0) // Transparent pixel... skip
                    {
                        continue;
                    }

                    // This is not transparent. Is it a sprite-zero hit therefore?
                    // Check that left 8 pixel clipping is not off.
                    if ((i == 0) && (!backgroundTransparent) &&
                        (!(x < 8 && (!left8SpriteShow || !left8BackgroundShow)) && showBackground && showSprites))
                    {
                        status |= 0b01000000;
                    }
                    // Need to do this after sprite-zero checking so we still count background sprites for sprite-zero checks
                    if (backgroundSprite && !backgroundTransparent)
                    {
                        continue; // Background sprite shouldn't draw over opaque pixels
                    }

                    var color = bit3and2 | bit1and0;
                    color = ReadMemory((ushort)(0x3F10 + color)); // From palette

                    DisplayBuffer[x, y] = nesPalette[color];
                }
            }
        }
    }

    public byte ReadRegister(int address)
    {
        switch (address)
        {
            case 0x2002:
                {
                    addrWriteLatch = false;
                    var current = status;
                    status &= 0b01111111; // Clear vblank on read to 0x2002
                    return current;
                }

            case 0x2004:
                return Spr[sprAddress];
            case 0x2007:
                {
                    byte value;
                    if (addr % 0x4000 < 0x3F00)
                    {
                        value = buffer2007;
                        buffer2007 = ReadMemory(addr);
                    }
                    else
                    {
                        value = ReadMemory(addr);
                        buffer2007 = ReadMemory((ushort)(addr - 0x1000));
                    }
                    // Every read to 0x2007 there is an increment
                    addr += addressIncrement;
                    return value;
                }

            default:
                throw new IndexOutOfRangeException($"Error: Unrecognized PPU read {address:X}");
        }
    }

    public void WriteRegister(int address, byte value)
    {
        switch (address)
        {
            case 0x2000: // Control1
                nametableAddress = (ushort)(0x2000 + (value & 0b00000011) * 0x400);
                addressIncrement = (ushort)((value & 0b00000100) != 0 ? 32 : 1);
                sprPatternTableAddress = (ushort)(((value & 0b00001000) >> 3) * 0x1000);
                backgroundPatternTableAddress = (ushort)(((value & 0b00010000) >> 4) * 0x1000);
                GenerateNmi = (value & 0b10000000) != 0;
                break;
            case 0x2001:
                showBackground = (value & 0b00001000) != 0;
                showSprites = (value & 0b00010000) != 0;
                left8BackgroundShow = (value & 0b00000010) != 0;
                left8SpriteShow = (value & 0b00000100) != 0;
                break;
            case 0x2003:
                sprAddress = value;
                break;
            case 0x2004:
                Spr[sprAddress] = value;
                sprAddress++;
                break;
            case 0x2005: // Scroll
                break;
            case 0x2006:
                // Based on https://www.nesdev.org/wiki/PPU_scrolling
                if (!addrWriteLatch) // First write
                {
                    addr = (ushort)(addr & 0x00FF | ((value & 0xFF) << 8));
                }
                else // Second write
                {
                    addr = (ushort)((addr & 0xFF00) | (value & 0xFF));
                }
                addrWriteLatch = !addrWriteLatch;
                break;
            case 0x2007:
                WriteMemory(addr, value);
                addr += addressIncrement;
                break;
            default:
                throw new IndexOutOfRangeException($"Error: Unrecognized PPU write {address:X}");
        }
    }

    private byte ReadMemory(ushort address)
    {
        address %= 0x4000; // Mirror >0x4000
        switch (address)
        {
            case < 0x2000: // Pattern tables
                return rom.ReadCartridge(address);
            case < 0x3F00:
                address = (ushort)((address - 0x2000) % 0x1000); // 3000-3EFF is a mirror
                if (rom.VerticalMirroring)
                {
                    address %= 0x0800;
                }
                else // Horizontal mirroring
                {
                    if (address is >= 0x400 and < 0xC00)
                    {
                        address -= 0x400;
                    }
                    else if (address >= 0xC00)
                    {
                        address -= 0x800;
                    }
                }
                return nametables[address];
            case < 0x4000: // Palette memory
                address = (ushort)((address - 0x3F00) % 0x20);
                if (address > 0x0F && address % 0x04 == 0)
                {
                    address -= 0x10;
                }
                return palette[address];
            default:
                throw new IndexOutOfRangeException($"Error: Unrecognized PPU read at {address:X}");
        }
    }

    private void WriteMemory(ushort address, byte value)
    {
        address %= 0x4000; // Mirror >0x4000
        switch (address)
        {
            case < 0x2000: // Pattern tables
                rom.WriteCartridge(address, value);
                break;
            case < 0x3F00: // Nametables
                address = (ushort)((address - 0x2000) % 0x1000); // 3000-3EFF is a mirror
                if (rom.VerticalMirroring)
                {
                    address %= 0x0800;
                }
                else // Horizontal mirroring
                {
                    if (address is >= 0x400 and < 0xC00)
                    {
                        address -= 0x400;
                    }
                    else if (address >= 0xC00)
                    {
                        address -= 0x800;
                    }
                }
                nametables[address] = value;
                break;
            case < 0x4000: // Palette memory
                address = (ushort)((address - 0x3F00) % 0x20);
                if (address > 0x0F && address % 0x04 == 0)
                {
                    address -= 0x10;
                }
                palette[address] = value;
                break;
            default:
                throw new IndexOutOfRangeException($"Error: Unrecognized PPU write at {address:X}");
        }
    }
}