using NanoBASIC;
using System.CommandLine;

var nanoBasicFileArgument = new Argument<string>("basic_file") { Description = "A text file containing NanoBASIC code." };
var rootCommand = new RootCommand("NanoBASIC")
{
    nanoBasicFileArgument
};
rootCommand.SetAction(parseResult =>
{
    var nanoBasicFile = parseResult.GetRequiredValue(nanoBasicFileArgument);

    var executioner = new Executioner();
    executioner.Execute(nanoBasicFile);
    return 0;
});

var parseResult = rootCommand.Parse(args);
parseResult.Invoke();