using NanoBASIC.Ast;
using NanoBASIC.Errors;
using NanoBASIC.Tokens;

namespace NanoBASIC;

public class Parser(Token[] tokens)
{
    private Token[] tokens { get; } = tokens;
    private int tokenIndex = 0;

    private bool OutOfTokens => tokenIndex >= tokens.Length;
    private Token Current => OutOfTokens
        ? throw new ParserError($"No tokens after {Previous.Kind}", Previous)
        : tokens[tokenIndex];
    private Token Previous => tokens[tokenIndex - 1];

    private Token Consume(TokenType kind)
    {
        if (Current.Kind == kind)
        {
            tokenIndex++;
            return Previous;
        }
        throw new ParserError($"Expected {kind} after {Previous} but got {Current}.", Current);
    }

    private Token<T> Consume<T>(TokenType kind) => (Token<T>)Consume(kind);

    public IEnumerable<Statement> Parse()
    {
        while (!OutOfTokens)
        {
            var statement = ParseLine();
            yield return statement;
        }
    }

    private Statement ParseLine()
    {
        var number = Consume<int>(TokenType.NUMBER);
        return ParseStatement(number.AssociatedValue);
    }

    private Statement ParseStatement(int lineId) => Current.Kind switch
    {
        TokenType.PRINT => ParsePrint(lineId),
        TokenType.IF_T => ParseIf(lineId),
        TokenType.LET => ParseLet(lineId),
        TokenType.GOTO => ParseGoto(lineId),
        TokenType.GOSUB => ParseGosub(lineId),
        TokenType.RETURN_T => ParseReturn(lineId),
        _ => throw new ParserError("Expected to find start of statement.", Current)
    };

    private PrintStatement ParsePrint(int lineId)
    {
        var printToken = Consume(TokenType.PRINT);
        var printables = new List<object>();
        var lastCol = printToken.ColEnd;
        while (true)
        {
            if (Current.Kind is TokenType.STRING)
            {
                var @string = Consume<string>(TokenType.STRING);
                printables.Add(@string.AssociatedValue);
                lastCol = @string.ColEnd;
            }
            else
            {
                var expression = ParseNumericExpression();
                if (expression is not null)
                {
                    printables.Add(expression);
                    lastCol = expression.ColEnd;
                }
                else
                {
                    throw new ParserError("Only string and numeric expressions allowd in print list.", Current);
                }
            }
            if (!OutOfTokens && Current.Kind is TokenType.COMMA)
            {
                Consume(TokenType.COMMA);
                continue;
            }
            break;
        }
        return new() { LineId = lineId, LineNum = printToken.LineNum, ColStart = printToken.ColStart, ColEnd = lastCol, Printables = [.. printables] };
    }

    private IfStatement ParseIf(int lineId)
    {
        var ifToken = Consume(TokenType.IF_T);
        var booleanExpression = ParseBooleanExpression();
        Consume(TokenType.THEN);
        var statement = ParseStatement(lineId);
        return new() { LineId = lineId, LineNum = ifToken.LineNum, ColStart = ifToken.ColStart, ColEnd = statement.ColEnd, BooleanExpr = booleanExpression, ThenStatement = statement };
    }

    private LetStatement ParseLet(int lineId)
    {
        var letToken = Consume(TokenType.LET);
        var variable = Consume<string>(TokenType.VARIABLE);
        Consume(TokenType.EQUAL);
        var expression = ParseNumericExpression();
        return new() { LineId = lineId, LineNum = letToken.LineNum, ColStart = letToken.ColStart, ColEnd = expression.ColEnd, Name = variable.AssociatedValue, Expr = expression };
    }

    private GoToStatement ParseGoto(int lineId)
    {
        var gotoToken = Consume(TokenType.GOTO);
        var expression = ParseNumericExpression();
        return new() { LineId = lineId, LineNum = gotoToken.LineNum, ColStart = gotoToken.ColStart, ColEnd = expression.ColEnd, LineExpr = expression };
    }

    private GoSubStatement ParseGosub(int lineId)
    {
        var gosubToken = Consume(TokenType.GOSUB);
        var expression = ParseNumericExpression();
        return new GoSubStatement { LineId = lineId, LineNum = gosubToken.LineNum, ColStart = gosubToken.ColStart, ColEnd = expression.ColEnd, LineExpr = expression };
    }

