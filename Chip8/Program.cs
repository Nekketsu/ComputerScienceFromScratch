using Chip8;
using System.CommandLine;
using System.IO;

var romFileArgument = new Argument<string>("rom_file") { Description = "A file containing a Chip-8 game." };
var rootCommand = new RootCommand("Chip8")
{
    romFileArgument
};
rootCommand.SetAction(parseResult =>
{
    var romFile = parseResult.GetRequiredValue(romFileArgument);
    var fileData = File.ReadAllBytes(romFile);

    using var game = new Chip8Game(fileData, romFile);
    game.Run();

    return 0;
});

var parseResult = rootCommand.Parse(args);
parseResult.Invoke();
