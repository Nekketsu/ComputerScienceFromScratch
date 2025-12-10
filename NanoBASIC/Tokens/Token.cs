namespace NanoBASIC.Tokens;

public class Token(TokenType kind, int lineNum, int colStart, int colEnd)
{
    public TokenType Kind { get; } = kind;
    public int LineNum { get; } = lineNum;
    public int ColStart { get; } = colStart;
    public int ColEnd { get; } = colEnd;
}

public class Token<T>(TokenType kind, int lineNum, int colStart, int colEnd, T associatedValue) : Token(kind, lineNum, colStart, colEnd)
{
    public T AssociatedValue { get; } = associatedValue;
}
