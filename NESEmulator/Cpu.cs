using System.Diagnostics;

namespace NESEmulator;

public class Cpu
{
    public Cpu(Ppu ppu, Rom rom)
    {
        this.ppu = ppu;
        this.rom = rom;

        PC = (ushort)(ReadMemory(ResetVector, MemMode.Absolute) | (ReadMemory(ResetVector + 1, MemMode.Absolute) << 8));

        instructions = instructions =
        [
            new Instruction(InstructionType.BRK, BRK, MemMode.Implied, 1, 7, 0), // 00
            new Instruction(InstructionType.ORA, ORA, MemMode.IndexedIndirect, 2, 6, 0), // 01
            new Instruction(InstructionType.KIL, Unimplemented, MemMode.Implied, 0, 2, 0), // 02
            new Instruction(InstructionType.SLO, Unimplemented, MemMode.IndexedIndirect, 0, 8, 0), // 03
            new Instruction(InstructionType.NOP, NOP, MemMode.Zeropage, 2, 3, 0), // 04
            new Instruction(InstructionType.ORA, ORA, MemMode.Zeropage, 2, 3, 0), // 05
            new Instruction(InstructionType.ASL, ASL, MemMode.Zeropage, 2, 5, 0), // 06
            new Instruction(InstructionType.SLO, Unimplemented, MemMode.Zeropage, 0, 5, 0), // 07
            new Instruction(InstructionType.PHP, PHP, MemMode.Implied, 1, 3, 0), // 08
            new Instruction(InstructionType.ORA, ORA, MemMode.Immediate, 2, 2, 0), // 09
            new Instruction(InstructionType.ASL, ASL, MemMode.Accumulator, 1, 2, 0), // 0a
            new Instruction(InstructionType.ANC, Unimplemented, MemMode.Immediate, 0, 2, 0), // 08
            new Instruction(InstructionType.NOP, NOP, MemMode.Absolute, 3, 4, 0), // 0c
            new Instruction(InstructionType.ORA, ORA, MemMode.Absolute, 3, 4, 0), // 0d
            new Instruction(InstructionType.ASL, ASL, MemMode.Absolute, 3, 6, 0), // 0e
            new Instruction(InstructionType.SLO, Unimplemented, MemMode.Absolute, 0, 6, 0), // 0f
            new Instruction(InstructionType.BPL, BPL, MemMode.Relative, 2,2, 1), // 10
            new Instruction(InstructionType.ORA, ORA, MemMode.IndirectIndexed, 2, 5, 1), // 11
            new Instruction(InstructionType.KIL, Unimplemented, MemMode.Implied, 0, 2, 0), // 12
            new Instruction(InstructionType.SLO, Unimplemented, MemMode.IndirectIndexed, 0, 8, 0), // 13
            new Instruction(InstructionType.NOP, NOP, MemMode.ZeropageX, 2, 4, 0), // 14
            new Instruction(InstructionType.ORA, ORA, MemMode.ZeropageX, 2, 4, 0), // 15
            new Instruction(InstructionType.ASL, ASL, MemMode.ZeropageX, 2, 6, 0), // 16
            new Instruction(InstructionType.SLO, Unimplemented, MemMode.ZeropageX, 0, 6, 0), // 17
            new Instruction(InstructionType.CLC, CLC, MemMode.Implied, 1, 2, 0), // 18
            new Instruction(InstructionType.ORA, ORA, MemMode.AbsoluteY, 3, 4, 1), // 19
            new Instruction(InstructionType.NOP, NOP, MemMode.Implied, 1, 2, 0), // 1a
            new Instruction(InstructionType.SLO, Unimplemented, MemMode.AbsoluteY, 0, 7, 0), // 18
            new Instruction(InstructionType.NOP, NOP, MemMode.AbsoluteX, 3, 4, 1), // 1c
            new Instruction(InstructionType.ORA, ORA, MemMode.AbsoluteX, 3, 4, 1), // 1d
            new Instruction(InstructionType.ASL, ASL, MemMode.AbsoluteX, 3, 7, 0), // 1e
            new Instruction(InstructionType.SLO, Unimplemented, MemMode.AbsoluteX, 0, 7, 0), // 18
            new Instruction(InstructionType.JSR, JSR, MemMode.Absolute, 3,6, 0), // 20
            new Instruction(InstructionType.AND, AND, MemMode.IndexedIndirect, 2, 6, 0), // 21
            new Instruction(InstructionType.KIL, Unimplemented, MemMode.Implied, 0, 2, 0), // 22
            new Instruction(InstructionType.RLA, Unimplemented, MemMode.IndexedIndirect, 0, 8, 0), // 23
            new Instruction(InstructionType.BIT, BIT, MemMode.Zeropage, 2, 3, 0), // 24
            new Instruction(InstructionType.AND, AND, MemMode.Zeropage, 2, 3, 0), // 25
            new Instruction(InstructionType.ROL, ROL, MemMode.Zeropage, 2, 5, 0), // 26
            new Instruction(InstructionType.RLA, Unimplemented, MemMode.Zeropage, 0, 5, 0), // 27
            new Instruction(InstructionType.PLP, PLP, MemMode.Implied, 1, 4, 0), // 28
            new Instruction(InstructionType.AND, AND, MemMode.Immediate, 2, 2, 0), // 29
            new Instruction(InstructionType.ROL, ROL, MemMode.Accumulator, 1, 2, 0), // 2a
            new Instruction(InstructionType.ANC, Unimplemented, MemMode.Immediate, 0, 2, 0), // 2b
            new Instruction(InstructionType.BIT, BIT, MemMode.Absolute, 3, 4, 0), // 2c
            new Instruction(InstructionType.AND, AND, MemMode.Absolute, 3, 4, 0), // 2d
            new Instruction(InstructionType.ROL, ROL, MemMode.Absolute, 3, 6, 0), // 2e
            new Instruction(InstructionType.RLA, Unimplemented, MemMode.Absolute, 0, 6, 0), // 2f
            new Instruction(InstructionType.BMI, BMI, MemMode.Relative, 2, 2, 1), // 30
            new Instruction(InstructionType.AND, AND, MemMode.IndirectIndexed, 2, 5, 1), // 31
            new Instruction(InstructionType.KIL, Unimplemented, MemMode.Implied, 0, 2, 0), // 32
            new Instruction(InstructionType.RLA, Unimplemented, MemMode.IndirectIndexed, 0, 8, 0), // 33
            new Instruction(InstructionType.NOP, NOP, MemMode.ZeropageX, 2, 4, 0), // 34
            new Instruction(InstructionType.AND, AND, MemMode.ZeropageX, 2, 4, 0), // 35
            new Instruction(InstructionType.ROL, ROL, MemMode.ZeropageX, 2, 6, 0), // 36
            new Instruction(InstructionType.RLA, Unimplemented, MemMode.ZeropageX, 0, 6, 0), // 37
            new Instruction(InstructionType.SEC, SEC, MemMode.Implied, 1, 2, 0), // 38
            new Instruction(InstructionType.AND, AND, MemMode.AbsoluteY, 3, 4, 1), // 39
            new Instruction(InstructionType.NOP, NOP, MemMode.Implied, 1, 2, 0), // 3a
            new Instruction(InstructionType.RLA, Unimplemented, MemMode.AbsoluteY, 0, 7, 0), // 3b
            new Instruction(InstructionType.NOP, NOP, MemMode.AbsoluteX, 3, 4, 1), // 3c
            new Instruction(InstructionType.AND, AND, MemMode.AbsoluteX, 3, 4, 1), // 3d
            new Instruction(InstructionType.ROL, ROL, MemMode.AbsoluteX, 3, 7, 0), // 3e
            new Instruction(InstructionType.RLA, Unimplemented, MemMode.AbsoluteX, 0, 7, 0), // 3f
            new Instruction(InstructionType.RTI, RTI, MemMode.Implied, 1, 6, 0), // 40
            new Instruction(InstructionType.EOR, EOR, MemMode.IndexedIndirect, 2, 6, 0), // 41
            new Instruction(InstructionType.KIL, Unimplemented, MemMode.Implied, 0, 2, 0), // 42
            new Instruction(InstructionType.SRE, Unimplemented, MemMode.IndexedIndirect, 0, 8, 0), // 43
            new Instruction(InstructionType.NOP, NOP, MemMode.Zeropage, 2, 3, 0), // 44
            new Instruction(InstructionType.EOR, EOR, MemMode.Zeropage, 2, 3, 0), // 45
            new Instruction(InstructionType.LSR, LSR, MemMode.Zeropage, 2, 5, 0), // 46
            new Instruction(InstructionType.SRE, Unimplemented, MemMode.Zeropage, 0, 5, 0), // 47
            new Instruction(InstructionType.PHA, PHA, MemMode.Implied, 1, 3, 0), // 48
            new Instruction(InstructionType.EOR, EOR, MemMode.Immediate, 2, 2, 0), // 49
            new Instruction(InstructionType.LSR, LSR, MemMode.Accumulator, 1, 2, 0), // 4a
            new Instruction(InstructionType.ALR, Unimplemented, MemMode.Immediate, 0, 2, 0), // 4b
            new Instruction(InstructionType.JMP, JMP, MemMode.Absolute, 3, 3, 0), // 4c
            new Instruction(InstructionType.EOR, EOR, MemMode.Absolute, 3, 4, 0), // 4d
            new Instruction(InstructionType.LSR, LSR, MemMode.Absolute, 3, 6, 0), // 4e
            new Instruction(InstructionType.SRE, Unimplemented, MemMode.Absolute, 0, 6, 0), // 4f
            new Instruction(InstructionType.BVC, BVC, MemMode.Relative, 2, 2, 1), // 50
            new Instruction(InstructionType.EOR, EOR, MemMode.IndirectIndexed, 2, 5, 1), // 51
            new Instruction(InstructionType.KIL, Unimplemented, MemMode.Implied, 0, 2, 0), // 52
            new Instruction(InstructionType.SRE, Unimplemented, MemMode.IndirectIndexed, 0, 8, 0), // 53
            new Instruction(InstructionType.NOP, NOP, MemMode.ZeropageX, 2, 4, 0), // 54
            new Instruction(InstructionType.EOR, EOR, MemMode.ZeropageX, 2, 4, 0), // 55
            new Instruction(InstructionType.LSR, LSR, MemMode.ZeropageX, 2, 6, 0), // 56
            new Instruction(InstructionType.SRE, Unimplemented, MemMode.ZeropageX, 0, 6, 0), // 57
            new Instruction(InstructionType.CLI, CLI, MemMode.Implied, 1, 2, 0), // 58
            new Instruction(InstructionType.EOR, EOR, MemMode.AbsoluteY, 3, 4, 1), // 59
            new Instruction(InstructionType.NOP, NOP, MemMode.Implied, 1, 2, 0), // 5a
            new Instruction(InstructionType.SRE, Unimplemented, MemMode.AbsoluteY, 0, 7, 0), // 5b
            new Instruction(InstructionType.NOP, NOP, MemMode.AbsoluteX, 3, 4, 1), // 5c
            new Instruction(InstructionType.EOR, EOR, MemMode.AbsoluteX, 3, 4, 1), // 5d
            new Instruction(InstructionType.LSR, LSR, MemMode.AbsoluteX, 3, 7, 0), // 5e
            new Instruction(InstructionType.SRE, Unimplemented, MemMode.AbsoluteX, 0, 7, 0), // 5f
            new Instruction(InstructionType.RTS, RTS, MemMode.Implied, 1, 6, 0), // 60
            new Instruction(InstructionType.ADC, ADC, MemMode.IndexedIndirect, 2, 6, 0), // 61
            new Instruction(InstructionType.KIL, Unimplemented, MemMode.Implied, 0, 2, 0), // 62
            new Instruction(InstructionType.RRA, Unimplemented, MemMode.IndexedIndirect, 0, 8, 0), // 63
            new Instruction(InstructionType.NOP, NOP, MemMode.Zeropage, 2, 3, 0), // 64
            new Instruction(InstructionType.ADC, ADC, MemMode.Zeropage, 2, 3, 0), // 65
            new Instruction(InstructionType.ROR, ROR, MemMode.Zeropage, 2, 5, 0), // 66
            new Instruction(InstructionType.RRA, Unimplemented, MemMode.Zeropage, 0, 5, 0), // 67
            new Instruction(InstructionType.PLA, PLA, MemMode.Implied, 1, 4, 0), // 68
            new Instruction(InstructionType.ADC, ADC, MemMode.Immediate, 2, 2, 0), // 69
            new Instruction(InstructionType.ROR, ROR, MemMode.Accumulator, 1, 2, 0), // 6a
            new Instruction(InstructionType.ARR, Unimplemented, MemMode.Immediate, 0, 2, 0), // 6b
            new Instruction(InstructionType.JMP, JMP, MemMode.Indirect, 3, 5, 0), // 6c
            new Instruction(InstructionType.ADC, ADC, MemMode.Absolute, 3, 4, 0), // 6d
            new Instruction(InstructionType.ROR, ROR, MemMode.Absolute, 3, 6, 0), // 6e
            new Instruction(InstructionType.RRA, Unimplemented, MemMode.Absolute, 0, 6, 0), // 6f
            new Instruction(InstructionType.BVS, BVS, MemMode.Relative, 2, 2, 1), // 70
            new Instruction(InstructionType.ADC, ADC, MemMode.IndirectIndexed, 2, 5, 1), // 71
            new Instruction(InstructionType.KIL, Unimplemented, MemMode.Implied, 0, 2, 0), // 72
            new Instruction(InstructionType.RRA, Unimplemented, MemMode.IndirectIndexed, 0, 8, 0), // 73
            new Instruction(InstructionType.NOP, NOP, MemMode.ZeropageX, 2, 4, 0), // 74
            new Instruction(InstructionType.ADC, ADC, MemMode.ZeropageX, 2, 4, 0), // 75
            new Instruction(InstructionType.ROR, ROR, MemMode.ZeropageX, 2, 6, 0), // 76
            new Instruction(InstructionType.RRA, Unimplemented, MemMode.ZeropageX, 0, 6, 0), // 77
            new Instruction(InstructionType.SEI, SEI, MemMode.Implied, 1, 2, 0), // 78
            new Instruction(InstructionType.ADC, ADC, MemMode.AbsoluteY, 3, 4, 1), // 79
            new Instruction(InstructionType.NOP, NOP, MemMode.Implied, 1, 2, 0), // 7a
            new Instruction(InstructionType.RRA, Unimplemented, MemMode.AbsoluteY, 0, 7, 0), // 7b
            new Instruction(InstructionType.NOP, NOP, MemMode.AbsoluteX, 3, 4, 1), // 7c
            new Instruction(InstructionType.ADC, ADC, MemMode.AbsoluteX, 3, 4, 1), // 7d
            new Instruction(InstructionType.ROR, ROR, MemMode.AbsoluteX, 3, 7, 0), // 7e
            new Instruction(InstructionType.RRA, Unimplemented, MemMode.AbsoluteX, 0, 7, 0), // 7f
            new Instruction(InstructionType.NOP, NOP, MemMode.Immediate, 2, 2, 0), // 80
            new Instruction(InstructionType.STA, STA, MemMode.IndexedIndirect, 2, 6, 0), // 81
            new Instruction(InstructionType.NOP, NOP, MemMode.Immediate, 0, 2, 0), // 82
            new Instruction(InstructionType.SAX, Unimplemented, MemMode.IndexedIndirect, 0, 6, 0), // 83
            new Instruction(InstructionType.STY, STY, MemMode.Zeropage, 2, 3, 0), // 84
            new Instruction(InstructionType.STA, STA, MemMode.Zeropage, 2, 3, 0), // 85
            new Instruction(InstructionType.STX, STX, MemMode.Zeropage, 2, 3, 0), // 86
            new Instruction(InstructionType.SAX, Unimplemented, MemMode.Zeropage, 0, 3, 0), // 87
            new Instruction(InstructionType.DEY, DEY, MemMode.Implied, 1, 2, 0), // 88
            new Instruction(InstructionType.NOP, NOP, MemMode.Immediate, 0, 2, 0), // 89
            new Instruction(InstructionType.TXA, TXA, MemMode.Implied, 1, 2, 0), // 8a
            new Instruction(InstructionType.XAA, Unimplemented, MemMode.Immediate, 0, 2, 0), // 8b
            new Instruction(InstructionType.STY, STY, MemMode.Absolute, 3, 4, 0), // 8c
            new Instruction(InstructionType.STA, STA, MemMode.Absolute, 3, 4, 0), // 8d
            new Instruction(InstructionType.STX, STX, MemMode.Absolute, 3, 4, 0), // 8e
            new Instruction(InstructionType.SAX, Unimplemented, MemMode.Absolute, 0, 4, 0), // 8f
            new Instruction(InstructionType.BCC, BCC, MemMode.Relative, 2, 2, 1), // 90
            new Instruction(InstructionType.STA, STA, MemMode.IndirectIndexed, 2, 6, 0), // 91
            new Instruction(InstructionType.KIL, Unimplemented, MemMode.Implied, 0, 2, 0), // 92
            new Instruction(InstructionType.AHX, Unimplemented, MemMode.IndirectIndexed, 0, 6, 0), // 93
            new Instruction(InstructionType.STY, STY, MemMode.ZeropageX, 2, 4, 0), // 94
            new Instruction(InstructionType.STA, STA, MemMode.ZeropageX, 2, 4, 0), // 95
            new Instruction(InstructionType.STX, STX, MemMode.ZeropageY, 2, 4, 0), // 96
            new Instruction(InstructionType.SAX, Unimplemented, MemMode.ZeropageY, 0, 4, 0), // 97
            new Instruction(InstructionType.TYA, TYA, MemMode.Implied, 1, 2, 0), // 98
            new Instruction(InstructionType.STA, STA, MemMode.AbsoluteY, 3, 5, 0), // 99
            new Instruction(InstructionType.TXS, TXS, MemMode.Implied, 1, 2, 0), // 9a
            new Instruction(InstructionType.TAS, Unimplemented, MemMode.AbsoluteY, 0, 5, 0), // 9b
            new Instruction(InstructionType.SHY, Unimplemented, MemMode.AbsoluteX, 0, 5, 0), // 9c
            new Instruction(InstructionType.STA, STA, MemMode.AbsoluteX, 3, 5, 0), // 9d
            new Instruction(InstructionType.SHX, Unimplemented, MemMode.AbsoluteY, 0, 5, 0), // 9e
            new Instruction(InstructionType.AHX, Unimplemented, MemMode.AbsoluteY, 0, 5, 0), // 9f
            new Instruction(InstructionType.LDY, LDY, MemMode.Immediate, 2, 2, 0), // a0
            new Instruction(InstructionType.LDA, LDA, MemMode.IndexedIndirect, 2, 6, 0), // a1
            new Instruction(InstructionType.LDX, LDX, MemMode.Immediate, 2, 2, 0), // a2
            new Instruction(InstructionType.LAX, Unimplemented, MemMode.IndexedIndirect, 0, 6, 0), // a3
            new Instruction(InstructionType.LDY, LDY, MemMode.Zeropage, 2, 3, 0), // a4
            new Instruction(InstructionType.LDA, LDA, MemMode.Zeropage, 2, 3, 0), // a5
            new Instruction(InstructionType.LDX, LDX, MemMode.Zeropage, 2, 3, 0), // a6
            new Instruction(InstructionType.LAX, Unimplemented, MemMode.Zeropage, 0, 3, 0), // a7
            new Instruction(InstructionType.TAY, TAY, MemMode.Implied, 1, 2, 0), // a8
            new Instruction(InstructionType.LDA, LDA, MemMode.Immediate, 2, 2, 0), // a9
            new Instruction(InstructionType.TAX, TAX, MemMode.Implied, 1, 2, 0), // aa
            new Instruction(InstructionType.LAX, Unimplemented, MemMode.Immediate, 0, 2, 0), // ab
            new Instruction(InstructionType.LDY, LDY, MemMode.Absolute, 3, 4, 0), // ac
            new Instruction(InstructionType.LDA, LDA, MemMode.Absolute, 3, 4, 0), // ad
            new Instruction(InstructionType.LDX, LDX, MemMode.Absolute, 3, 4, 0), // ae
            new Instruction(InstructionType.LAX, Unimplemented, MemMode.Absolute, 0, 4, 0), // af
            new Instruction(InstructionType.BCS, BCS, MemMode.Relative, 2, 2, 1), // b0
            new Instruction(InstructionType.LDA, LDA, MemMode.IndirectIndexed, 2, 5, 1), // b1
            new Instruction(InstructionType.KIL, Unimplemented, MemMode.Implied, 0, 2, 0), // b2
            new Instruction(InstructionType.LAX, Unimplemented, MemMode.IndirectIndexed, 0, 5, 1), // b3
            new Instruction(InstructionType.LDY, LDY, MemMode.ZeropageX, 2, 4, 0), // b4
            new Instruction(InstructionType.LDA, LDA, MemMode.ZeropageX, 2, 4, 0), // b5
            new Instruction(InstructionType.LDX, LDX, MemMode.ZeropageY, 2, 4, 0), // b6
            new Instruction(InstructionType.LAX, Unimplemented, MemMode.ZeropageY, 0, 4, 0), // b7
            new Instruction(InstructionType.CLV, CLV, MemMode.Implied, 1, 2, 0), // b8
            new Instruction(InstructionType.LDA, LDA, MemMode.AbsoluteY, 3, 4, 1), // b9
            new Instruction(InstructionType.TSX, TSX, MemMode.Implied, 1, 2, 0), // ba
            new Instruction(InstructionType.LAS, Unimplemented, MemMode.AbsoluteY, 0, 4, 1), // bb
            new Instruction(InstructionType.LDY, LDY, MemMode.AbsoluteX, 3, 4, 1), // bc
            new Instruction(InstructionType.LDA, LDA, MemMode.AbsoluteX, 3, 4, 1), // bd
            new Instruction(InstructionType.LDX, LDX, MemMode.AbsoluteY, 3, 4, 1), // be
            new Instruction(InstructionType.LAX, Unimplemented, MemMode.AbsoluteY, 0, 4, 1), // bf
            new Instruction(InstructionType.CPY, CPY, MemMode.Immediate, 2, 2, 0), // c0
            new Instruction(InstructionType.CMP, CMP, MemMode.IndexedIndirect, 2, 6, 0), // c1
            new Instruction(InstructionType.NOP, NOP, MemMode.Immediate, 0, 2, 0), // c2
            new Instruction(InstructionType.DCP, Unimplemented, MemMode.IndexedIndirect, 0, 8, 0), // c3
            new Instruction(InstructionType.CPY, CPY, MemMode.Zeropage, 2, 3, 0), // c4
            new Instruction(InstructionType.CMP, CMP, MemMode.Zeropage, 2, 3, 0), // c5
            new Instruction(InstructionType.DEC, DEC, MemMode.Zeropage, 2, 5, 0), // c6
            new Instruction(InstructionType.DCP, Unimplemented, MemMode.Zeropage, 0, 5, 0), // c7
            new Instruction(InstructionType.INY, INY, MemMode.Implied, 1, 2, 0), // c8
            new Instruction(InstructionType.CMP, CMP, MemMode.Immediate, 2, 2, 0), // c9
            new Instruction(InstructionType.DEX, DEX, MemMode.Implied, 1, 2, 0), // ca
            new Instruction(InstructionType.AXS, Unimplemented, MemMode.Immediate, 0, 2, 0), // cb
            new Instruction(InstructionType.CPY, CPY, MemMode.Absolute, 3, 4, 0), // cc
            new Instruction(InstructionType.CMP, CMP, MemMode.Absolute, 3, 4, 0), // cd
            new Instruction(InstructionType.DEC, DEC, MemMode.Absolute, 3, 6, 0), // ce
            new Instruction(InstructionType.DCP, Unimplemented, MemMode.Absolute, 0, 6, 0), // cf
            new Instruction(InstructionType.BNE, BNE, MemMode.Relative, 2,2, 1), // d0
            new Instruction(InstructionType.CMP, CMP, MemMode.IndirectIndexed, 2, 5, 1), // d1
            new Instruction(InstructionType.KIL, Unimplemented, MemMode.Implied, 0, 2, 0), // d2
            new Instruction(InstructionType.DCP, Unimplemented, MemMode.IndirectIndexed, 0, 8, 0), // d3
            new Instruction(InstructionType.NOP, NOP, MemMode.ZeropageX, 2, 4, 0), // d4
            new Instruction(InstructionType.CMP, CMP, MemMode.ZeropageX, 2, 4, 0), // d5
            new Instruction(InstructionType.DEC, DEC, MemMode.ZeropageX, 2, 6, 0), // d6
            new Instruction(InstructionType.DCP, Unimplemented, MemMode.ZeropageX, 0, 6, 0), // d7
            new Instruction(InstructionType.CLD, CLD, MemMode.Implied, 1, 2, 0), // d8
            new Instruction(InstructionType.CMP, CMP, MemMode.AbsoluteY, 3, 4, 1), // d9
            new Instruction(InstructionType.NOP, NOP, MemMode.Implied, 1, 2, 0), // da
            new Instruction(InstructionType.DCP, Unimplemented, MemMode.AbsoluteY, 0, 7, 0), // db
            new Instruction(InstructionType.NOP, NOP, MemMode.AbsoluteX, 3, 4, 1), // dc
            new Instruction(InstructionType.CMP, CMP, MemMode.AbsoluteX, 3, 4, 1), // dd
            new Instruction(InstructionType.DEC, DEC, MemMode.AbsoluteX, 3, 7, 0), // de
            new Instruction(InstructionType.DCP, Unimplemented, MemMode.AbsoluteX, 0, 7, 0), // df
            new Instruction(InstructionType.CPX, CPX, MemMode.Immediate, 2, 2, 0), // e0
            new Instruction(InstructionType.SBC, SBC, MemMode.IndexedIndirect, 2, 6, 0), // e1
            new Instruction(InstructionType.NOP, NOP, MemMode.Immediate, 0, 2, 0), // e2
            new Instruction(InstructionType.ISC, Unimplemented, MemMode.IndexedIndirect, 0, 8, 0), // e3
            new Instruction(InstructionType.CPX, CPX, MemMode.Zeropage, 2, 3, 0), // e4
            new Instruction(InstructionType.SBC, SBC, MemMode.Zeropage, 2, 3, 0), // e5
            new Instruction(InstructionType.INC, INC, MemMode.Zeropage, 2, 5, 0), // e6
            new Instruction(InstructionType.ISC, Unimplemented, MemMode.Zeropage, 0, 5, 0), // e7
            new Instruction(InstructionType.INX, INX, MemMode.Implied, 1, 2, 0), // e8
            new Instruction(InstructionType.SBC, SBC, MemMode.Immediate, 2, 2, 0), // e9
            new Instruction(InstructionType.NOP, NOP, MemMode.Implied, 1, 2, 0), // ea
            new Instruction(InstructionType.SBC, SBC, MemMode.Immediate, 0, 2, 0), // eb
            new Instruction(InstructionType.CPX, CPX, MemMode.Absolute, 3, 4, 0), // ec
            new Instruction(InstructionType.SBC, SBC, MemMode.Absolute, 3, 4, 0), // ed
            new Instruction(InstructionType.INC, INC, MemMode.Absolute, 3, 6, 0), // ee
            new Instruction(InstructionType.ISC, Unimplemented, MemMode.Absolute, 0, 6, 0), // ef
            new Instruction(InstructionType.BEQ, BEQ, MemMode.Relative, 2, 2, 1), // f0
            new Instruction(InstructionType.SBC, SBC, MemMode.IndirectIndexed, 2, 5, 1), // f1
            new Instruction(InstructionType.KIL, Unimplemented, MemMode.Implied, 0, 2, 0), // f2
            new Instruction(InstructionType.ISC, Unimplemented, MemMode.IndirectIndexed, 0, 8, 0), // f3
            new Instruction(InstructionType.NOP, NOP, MemMode.ZeropageX, 2, 4, 0), // f4
            new Instruction(InstructionType.SBC, SBC, MemMode.ZeropageX, 2, 4, 0), // f5
            new Instruction(InstructionType.INC, INC, MemMode.ZeropageX, 2, 6, 0), // f6
            new Instruction(InstructionType.ISC, Unimplemented, MemMode.ZeropageX, 0, 6, 0), // f7
            new Instruction(InstructionType.SED, SED, MemMode.Implied, 1, 2, 0), // f8
            new Instruction(InstructionType.SBC, SBC, MemMode.AbsoluteY, 3, 4, 1), // f9
            new Instruction(InstructionType.NOP, NOP, MemMode.Implied, 1,  2, 0), // fa
            new Instruction(InstructionType.ISC, Unimplemented, MemMode.AbsoluteY, 0, 7, 0), // fb
            new Instruction(InstructionType.NOP, NOP, MemMode.AbsoluteX, 3, 4, 1), // fc
            new Instruction(InstructionType.SBC, SBC, MemMode.AbsoluteX, 3, 4, 1), // fd
            new Instruction(InstructionType.INC, INC, MemMode.AbsoluteX, 3, 7, 0), // fe
            new Instruction(InstructionType.ISC, Unimplemented, MemMode.AbsoluteX, 0, 7, 0) //ff
        ];
    }

