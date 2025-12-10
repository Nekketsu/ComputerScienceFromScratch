namespace NanoBASIC.Tests;

public class NanoBASICTests
{
    private const string exampleFolder = "Examples";

    [Theory]
    [InlineData("print1.bas", "Hello World\n")]
    [InlineData("print2.bas", "4\n12\n30\n7\n100\t9\n")]
    [InlineData("print3.bas", "E is\t-31\n")]
    [InlineData("variables.bas", "15\n")]
    [InlineData("goto.bas", "Josh\nDave\nNanoBASIC ROCKS\n")]
    [InlineData("gosub.bas", "10\n")]
    [InlineData("if1.bas", "10\n40\n50\n60\n70\n100\n")]
    [InlineData("if2.bas", "GOOD\n")]
    [InlineData("fib.bas", "0\n1\n1\n2\n3\n5\n8\n13\n21\n34\n55\n89\n")]
    [InlineData("factorial.bas", "120\n")]
    [InlineData("gcd.bas", "7\n")]
    public void Test(string test, string expected)
    {
        var programOutput = Run(Path.Combine(exampleFolder, test));
        Assert.Equal(expected.ReplaceLineEndings(), programOutput.ReplaceLineEndings());
    }

    private static string Run(string fileName)
    {
        var sw = new StringWriter();
        Console.SetOut(sw);

        var executioner = new Executioner();
        executioner.Execute(fileName);

        return sw.ToString();
    }
}
