using NanoBASIC.Tokens;
using System.Text.RegularExpressions;

namespace NanoBASIC;

public class Tokenizer(string fileName)
{
    private string fileName = fileName;

    private Dictionary<TokenType, string> possibilities = new()
    {
        [TokenType.COMMENT] = @"rem.*",
        [TokenType.WHITESPACE] = @"[ \t\n\r]",
        [TokenType.PRINT] = @"print",
        [TokenType.IF_T] = @"if",
        [TokenType.THEN] = @"then",
        [TokenType.LET] = @"let",
        [TokenType.GOTO] = @"goto",
        [TokenType.GOSUB] = @"gosub",
        [TokenType.RETURN_T] = @"return",
        [TokenType.COMMA] = @",",
        [TokenType.EQUAL] = @"=",
        [TokenType.NOT_EQUAL] = @"<>|><",
        [TokenType.LESS_EQUAL] = @"<=",
        [TokenType.GREATER_EQUAL] = @">=",
        [TokenType.LESS] = @"<",
        [TokenType.GREATER] = @">",
        [TokenType.PLUS] = @"\+",
        [TokenType.MINUS] = @"\-",
        [TokenType.MULTIPLY] = @"\*",
        [TokenType.DIVIDE] = @"/",
        [TokenType.OPEN_PAREN] = @"\(",
        [TokenType.CLOSE_PAREN] = @"\)",
        [TokenType.VARIABLE] = @"[A-Za-z]+",
        [TokenType.NUMBER] = @"-?[0-9]+",
        [TokenType.STRING] = @""".*"""
    };

    public IEnumerable<Token> Tokenize()
    {
        foreach (var (lineNum, wholeLine) in File.ReadAllLines(fileName).Index())
        {
            var line = wholeLine;
            var colStart = 1;

            while (line.Length > 0)
            {
                var found = false;
                foreach (var (tokenType, pattern) in possibilities)
                {
                    var match = Regex.Match(line, '^' + pattern, RegexOptions.IgnoreCase);
                    found = match.Success;

                    if (found)
                    {
                        var colEnd = colStart + match.Length - 1;

                        if (tokenType is not TokenType.WHITESPACE and not TokenType.COMMENT)
                        {
                            var token = tokenType switch
                            {
                                TokenType.NUMBER => new Token<int>(tokenType, lineNum, colStart, colEnd, int.Parse(match.Groups.Values.First().Value)),
                                TokenType.VARIABLE => new Token<string>(tokenType, lineNum, colStart, colEnd, match.Groups.Values.First().Value),
                                TokenType.STRING => new Token<string>(tokenType, lineNum, colStart, colEnd, match.Groups.Values.First().Value[1..^1]),
                                _ => new Token(tokenType, lineNum, colStart, colEnd),
                            };

                            yield return token;
                        }

                        line = line[match.Value.Length..];
                        colStart = colEnd + 1;
                        break;
                    }
                }
                if (!found)
                {
                    Console.WriteLine($"Syntax error no line {lineNum} column {colStart}");
                    break;
                }
            }
        }
    }
}