    private const int StackPointerReset = 0xFD;
    private const int StackStart = 0x100;
    private const int ResetVector = 0xFFFC;
    private const int NmiVector = 0xFFFA;
    private const int IrqBrkVector = 0xFFFE;
    private const int MemSize = 2048;

    private readonly Ppu ppu;
    private readonly Rom rom;
    // Memory on the CPU
    private readonly byte[] ram = new byte[MemSize];
    // Registers
    private byte A = 0;
    private byte X = 0;
    private byte Y = 0;
    private byte SP = StackPointerReset;
    public ushort PC { get; set; }
    // Flags
    private bool C = false; // Carry
    private bool Z = false; // Zero
    private bool I = true; // Interrupt disable
    private bool D = false; // Decimal mode
    private bool B = false; // Break command
    private bool V = false; // Overflow
    private bool N = false; // Negative

    // Miscellaneous State
    private bool jumped = false;
    private bool pageCrossed = false;
    public int CpuTicks { get; private set; } = 0;
    private int stall = 0; // Number of cycles to stall

    public Joypad Joypad1 { get; } = new Joypad();

    private readonly Instruction[] instructions;

    // Add memory to accumulator with carry
    private void ADC(Instruction instruction, int data)
    {
        var src = ReadMemory(data, instruction.Mode);
        var signedResult = src + A + (C ? 1 : 0);
        V = (~(A ^ src) & (A ^ signedResult) & 0x80) != 0;
        A += (byte)(src + (C ? 1 : 0));
        C = signedResult > 0xFF;
        SetZN(A);
    }

