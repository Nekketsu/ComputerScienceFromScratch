using Impressionist;
using System.CommandLine;

var imageFileArgument = new Argument<string>("image_file") { Description = "The input image" };
var outputFileArgument = new Argument<string>("output_file") { Description = "The resulting abstract art" };
var trialsOption = new Option<int>("--trials", "-t") { Description = "The number of trials to run (default 10000).", DefaultValueFactory = parseResult => 10000 };
var methodOption = new Option<ColorMethod>("--method", "-t") { Description = "Shape color determination method (default average).", DefaultValueFactory = parseResult => ColorMethod.Average };
var shapeOption = new Option<ShapeType>("--shape", "-s") { Description = "The shape type (default ellipse).", DefaultValueFactory = parseResult => ShapeType.Ellipse };
var lengthOption = new Option<int>("--length", "-l") { Description = "The length of the final image in pixels (default 356).", DefaultValueFactory = parseResult => 256 };
var vectorOption = new Option<bool>("--vector", "-v") { Description = "Create vector output. A SVG file will also be output.", DefaultValueFactory = parseResult => false };
var animateOption = new Option<int>("--animate", "-a") { Description = "If greater than 0, will create an animated GIF with the number of milliseconds per frame provided." };
var rootCommand = new RootCommand("Impressionist")
{
    imageFileArgument,
    outputFileArgument,
    trialsOption,
    methodOption,
    shapeOption,
    lengthOption,
    vectorOption,
    animateOption
};
rootCommand.SetAction(parseResult =>
{
    var imageFile = parseResult.GetRequiredValue(imageFileArgument);
    var outputFile = parseResult.GetRequiredValue(outputFileArgument);
    var trials = parseResult.GetValue(trialsOption);
    var method = parseResult.GetValue(methodOption);
    var shape = parseResult.GetValue(shapeOption);
    var length = parseResult.GetValue(lengthOption);
    var vector = parseResult.GetValue(vectorOption);
    var animate = parseResult.GetValue(animateOption);

    var impressionist = new Impressionist.Impressionist(imageFile, outputFile, trials, method, shape, length, vector, animate);
    impressionist.Create();

    return 0;
});

var parseResult = rootCommand.Parse(args);
parseResult.Invoke();

public enum ColorMethod
{
    Random,
    Average,
    Common
}

public enum ShapeType
{
    Ellipse,
    Triangle,
    Quadrilateral,
    Line
}