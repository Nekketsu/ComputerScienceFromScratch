namespace NanoBASIC.Ast;

public class Statement : Node
{
    public required int LineId { get; init; }
}