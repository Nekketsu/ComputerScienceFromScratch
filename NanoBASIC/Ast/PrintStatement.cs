namespace NanoBASIC.Ast;

public class PrintStatement : Statement
{
    public required object[] Printables { get; init; }
}
