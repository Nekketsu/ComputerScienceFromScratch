using NanoBASIC.Tokens;

namespace NanoBASIC.Ast;

public class BinaryOperation : NumericExpression
{
    public required TokenType Operator { get; init; }
    public required NumericExpression LeftExpr { get; init; }
    public required NumericExpression RightExpr { get; init; }

    public override string ToString() => $"({LeftExpr} {Operator} {RightExpr})";
}