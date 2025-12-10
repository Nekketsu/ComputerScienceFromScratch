using NanoBASIC.Tokens;

namespace NanoBASIC.Ast;

public class UnaryOperation : NumericExpression
{
    public required NumericExpression Expr { get; init; }
    public required TokenType Operator { get; init; }

    public override string ToString() => $"({Operator}{Expr})";
}