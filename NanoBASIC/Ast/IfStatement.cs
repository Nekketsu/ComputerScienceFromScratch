namespace NanoBASIC.Ast;

public class IfStatement : Statement
{
    public required BooleanExpression BooleanExpr { get; init; }
    public required Statement ThenStatement { get; init; }
}