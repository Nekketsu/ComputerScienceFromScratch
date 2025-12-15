using System;
using System.Collections.Generic;
using System.Diagnostics;
using Input = Microsoft.Xna.Framework.Input;

namespace Chip8;

public class Chip8Vm(byte[] programData)
{
    private byte[] v = new byte[16];
    private ushort i = 0;
    private ushort pc = 0x200;
    private byte[] ram = CreateRam(programData);
    private Stack<ushort> stack = [];

    public bool[,] DisplayBuffer { get; private set; } = new bool[ScreenWidth, ScreenHeight];
    public bool NeedsRedraw { get; private set; } = false;

    private byte delayTimer = 0;
    private byte soundTimer = 0;

    private bool[] previousKeys = new bool[16];
    private bool[] keys = new bool[16];

    public void DecrementTimers()
    {
        if (delayTimer > 0)
        {
            delayTimer--;
        }
        if (soundTimer > 0)
        {
            soundTimer--;
        }
    }

    public bool PlaySound => soundTimer > 0;

    private void DrawSprite(int x, int y, int height)
    {
        var flippedBack = false;
        for (var row = 0; row < height; row++)
        {
            var rowBits = ram[i + row];
            for (var col = 0; col < SpriteWidth; col++)
            {
                var px = x + col;
                var py = y + row;
                if (px >= ScreenWidth || py >= ScreenHeight)
                {
                    continue;
                }
                var newBit = ((rowBits >> (7 - col)) & 1) == 1;
                var oldBit = DisplayBuffer[px, py];
                if (newBit & oldBit)
                {
                    flippedBack = true;
                }
                var newPixel = newBit != oldBit;
                DisplayBuffer[px, py] = newPixel;
            }
        }

        v[0xF] = flippedBack ? (byte)1 : (byte)0;
    }

    public void Step()
    {
        var first2 = ram[pc];
        var last2 = ram[pc + 1];
        var first = (first2 & 0xF0) >> 4;
        var second = first2 & 0xF;
        var third = (last2 & 0xF0) >> 4;
        var fourth = last2 & 0xF;

        NeedsRedraw = false;
        var jumped = false;

        switch ((first, second, third, fourth))
        {
            case (0x0, 0x0, 0xE, 0x0): // Display clear
                Debug.WriteLine($"cls");
                Array.Clear(DisplayBuffer);
                NeedsRedraw = true;
                break;
            case (0x0, 0x0, 0xE, 0xE): // Return from subroutine
                pc = stack.Pop();
                jumped = true;
                break;
            case (0x0, var n1, var n2, var n3): // Call program
                pc = ConcatNibbles(n1, n2, n3); // Go to start
                // Clear registers
                delayTimer = 0;
                soundTimer = 0;
                Array.Clear(v);
                i = 0;
                // Clear screen
                Array.Clear(DisplayBuffer);
                NeedsRedraw = true;
                jumped = true;
                break;
            case (0x1, var n1, var n2, var n3): // Jump to address
                pc = ConcatNibbles(n1, n2, n3);
                jumped = true;
                break;
            case (0x2, var n1, var n2, var n3): // Call subroutine
                stack.Push((ushort)(pc + 2)); // Put return place on stack
                pc = ConcatNibbles(n1, n2, n3); // Goto subroutine
                jumped = true;
                break;
            case (0x3, var x, _, _): // Conditional skip v[x] equal last2
                if (v[x] == last2)
                {
                    pc += 4;
                    jumped = true;
                }
                break;
            case (0x4, var x, _, _): // Conditional skip v[x] not equal last2
                if (v[x] != last2)
                {
                    pc += 4;
                    jumped = true;
                }
                break;
            case (0x5, var x, var y, _): // Condition skip v[x] equal v[y]
                if (v[x] == v[y])
                {
                    pc += 4;
                    jumped = true;
                }
                break;
            case (0x6, var x, _, _): // Set v[x] to last2
                v[x] = last2;
                break;
            case (0x7, var x, _, _): // Add last2 to v[x]
                v[x] += last2;
                break;
            case (0x8, var x, var y, 0x0): // Set v[x] to v[y]
                v[x] = v[y];
                break;
            case (0x8, var x, var y, 0x1): // Set v[x] to v[x] | v[y]
                v[x] |= v[y];
                break;
            case (0x8, var x, var y, 0x2): // Set v[x] to v[x] & v[y]
                v[x] &= v[y];
                break;
            case (0x8, var x, var y, 0x3): // Set v[x] to v[x] ^ v[y]
                v[x] ^= v[y];
                break;
            case (0x8, var x, var y, 0x4): // Add with carry flag
                var sum = v[x] + v[y];
                v[x] = (byte)(sum & 0xFF);
                v[0xF] = (byte)(sum > 255 ? 1 : 0); // Indicate no borrow (1) or borrow (0)
                break;
            case (0x8, var x, var y, 0x5): // Subtract with borrow flag
                {
                    var sub = v[x] - v[y];
                    v[x] = (byte)(sub & 0xFF);
                    v[0xF] = (byte)(sub >= 0 ? 1 : 0); // Indicate no borrow (1) or borrow (0)
                }
                break;
            case (0x8, var x, _, 0x6): // v[x] >> 1 v[f] = least significant bit
                v[0xF] = (byte)(v[x] & 0x1);
                v[x] >>= 1;
                break;
            case (0x8, var x, var y, 0x7): // Subtract with borrow flag (y - x in x)
                {
                    var sub = v[y] - v[x];
                    v[x] = (byte)(sub & 0xFF);
                    v[0xF] = (byte)(sub >= 0 ? 1 : 0); // Indicate no borrow (1) or borrow (0)
                }
                break;
            case (0x8, var x, _, 0xE): // v[x] << 1 v[f] = most significant bit
                v[0xF] = (byte)((v[x] & 0b10000000) >> 7);
                v[x] <<= 1;
                break;
            case (0x9, var x, var y, 0x0): // Conditional skip if v[x] != v[y]
                if (v[x] != v[y])
                {
                    pc += 4;
                    jumped = true;
                }
                break;
            case (0xA, var n1, var n2, var n3): // Set i to address n1n2n3
                i = ConcatNibbles(n1, n2, n3);
                break;
            case (0xB, var n1, var n2, var n3): // Jump to address n1n2n3 + v[0]
                pc = (ushort)(ConcatNibbles(n1, n2, n3) + v[0]);
                jumped = true;
                break;
            case (0xC, var x, _, _): // v[x] = random number (0-255) & last2
                v[x] = (byte)(last2 & Random.Shared.Next(256));
                break;
            case (0xD, var x, var y, var n): // Draw sprite at (vx, vy) that's n high
                DrawSprite(v[x], v[y], n);
                NeedsRedraw = true;
                break;
            case (0xE, var x, 0x9, 0xE): // Conditional skip if keys(v[x])
                if (keys[v[x]])
                {
                    pc += 4;
                    jumped = true;
                }
                break;
            case (0xE, var x, 0xA, 0x1): // Conditional skip if not keys(v[x])
                if (!keys[v[x]])
                {
                    pc += 4;
                    jumped = true;
                }
                break;
            case (0xF, var x, 0x0, 0x7): // Set v[x] to delay timer
                v[x] = delayTimer;
                break;
            case (0xF, var x, 0x0, 0xA): // Wait until next key then store in v[x]
                // Wait here for the next key then continue
                for (var keyIndex = 0; keyIndex < AllowedKeys.Length; keyIndex++)
                {
                    if (!previousKeys[keyIndex] && keys[keyIndex])
                    {
                        v[x] = (byte)keyIndex;
                        break;
                    }
                }
                return;
            case (0xF, var x, 0x1, 0x5): // Set delay timer to v[x]
                delayTimer = v[x];
                break;
            case (0xF, var x, 0x1, 0x8): // Set sound timer to v[x]
                soundTimer = v[x];
                break;
            case (0xF, var x, 0x1, 0xE): // Add v[x] to i
                i += v[x];
                break;
            case (0xF, var x, 0x2, 0x9): // Set i to location of character v[x]
                i = (ushort)(v[x] * 5); // Built-in font set is 5 bytes apart
                break;
            case (0xF, var x, 0x3, 0x3): // Store BCD at v[x] in i, i+1, i+2;
                ram[i] = (byte)(v[x] / 100); // 100s digit
                ram[i + 1] = (byte)((v[x] % 100) / 10); // 10s digit
                ram[i + 2] = (byte)((v[x] % 100) % 10); // 1s digit
                break;
            case (0xF, var x, 0x5, 0x5): // Reg dump v0 to vx starting at i
                for (var r = 0; r <= x; r++)
                {
                    ram[i + r] = v[r];
                }
                break;
            case (0xF, var x, 0x6, 0x5): // Store i through i+r in v0 through vr
                for (var r = 0; r <= x; r++)
                {
                    v[r] = ram[i + r];
                }
                break;
            default:
                Debug.WriteLine($"Unknown opcode ({first:X}, {second:X}, {third:X}, {fourth:X})!");
                break;
        }

        if (!jumped)
        {
            pc += 2; // Increment program counter
        }
    }

