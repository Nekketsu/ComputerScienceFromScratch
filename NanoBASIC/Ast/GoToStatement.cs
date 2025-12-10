namespace NanoBASIC.Ast;

public class GoToStatement : Statement
{
    public required NumericExpression LineExpr { get; init; }
}