    // Bitwise AND with accumulator
    private void AND(Instruction instruction, int data)
    {
        var src = ReadMemory(data, instruction.Mode);
        A &= src;
        SetZN(A);
    }

    // Arithmethic shift left
    private void ASL(Instruction instruction, int data)
    {
        var src = instruction.Mode == MemMode.Accumulator
            ? A
            : ReadMemory(data, instruction.Mode);
        C = (src >> 7) == 1; // Carry is set to 7th bit
        src <<= 1;
        SetZN(src);
        if (instruction.Mode == MemMode.Accumulator)
        {
            A = src;
        }
        else
        {
            WriteMemory(data, instruction.Mode, src);
        }
    }

    // Branch if carry clear
    private void BCC(Instruction instruction, int data)
    {
        if (!C)
        {
            PC = AddressForMode(data, instruction.Mode);
            jumped = true;
        }
    }

    // Branch if carry set
    private void BCS(Instruction instruction, int data)
    {
        if (C)
        {
            PC = AddressForMode(data, instruction.Mode);
            jumped = true;
        }
    }

    // Branch on result zero
    private void BEQ(Instruction instruction, int data)
    {
        if (Z)
        {
            PC = AddressForMode(data, instruction.Mode);
            jumped = true;
        }
    }

    // Bit test bits in memory with accumulator
    private void BIT(Instruction instruction, int data)
    {
        var src = ReadMemory(data, instruction.Mode);
        V = ((src >> 6) & 1) == 1;
        Z = (src & A) == 0;
        N = (src >> 7) == 1;
    }

