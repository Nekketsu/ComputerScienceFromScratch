namespace NanoBASIC.Ast;

public abstract class Node
{
    public required int LineNum { get; init; }
    public required int ColStart { get; init; }
    public required int ColEnd { get; init; }
}