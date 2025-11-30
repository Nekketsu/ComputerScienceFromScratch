namespace Brainfuck.Tests;

public class BrainfuckTests
{
    private const string exampleFolder = "Examples";

    [Theory]
    [InlineData("hello_world_verbose.bf", "Hello World!\n")]
    [InlineData("fibonacci.bf", "1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89")]
    [InlineData("cell_size.bf", "8 bit cells\n")]
    public void Test(string test, string expected)
    {
        var programOutput = Run(Path.Combine(exampleFolder, test));
        Assert.Equal(expected.ReplaceLineEndings(), programOutput.ReplaceLineEndings());
    }

    [Fact]
    public void TestBeer()
    {
        var programOutput = Run(Path.Combine(exampleFolder, "beer.bf"));
        var expected = File.ReadAllText(Path.Combine(exampleFolder, "beer.out"));
        Assert.Equal(expected.ReplaceLineEndings(), programOutput.ReplaceLineEndings());
    }

    private string Run(string fileName)
    {
        var sw = new StringWriter();
        Console.SetOut(sw);

        var brainfuck = new Brainfuck(fileName);
        brainfuck.Execute();

        return sw.ToString();
    }
}