    // Branch on result minus
    private void BMI(Instruction instruction, int data)
    {
        if (N)
        {
            PC = AddressForMode(data, instruction.Mode);
            jumped = true;
        }
    }

    // Branch on result not zero
    private void BNE(Instruction instruction, int data)
    {
        if (!Z)
        {
            PC = AddressForMode(data, instruction.Mode);
            jumped = true;
        }
    }

    // Branch on result plus
    private void BPL(Instruction instruction, int data)
    {
        if (!N)
        {
            PC = AddressForMode(data, instruction.Mode);
            jumped = true;
        }
    }

    // Force break
    private void BRK(Instruction instruction, int data)
    {
        PC += 2;
        // Push PC to stack
        StackPush((byte)(PC >> 8));
        StackPush((byte)PC);
        // Push statis to stack
        B = true;
        StackPush(Status);
        B = false;
        I = true;
        // Set PC to reset vector
        PC = (ushort)(ReadMemory(IrqBrkVector, MemMode.Absolute) | (ReadMemory(IrqBrkVector + 1, MemMode.Absolute) << 8));
        jumped = true;
    }

    // Branch on overflow clear
    private void BVC(Instruction instruction, int data)
    {
        if (!V)
        {
            PC = AddressForMode(data, instruction.Mode);
            jumped = true;
        }
    }

