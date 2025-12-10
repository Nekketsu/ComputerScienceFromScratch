namespace NanoBASIC.Ast;

public class LetStatement : Statement
{
    public required string Name { get; init; }
    public required NumericExpression Expr { get; init; }
}