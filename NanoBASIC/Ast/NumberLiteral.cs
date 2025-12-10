namespace NanoBASIC.Ast;

public class NumberLiteral : NumericExpression
{
    public required int Number { get; init; }
}