    // Branch on overflow set
    private void BVS(Instruction instruction, int data)
    {
        if (V)
        {
            PC = AddressForMode(data, instruction.Mode);
            jumped = true;
        }
    }

    // Clear carry
    private void CLC(Instruction instruction, int data)
    {
        C = false;
    }

    // Clear decimal
    private void CLD(Instruction instruction, int data)
    {
        D = false;
    }

    // Clear interrupt
    private void CLI(Instruction instruction, int data)
    {
        I = false;
    }

    // Clear overflow
    private void CLV(Instruction instruction, int data)
    {
        V = false;
    }

    // Compare accumulator
    private void CMP(Instruction instruction, int data)
    {
        var src = ReadMemory(data, instruction.Mode);
        C = A >= src;
        SetZN((byte)(A - src));
    }

    // Compare X register
    private void CPX(Instruction instruction, int data)
    {
        var src = ReadMemory(data, instruction.Mode);
        C = X >= src;
        SetZN((byte)(X - src));
    }

    // Compare Y register
    private void CPY(Instruction instruction, int data)
    {
        var src = ReadMemory(data, instruction.Mode);
        C = Y >= src;
        SetZN((byte)(Y - src));
    }

    // Decrement memory
    private void DEC(Instruction instruction, int data)
    {
        var src = ReadMemory(data, instruction.Mode);
        src--;
        WriteMemory(data, instruction.Mode, src);
        SetZN(src);
    }

