namespace NanoBASIC.Errors;

public class NanoBASICError(string message, int lineNum, int column) : Exception(message)
{
    public int LineNum { get; } = lineNum;
    public int Column { get; } = column;

    public override string ToString() => $"{Message} Ocurred at line {LineNum} and column {Column}";
}