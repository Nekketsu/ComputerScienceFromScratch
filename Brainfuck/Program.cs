using System.CommandLine;

var brainfuckFileArgument = new Argument<string>("brainfuck_file") { Description = "A file containing Brainfuck source code." };
var rootCommand = new RootCommand("Brainfuck")
{
    brainfuckFileArgument
};
rootCommand.SetAction(parseResult =>
{
    var brainfuckFile = parseResult.GetRequiredValue(brainfuckFileArgument);

    var brainfuck = new Brainfuck.Brainfuck(brainfuckFile);
    brainfuck.Execute();
    return 0;
});

var parseResult = rootCommand.Parse(args);
parseResult.Invoke();