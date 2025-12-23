namespace NESEmulator;

public enum MemMode
{
    Dummy, Absolute, AbsoluteX, AbsoluteY, Accumulator,
    Immediate, Implied, IndexedIndirect, Indirect,
    IndirectIndexed, Relative, Zeropage, ZeropageX,
    ZeropageY
}
