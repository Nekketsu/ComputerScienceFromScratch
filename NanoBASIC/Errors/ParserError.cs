using NanoBASIC.Tokens;

namespace NanoBASIC.Errors;

public class ParserError(string message, Token token) : NanoBASICError(message, token.LineNum, token.ColStart)
{
    public Token Token { get; } = token;
}