    private static byte[] CreateRam(byte[] programData)
    {
        var ram = new byte[RamSize];
        FontSet.CopyTo(ram.AsSpan(0, FontSet.Length));
        programData.CopyTo(ram.AsSpan(512, programData.Length));

        return ram;
    }

    public const int RamSize = 4096;
    public const int ScreenWidth = 64;
    public const int ScreenHeight = 32;
    public const int SpriteWidth = 8;

    public static readonly TimeSpan TimerDelay = TimeSpan.FromSeconds(1d / 60);
    public static readonly TimeSpan FrameTimeExpected = TimeSpan.FromSeconds(1d / 500);

    public static readonly Input.Keys[] AllowedKeys =
    [
        Input.Keys.D0, Input.Keys.D1, Input.Keys.D2, Input.Keys.D3,
        Input.Keys.D4, Input.Keys.D5, Input.Keys.D6, Input.Keys.D7,
        Input.Keys.D8, Input.Keys.D9, Input.Keys.A, Input.Keys.B,
        Input.Keys.C, Input.Keys.D, Input.Keys.E, Input.Keys.F
    ];

    public static byte[] FontSet =
    [
        0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
        0x20, 0x60, 0x20, 0x20, 0x70, // 1
        0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
        0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
        0x90, 0x90, 0xF0, 0x10, 0x10, // 4
        0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
        0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
        0xF0, 0x10, 0x20, 0x40, 0x40, // 7
        0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
        0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
        0xF0, 0x90, 0xF0, 0x90, 0x90, // A
        0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
        0xF0, 0x80, 0x80, 0x80, 0xF0, // C
        0xE0, 0x90, 0x90, 0x90, 0xE0, // D
        0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
        0xF0, 0x80, 0xF0, 0x80, 0x80 // F
    ];

    private ushort ConcatNibbles(params int[] args)
    {
        var result = 0;

        foreach (var arg in args)
        {
            result = (result << 4) | (arg & 0xF);
        }

        return (ushort)result;
    }

    public void UpdateKeys(bool[] keys)
    {
        previousKeys = this.keys;
        this.keys = keys;
    }
}