    // Decrement X
    private void DEX(Instruction instruction, int data)
    {
        X--;
        SetZN(X);
    }

    // Decrement Y
    private void DEY(Instruction instruction, int data)
    {
        Y--;
        SetZN(Y);
    }

    // Exclusive or memory with accumulator
    private void EOR(Instruction instruction, int data)
    {
        A ^= ReadMemory(data, instruction.Mode);
        SetZN(A);
    }

    // Increment memory
    private void INC(Instruction instruction, int data)
    {
        var src = ReadMemory(data, instruction.Mode);
        src++;
        WriteMemory(data, instruction.Mode, src);
        SetZN(src);
    }

    // Increment X
    private void INX(Instruction instruction, int data)
    {
        X++;
        SetZN(X);
    }

    // Increment Y
    private void INY(Instruction instruction, int data)
    {
        Y++;
        SetZN(Y);
    }

    // Jump
    private void JMP(Instruction instruction, int data)
    {
        PC = AddressForMode(data, instruction.Mode);
        jumped = true;
    }

    // Jump to subroutine
    private void JSR(Instruction instruction, int data)
    {
        PC += 2;
        // Push PC to stack
        StackPush((byte)(PC >> 8));
        StackPush((byte)PC);
        // Jump to subroutine
        PC = AddressForMode(data, instruction.Mode);
        jumped = true;
    }

    // Load accumulator with memory
    private void LDA(Instruction instruction, int data)
    {
        A = ReadMemory(data, instruction.Mode);
        SetZN(A);
    }

    // Load X with memory
    private void LDX(Instruction instruction, int data)
    {
        X = ReadMemory(data, instruction.Mode);
        SetZN(X);
    }

    // Load Y with memory
    private void LDY(Instruction instruction, int data)
    {
        Y = ReadMemory(data, instruction.Mode);
        SetZN(Y);
    }

    // Logical shift right
    private void LSR(Instruction instruction, int data)
    {
        var src = instruction.Mode == MemMode.Accumulator
            ? A
            : ReadMemory(data, instruction.Mode);
        C = (src & 1) == 1; // Carry is set to 0th bit
        src >>= 1;
        SetZN(src);
        if (instruction.Mode == MemMode.Accumulator)
        {
            A = src;
        }
        else
        {
            WriteMemory(data, instruction.Mode, src);
        }
    }

    // No op
    private void NOP(Instruction instruction, int data)
    {
    }

    // Or memory with accumulator
    private void ORA(Instruction instruction, int data)
    {
        A |= ReadMemory(data, instruction.Mode);
        SetZN(A);
    }

