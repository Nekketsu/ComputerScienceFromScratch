using RetroDither;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.CommandLine;

var imageFileArgument = new Argument<string>("image_file") { Description = "Input image file." };
var outputFileArgument = new Argument<string>("output_file") { Description = "Resulting Mac Paint file." };
var pngOption = new Option<bool>("--gif", "-g") { Description = "Create an output gif as well.", DefaultValueFactory = parseResult => false };
var rootCommand = new RootCommand("RetroDither")
{
    imageFileArgument,
    outputFileArgument,
    pngOption
};
rootCommand.SetAction(parseResult =>
{
    var imageFile = parseResult.GetRequiredValue(imageFileArgument);
    var outputFile = parseResult.GetRequiredValue(outputFileArgument);
    var gif = parseResult.GetValue(pngOption);

    var ditherer = new Ditherer();

    var original = ditherer.Prepare(imageFile);
    var dithered = ditherer.Dither(original);

    var macPaint = new MacPaint();
    macPaint.WriteMacpaintFile(dithered, outputFile, original.Width, original.Height);

    if (gif)
    {
        var outImage = Image.LoadPixelData<L8>(dithered, original.Width, original.Height);
        outImage.SaveAsGif($"{Path.GetFileNameWithoutExtension(outputFile)}.gif");
    }

    return 0;
});

var parseResult = rootCommand.Parse(args);
parseResult.Invoke();