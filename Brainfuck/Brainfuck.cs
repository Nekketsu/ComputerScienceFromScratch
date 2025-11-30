namespace Brainfuck;

public class Brainfuck(string fileName)
{
    private string sourceCode = File.ReadAllText(fileName);

    public void Execute()
    {
        var cells = new byte[30000];
        var cellIndex = 0;
        var instructionIndex = 0;

        while (instructionIndex < sourceCode.Length)
        {
            var instruction = sourceCode[instructionIndex];
            switch (instruction)
            {
                case '>':
                    cellIndex++;
                    break;
                case '<':
                    cellIndex--;
                    break;
                case '+':
                    cells[cellIndex]++;
                    break;
                case '-':
                    cells[cellIndex]--;
                    break;
                case '.':
                    Console.Write((char)cells[cellIndex]);
                    break;
                case ',':
                    cells[cellIndex] = byte.Parse(Console.ReadLine()!);
                    break;
                case '[':
                    if (cells[cellIndex] == 0)
                    {
                        instructionIndex = FindBrackedMatch(instructionIndex, true);
                    }
                    break;
                case ']':
                    if (cells[cellIndex] != 0)
                    {
                        instructionIndex = FindBrackedMatch(instructionIndex, false);
                    }
                    break;
            }

            instructionIndex++;
        }
    }

    private int FindBrackedMatch(int start, bool forward)
    {
        var inBetweenBrackets = 0;
        var direction = forward ? 1 : -1;
        var location = start + direction;
        var startBracket = forward ? '[' : ']';
        var endBracket = forward ? ']' : '[';

        while (0 <= location && location < sourceCode.Length)
        {
            if (sourceCode[location] == endBracket)
            {
                if (inBetweenBrackets == 0)
                {
                    return location;
                }
                inBetweenBrackets--;
            }
            else if (sourceCode[location] == startBracket)
            {
                inBetweenBrackets++;
            }
            location += direction;
        }

        Console.WriteLine($"Error: could not find match for {startBracket} at {start}.");
        return start;
    }
}
