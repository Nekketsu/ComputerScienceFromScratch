using System;

namespace NESEmulator;

public class Instruction(
    InstructionType type,
    Action<Instruction, int> method,
    MemMode mode,
    int length,
    int ticks,
    int pageTicks)
{
    public InstructionType Type { get; } = type;
    public Action<Instruction, int> Method { get; } = method;
    public MemMode Mode { get; } = mode;
    public int Length { get; } = length;
    public int Ticks { get; } = ticks;
    public int PageTicks { get; } = pageTicks;
}
