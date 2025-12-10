namespace NanoBASIC;

public class Executioner
{
    public void Execute(string fileName)
    {
        var tokenizer = new Tokenizer(fileName);
        var tokens = tokenizer.Tokenize().ToArray();

        var parser = new Parser(tokens);
        var ast = parser.Parse().ToArray();

        var interpreter = new Interpreter(ast);
        interpreter.Run();
    }
}