    private ReturnStatement ParseReturn(int lineId)
    {
        var returnToken = Consume(TokenType.RETURN_T);
        return new() { LineId = lineId, LineNum = returnToken.LineNum, ColStart = returnToken.ColStart, ColEnd = returnToken.ColEnd };
    }

    private BooleanExpression ParseBooleanExpression()
    {
        var left = ParseNumericExpression();
        if (Current.Kind is TokenType.GREATER or TokenType.GREATER_EQUAL or TokenType.EQUAL or TokenType.LESS or TokenType.LESS_EQUAL or TokenType.NOT_EQUAL)
        {
            var @operator = Consume(Current.Kind);
            var right = ParseNumericExpression();
            return new() { LineNum = left.LineNum, ColStart = left.ColStart, ColEnd = right.ColEnd, Operator = @operator.Kind, LeftExpr = left, RightExpr = right };
        }
        throw new ParserError($"Expected boolean operator but found {Current.Kind}.", Current);
    }

    private NumericExpression ParseNumericExpression()
    {
        var left = ParseTerm();
        while (true)
        {
            if (OutOfTokens)
            {
                return left;
            }
            if (Current.Kind is TokenType.PLUS)
            {
                Consume(TokenType.PLUS);
                var right = ParseTerm();
                left = new BinaryOperation { LineNum = left.LineNum, ColStart = left.ColStart, ColEnd = right.ColEnd, Operator = TokenType.PLUS, LeftExpr = left, RightExpr = right };
            }
            else if (Current.Kind is TokenType.MINUS)
            {
                Consume(TokenType.MINUS);
                var right = ParseTerm();
                left = new BinaryOperation { LineNum = left.LineNum, ColStart = left.ColStart, ColEnd = right.ColEnd, Operator = TokenType.MINUS, LeftExpr = left, RightExpr = right };
            }
            else
            {
                break;
            }
        }
        return left;
    }

    private NumericExpression ParseTerm()
    {
        var left = ParseFactor();
        while (true)
        {
            if (OutOfTokens)
            {
                return left;
            }
            if (Current.Kind is TokenType.MULTIPLY)
            {
                Consume(TokenType.MULTIPLY);
                var right = ParseFactor();
                left = new BinaryOperation { LineNum = left.LineNum, ColStart = left.ColStart, ColEnd = right.ColEnd, Operator = TokenType.MULTIPLY, LeftExpr = left, RightExpr = right };
            }
            else if (Current.Kind is TokenType.DIVIDE)
            {
                Consume(TokenType.DIVIDE);
                var right = ParseFactor();
                left = new BinaryOperation { LineNum = left.LineNum, ColStart = left.ColStart, ColEnd = right.ColEnd, Operator = TokenType.DIVIDE, LeftExpr = left, RightExpr = right };
            }
            else
            {
                break;
            }
        }
        return left;
    }

    NumericExpression ParseFactor()
    {
        if (Current.Kind is TokenType.VARIABLE)
        {
            var variable = Consume<string>(TokenType.VARIABLE);
            return new VarRetrieve { LineNum = variable.LineNum, ColStart = variable.ColStart, ColEnd = variable.ColEnd, Name = variable.AssociatedValue };
        }
        else if (Current.Kind is TokenType.NUMBER)
        {
            var number = Consume<int>(TokenType.NUMBER);
            return new NumberLiteral { LineNum = number.LineNum, ColStart = number.ColStart, ColEnd = number.ColEnd, Number = number.AssociatedValue };
        }
        else if (Current.Kind is TokenType.OPEN_PAREN)
        {
            Consume(TokenType.OPEN_PAREN);
            var expression = ParseNumericExpression();
            if (Current.Kind is not TokenType.CLOSE_PAREN)
            {
                throw new ParserError("Expected matching close parenthesis.", Current);
            }
            Consume(TokenType.CLOSE_PAREN);
            return expression;
        }
        else if (Current.Kind is TokenType.MINUS)
        {
            var minus = Consume(TokenType.MINUS);
            var expression = ParseFactor();
            return new UnaryOperation { LineNum = minus.LineNum, ColStart = minus.ColStart, ColEnd = expression.ColEnd, Operator = TokenType.MINUS, Expr = expression };
        }
        throw new ParserError("Unexpected token in numeric expression.", Current);
    }
}