    // Push accumulator
    private void PHA(Instruction instruction, int data)
    {
        StackPush(A);
    }

    // Push status
    private void PHP(Instruction instruction, int data)
    {
        // https://www.nesdev.org/the%20'B'%20flag%20&%20BRK%20instruction.txt
        B = true;
        StackPush(Status);
        B = false;
    }

    // Pull accumulator
    private void PLA(Instruction instruction, int data)
    {
        A = StackPop();
        SetZN(A);
    }

    // Pull status
    private void PLP(Instruction instruction, int data)
    {
        Status = StackPop();
    }

    // Rotate one bit left
    private void ROL(Instruction instruction, int data)
    {
        var src = instruction.Mode == MemMode.Accumulator
            ? A
            : ReadMemory(data, instruction.Mode);
        var oldC = C;
        C = ((src >> 7) & 1) == 1; // Carry is set to 7th bit
        src = (byte)((src << 1) | (oldC ? 1 : 0));
        SetZN(src);
        if (instruction.Mode == MemMode.Accumulator)
        {
            A = src;
        }
        else
        {
            WriteMemory(data, instruction.Mode, src);
        }
    }

    // Rotate one bit right
    private void ROR(Instruction instruction, int data)
    {
        var src = instruction.Mode == MemMode.Accumulator
            ? A
            : ReadMemory(data, instruction.Mode);
        var oldC = C;
        C = (src & 1) == 1; // Carry is set to 0th bit
        src = (byte)((src >> 1) | ((oldC ? 1 : 0) << 7));
        SetZN(src);
        if (instruction.Mode == MemMode.Accumulator)
        {
            A = src;
        }
        else
        {
            WriteMemory(data, instruction.Mode, src);
        }
    }

    // Return from interrupt
    private void RTI(Instruction instruction, int data)
    {
        // Pull status out
        Status = StackPop();
        // Pull PC out
        var lb = StackPop();
        var hb = StackPop();
        PC = (ushort)((hb << 8) | lb);
        jumped = true;
    }

    // Return from subroutine
    private void RTS(Instruction instruction, int data)
    {
        // Pull PC out
        var lb = StackPop();
        var hb = StackPop();
        PC = (ushort)(((hb << 8) | lb) + 1); // 1 past last instruction
        jumped = true;
    }

    // Subtract with carry
    private void SBC(Instruction instruction, int data)
    {
        var src = ReadMemory(data, instruction.Mode);
        var signedResult = A - src - (C ? 0 : 1);
        // Set overflow
        V = ((A ^ src) & (A ^ signedResult) & 0x80) != 0;
        A = (byte)(A - src - (C ? 0 : 1));
        C = !(signedResult < 0); // Set carry
        SetZN(A);
    }

    // Set carry
    private void SEC(Instruction instruction, int data)
    {
        C = true;
    }

    // Set decimal
    private void SED(Instruction instruction, int data)
    {
        D = true;
    }

    // Set interrupt
    private void SEI(Instruction instruction, int data)
    {
        I = true;
    }

    // Store accumulator
    private void STA(Instruction instruction, int data)
    {
        WriteMemory(data, instruction.Mode, A);
    }

    // Store X register
    private void STX(Instruction instruction, int data)
    {
        WriteMemory(data, instruction.Mode, X);
    }

    // Store Y register
    private void STY(Instruction instruction, int data)
    {
        WriteMemory(data, instruction.Mode, Y);
    }

    // Transfer A to X
    private void TAX(Instruction instruction, int data)
    {
        X = A;
        SetZN(X);
    }

    // Transfer A to Y
    private void TAY(Instruction instruction, int data)
    {
        Y = A;
        SetZN(Y);
    }

    // Transfer stack pointer to X
    private void TSX(Instruction instruction, int data)
    {
        X = SP;
        SetZN(X);
    }

    // Transfer X to A
    private void TXA(Instruction instruction, int data)
    {
        A = X;
        SetZN(A);
    }

    // Transfer X to SP
    private void TXS(Instruction instruction, int data)
    {
        SP = X;
    }

    // Transfer Y to A
    private void TYA(Instruction instruction, int data)
    {
        A = Y;
        SetZN(A);
    }

    private static void Unimplemented(Instruction instruction, int data)
    {
        Debug.WriteLine($"{instruction.Type} is unimplemented.");
    }

    public void Step()
    {
        if (stall > 0)
        {
            stall--;
            CpuTicks++;
            return;
        }

        var opCode = ReadMemory(PC, MemMode.Absolute);
        pageCrossed = false;
        jumped = false;
        var instruction = instructions[opCode];
        var data = 0;
        for (var i = 1; i < instruction.Length; i++)
        {
            data |= ReadMemory(PC + i, MemMode.Absolute) << ((i - 1) * 8);
        }

        instruction.Method(instruction, data);

        if (!jumped)
        {
            PC += (ushort)instruction.Length;
        }
        else if (instruction.Type is
            InstructionType.BCC or InstructionType.BCS or
            InstructionType.BEQ or InstructionType.BMI or
            InstructionType.BNE or InstructionType.BPL or
            InstructionType.BVC or InstructionType.BVS)
        {
            // Branch instructions are +1 ticks if they succeded
            CpuTicks++;
        }
        CpuTicks += instruction.Ticks;
        if (pageCrossed)
        {
            CpuTicks += instruction.PageTicks;
        }
    }

    private ushort AddressForMode(int data, MemMode mode)
    {
        static bool DifferentPages(int address1, int address2) => (address1 & 0xFF00) != (address2 & 0xFF00);

        ushort address = 0;
        switch (mode)
        {
            case MemMode.Absolute:
                address = (ushort)data;
                break;
            case MemMode.AbsoluteX:
                address = (ushort)(data + X);
                pageCrossed = DifferentPages(address, address - X);
                break;
            case MemMode.AbsoluteY:
                address = (ushort)(data + Y);
                pageCrossed = DifferentPages(address, address - Y);
                break;
            case MemMode.IndexedIndirect:
                // 0xFF for zero-page wrapping in next two lines
                {
                    var ls = ram[(data + X) & 0xFF];
                    var ms = ram[(data + X + 1) & 0xFF];
                    address = (ushort)((ms << 8) | ls);
                }
                break;
            case MemMode.Indirect:
                {
                    var ls = ram[data];
                    var ms = ram[data + 1];
                    if ((data & 0xFF) == 0xFF)
                    {
                        ms = ram[data & 0xFF00];
                    }
                    address = (ushort)((ms << 8) | ls);
                }
                break;
            case MemMode.IndirectIndexed:
                // 0xFF for zero-page wrapping in next two lines
                {
                    var ls = ram[data & 0xFF];
                    var ms = ram[(data + 1) & 0xFF];
                    address = (ushort)((ms << 8) | ls);
                    address = (ushort)(address + Y);
                    pageCrossed = DifferentPages(address, address - Y);
                }
                break;
            case MemMode.Relative:
                address = data < 0x80
                    ? (ushort)(PC + 2 + data)
                    : (ushort)(PC + 2 + (data - 256)); // signed
                break;
            case MemMode.Zeropage:
                address = (ushort)data;
                break;
            case MemMode.ZeropageX:
                address = (ushort)((data + X) & 0xFF);
                break;
            case MemMode.ZeropageY:
                address = (ushort)((data + Y) & 0xFF);
                break;
        }

        return address;
    }

