using NESEmulator;
using System.CommandLine;

var romFileArgument = new Argument<string>("rom_file") { Description = "An NES game file in iNES format." };
var rootCommand = new RootCommand("NESEmulator")
{
    romFileArgument
};
rootCommand.SetAction(parseResult =>
{
    var romFile = parseResult.GetRequiredValue(romFileArgument);
    var rom = new Rom(romFile);

    using var game = new NESEmulatorGame(rom, romFile);
    game.Run();

    return 0;
});

var parseResult = rootCommand.Parse(args);
parseResult.Invoke();