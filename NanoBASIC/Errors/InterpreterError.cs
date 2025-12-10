using NanoBASIC.Ast;

namespace NanoBASIC.Errors;

public class InterpreterError(string message, Node node) : NanoBASICError(message, node.LineNum, node.ColStart)
{
}