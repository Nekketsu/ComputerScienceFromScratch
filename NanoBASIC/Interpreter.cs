using NanoBASIC.Ast;
using NanoBASIC.Errors;
using NanoBASIC.Tokens;
using System.Text;

namespace NanoBASIC;

public class Interpreter(Statement[] statements)
{
    private Statement[] statements = statements;
    private Dictionary<string, int> variableTable = [];
    private int statementIndex = 0;
    private Stack<int> subroutineStack = [];

    private Statement Current => statements[statementIndex];

    private int? FindLineIndex(int lineId)
    {
        var low = 0;
        var high = statements.Length - 1;
        while (low <= high)
        {
            var mid = (low + high) / 2;
            if (statements[mid].LineId < lineId)
            {
                low = mid + 1;
            }
            else if (statements[mid].LineId > lineId)
            {
                high = mid - 1;
            }
            else
            {
                return mid;
            }
        }
        return null;
    }

    public void Run()
    {
        while (statementIndex < statements.Length)
        {
            Interpret(Current);
        }
    }

    private void Interpret(Statement statement)
    {
        switch (statement)
        {
            case LetStatement { Name: var name, Expr: var expr }:
                {
                    var value = EvaluateNumeric(expr);
                    variableTable[name] = value;
                    statementIndex += 1;
                    break;
                }
            case GoToStatement { LineExpr: var lineExpr }:
                {
                    var goToLineId = EvaluateNumeric(lineExpr);
                    var lineIndex = FindLineIndex(goToLineId);
                    if (lineIndex is not null)
                    {
                        statementIndex = lineIndex.Value;
                    }
                    else
                    {
                        throw new InterpreterError("No GOTO line id.", Current);
                    }
                    break;
                }
            case GoSubStatement { LineExpr: var lineExpr }:
                {
                    var goSubLineId = EvaluateNumeric(lineExpr);
                    var lineIndex = FindLineIndex(goSubLineId);
                    if (lineIndex is not null)
                    {
                        subroutineStack.Push(statementIndex + 1);
                        statementIndex = lineIndex.Value;
                    }
                    else
                    {
                        throw new InterpreterError("No GOSUB line id.", Current);
                    }
                    break;
                }
            case ReturnStatement:
                if (!subroutineStack.Any())
                {
                    throw new InterpreterError("RETURN without GOSUB.", Current);
                }
                statementIndex = subroutineStack.Pop();
                break;
            case PrintStatement { Printables: var printables }:
                {
                    var accumulatedString = new StringBuilder();
                    foreach (var (index, printable) in printables.Index())
                    {
                        if (index > 0)
                        {
                            accumulatedString.Append('\t');
                        }
                        if (printable is NumericExpression numericExpression)
                        {
                            accumulatedString.Append(EvaluateNumeric(numericExpression));
                        }
                        else if (printable is string @string)
                        {
                            accumulatedString.Append(@string);
                        }
                    }
                    Console.WriteLine(accumulatedString);
                    statementIndex++;
                }
                break;
            case IfStatement { BooleanExpr: var booleanExpr, ThenStatement: var thenStatement }:
                if (EvaluateBoolean(booleanExpr))
                {
                    Interpret(thenStatement);
                }
                else
                {
                    statementIndex += 1;
                }
                break;
            default:
                throw new InterpreterError($"Unexpected item {Current} in statement list.", Current);
        }
    }

    private int EvaluateNumeric(NumericExpression numericExpression) => numericExpression switch
    {
        NumberLiteral { Number: var number } => number,
        VarRetrieve { Name: var name } => variableTable.TryGetValue(name, out var value)
            ? value
            : throw new InterpreterError($"Var {name} used before initialized.", numericExpression),
        UnaryOperation { Operator: var @operator, Expr: var expr } => @operator is Tokens.TokenType.MINUS
            ? -EvaluateNumeric(expr)
            : throw new InterpreterError($"Expected - but got {@operator}.", numericExpression),
        BinaryOperation { Operator: var @operator, LeftExpr: var left, RightExpr: var right } => @operator switch
        {
            TokenType.PLUS => EvaluateNumeric(left) + EvaluateNumeric(right),
            TokenType.MINUS => EvaluateNumeric(left) - EvaluateNumeric(right),
            TokenType.MULTIPLY => EvaluateNumeric(left) * EvaluateNumeric(right),
            TokenType.DIVIDE => EvaluateNumeric(left) / EvaluateNumeric(right),
            _ => throw new InterpreterError($"Unexpected binary operator {@operator}.", numericExpression)
        },
        _ => throw new InterpreterError("Expected numeric expression.", numericExpression)
    };

    private bool EvaluateBoolean(BooleanExpression booleanExpression)
    {
        var left = EvaluateNumeric(booleanExpression.LeftExpr);
        var right = EvaluateNumeric(booleanExpression.RightExpr);

        return booleanExpression.Operator switch
        {
            TokenType.LESS => left < right,
            TokenType.LESS_EQUAL => left <= right,
            TokenType.GREATER => left > right,
            TokenType.GREATER_EQUAL => left >= right,
            TokenType.EQUAL => left == right,
            TokenType.NOT_EQUAL => left != right,
            _ => throw new InterpreterError($"Unexpected boolean operator {booleanExpression.Operator}.", booleanExpression)
        };
    }
}
