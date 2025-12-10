namespace NanoBASIC.Ast;

public class VarRetrieve : NumericExpression
{
    public required string Name { get; init; }
}