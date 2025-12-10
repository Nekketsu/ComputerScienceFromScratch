namespace NanoBASIC.Ast;

public class GoSubStatement : Statement
{
    public required NumericExpression LineExpr { get; init; }
}