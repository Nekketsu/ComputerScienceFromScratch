namespace NESEmulator;

public enum InstructionType
{
    ADC, AHX, ALR, ANC, AND, ARR, ASL, AXS,
    BCC, BCS, BEQ, BIT, BMI, BNE, BPL, BRK,
    BVC, BVS, CLC, CLD, CLI, CLV, CMP, CPX,
    CPY, DCP, DEC, DEX, DEY, EOR, INC, INX,
    INY, ISC, JMP, JSR, KIL, LAS, LAX, LDA,
    LDX, LDY, LSR, NOP, ORA, PHA, PHP, PLA,
    PLP, RLA, ROL, ROR, RRA, RTI, RTS, SAX,
    SBC, SEC, SED, SEI, SHX, SHY, SLO, SRE,
    STA, STX, STY, TAS, TAX, TAY, TSX, TXA,
    TXS, TYA, XAA
}