    private byte ReadMemory(int location, MemMode mode)
    {
        if (mode == MemMode.Immediate)
        {
            return (byte)location; // Location is actually data in this case
        }
        var address = AddressForMode(location, mode);

        // Memory map at https://www.nesdev.org/wiki/CPU_memory_map
        switch (address) // Main RAM 2KB goes up to 0x800
        {
            case < 0x2000:
                return ram[address % 0x800]; // Mirrors for next  6KB
            case < 0x4000:
                {
                    var temp = (address % 8) | 0x2000; // Get data from PPU register
                    return ppu.ReadRegister(temp);
                }
            case 0x4016:
                if (Joypad1.Strobe)
                {
                    return (byte)(Joypad1.A ? 1 : 0);
                }
                Joypad1.ReadCount++;
                return (byte)(Joypad1.ReadCount switch
                {
                    1 => 0x40 | (Joypad1.A ? 1 : 0),
                    2 => 0x40 | (Joypad1.B ? 1 : 0),
                    3 => 0x40 | (Joypad1.Select ? 1 : 0),
                    4 => 0x40 | (Joypad1.Start ? 1 : 0),
                    5 => 0x40 | (Joypad1.Up ? 1 : 0),
                    6 => 0x40 | (Joypad1.Down ? 1 : 0),
                    7 => 0x40 | (Joypad1.Left ? 1 : 0),
                    8 => 0x40 | (Joypad1.Right ? 1 : 0),
                    _ => 0x41
                });
            case < 0x6000:
                return 0; // Unimplemented other kinds of IO
            default: // Addresses from 0x6000 to 0xFFFF are from the cartridge
                return rom.ReadCartridge(address);
        }
    }

    private void WriteMemory(int location, MemMode mode, byte value)
    {
        if (mode == MemMode.Immediate)
        {
            ram[location] = value;
            return;
        }

        var address = AddressForMode(location, mode);
        // Memory map at https://www.nesdev.org/wiki/CPU_memory_map
        switch (address) // Main RAM 2KB goes up to 0x800
        {
            case < 0x2000:
                ram[address % 0x800] = value; // mirrors for next  6KB
                break;
            case < 0x3FFF:
                {
                    var temp = address % 8 | 0x2000; // Write data to PPU register
                    ppu.WriteRegister(temp, value);
                    break;
                }

            case 0x4014:
                {
                    var fromAddress = value * 0x100; // Address to start copying from
                    for (var i = 0; i < Ppu.SprRamSize; i++) // Copy all 256 bytes to sprite RAM
                    {
                        ppu.Spr[i] = ReadMemory(fromAddress + i, MemMode.Absolute);
                    }
                    // Stall fro 512 cycles while this completes
                    stall = 512;
                    break;
                }

            case 0x4016:
                if (Joypad1.Strobe && (value & 1) == 0)
                {
                    Joypad1.ReadCount = 0;
                }
                Joypad1.Strobe = (value & 1) == 1;
                return;
            case < 0x6000:
                return; // Unimplemented other kinds of IO
            default: // Addresses from 0x6000 to 0xFFFF are from the cartridge
                // We haven't implemented support for cartridge RAM
                rom.WriteCartridge(address, value);
                break;
        }
    }

    private void SetZN(byte value)
    {
        Z = value == 0;
        N = ((value & 0x80) != 0) || (value < 0);
    }

    private void StackPush(byte value)
    {
        ram[0x100 | SP] = value;
        SP--;
    }

    private byte StackPop()
    {
        SP++;
        return ram[0x100 | SP];
    }

    private byte Status
    {
        get =>
            (byte)((C ? 1 : 0) | ((Z ? 1 : 0) << 1) | ((I ? 1 : 0) << 2) | ((D ? 1 : 0) << 3) |
            ((B ? 1 : 0) << 4) | (1 << 5) | ((V ? 1 : 0) << 6) | ((N ? 1 : 0) << 7));
        set
        {
            C = (value & 0b00000001) != 0;
            Z = (value & 0b00000010) != 0;
            I = (value & 0b00000100) != 0;
            D = (value & 0b00001000) != 0;
            // https://www.nesdev.org/the%20'B'%20flag%20&%20BRK%20instruction.txt
            B = false;
            V = (value & 0b01000000) != 0;
            N = (value & 0b10000000) != 0;
        }
    }

    public void TriggerNmi()
    {
        StackPush((byte)(PC >> 8));
        StackPush((byte)PC);
        // https://www.nesdev.org/the%20'B'%20flag%20&%20BRK%20instruction.txt
        B = true;
        StackPush(Status);
        B = false;
        I = true;
        // Set PC to NMI vector
        PC = (ushort)((ReadMemory(NmiVector, MemMode.Absolute)) |
            (ReadMemory(NmiVector + 1, MemMode.Absolute)) << 8);
    }

    public string Log()
    {
        var opCode = ReadMemory(PC, MemMode.Absolute);
        var instruction = instructions[opCode];
        var data1 = instruction.Length < 2
            ? "  "
            : $"{ReadMemory(PC + 1, MemMode.Absolute):X2}";
        var data2 = instruction.Length < 3
            ? "  "
            : $"{ReadMemory(PC + 2, MemMode.Absolute):X2}";

        return $"{PC:X4}  {opCode:X2} {data1} {data2} {instruction.Type}{new string(' ', 29)} A:{A:X2} X:{X:X2} Y:{Y:X2} P:{Status:X2} SP:{SP:X2}";
    }
}